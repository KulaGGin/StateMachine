using System;
using System.Collections.Generic;
using Kulagin.StateMachine.Core;
using NUnit.Framework;

namespace Kulagin.StateMachine.Unity.Tests {
    public class UnityStateMachineInterfaceTests {
        private class SkaterMachine : UnityStateMachine<SkaterMachine, SkaterState> {
        }

        private abstract class SkaterState : UnityState<SkaterMachine, SkaterState> {
            protected SkaterState(SkaterMachine StateMachine) : base(StateMachine) {
            }
        }

        private class SkatingState : SkaterState {
            public int Frames;
            public SkatingState(SkaterMachine StateMachine) : base(StateMachine) {
            }

            public override void FrameUpdate() {
                Frames++;
            }
        }

        [Test]
        public void FlatUnityMachine_PumpedThroughInterface_ForwardsToCurrentState() {
            SkaterMachine Machine = new();
            SkatingState Skating = new(Machine);
            Machine.SetStates(Skating);
            Machine.StartStateMachine<SkatingState>();

            IUnityStateMachine Pump = Machine;
            Pump.FrameUpdate();

            Assert.AreEqual(1, Skating.Frames);
        }
        
        private class BrainMachine : HierarchicalUnityStateMachine<BrainMachine, BrainState> {
            public List<string> Log = new();
        }

        private abstract class BrainState : HierarchicalUnityState<BrainMachine, BrainState> {
            protected BrainState(BrainMachine StateMachine) : base(StateMachine) {
            }

            public override void PhysicsUpdate() {
                StateMachine.Log.Add($"Physics:{GetType().Name}");
            }
        }

        private class DefendState : BrainState {
            public DefendState(BrainMachine StateMachine) : base(StateMachine) {
            }
        }

        private class MarkManState : BrainState {
            public MarkManState(BrainMachine StateMachine) : base(StateMachine) {
            }
        }

        [Test]
        public void HierarchicalUnityMachine_PumpedThroughInterface_BroadcastsLeafToRoot() {
            BrainMachine Machine = new();
            DefendState Defend = new(Machine);
            MarkManState MarkMan = new(Machine);
            Machine.SetStates(Defend, MarkMan);
            Machine.SetParent<MarkManState, DefendState>();
            Machine.StartStateMachine<MarkManState>();

            IUnityStateMachine Pump = Machine;      // same handle type as the flat machine in Test 4
            Pump.PhysicsUpdate();

            Assert.AreEqual(new[] { "Physics:MarkManState", "Physics:DefendState" }, Machine.Log);
        }
        
        private readonly struct PenaltyEvent { }

        private class ForecheckState : BrainState, IHandle<PenaltyEvent> {
            public ForecheckState(BrainMachine StateMachine) : base(StateMachine) {
            }

            public bool Handle(PenaltyEvent Event) {
                return false;      // not mine — let it bubble to the parent
            }
        }

        private class DisciplineState : BrainState, IHandle<PenaltyEvent> {
            public bool Handled;
            public DisciplineState(BrainMachine StateMachine) : base(StateMachine) {
            }

            public bool Handle(PenaltyEvent Event) {
                Handled = true;
                return true;
            }
        }

        [Test]
        public void Send_ThroughUnityHandle_OnHierarchicalMachine_StillBubbles() {
            BrainMachine Machine = new();
            DisciplineState Discipline = new(Machine);       // parent — handles it
            ForecheckState Forecheck = new(Machine);         // leaf — defers
            Machine.SetStates(Discipline, Forecheck);
            Machine.SetParent<ForecheckState, DisciplineState>();
            Machine.StartStateMachine<ForecheckState>();

            IUnityStateMachine Bus = Machine;
            bool Result = Bus.Send(new PenaltyEvent());

            Assert.IsTrue(Result);              // bubbled leaf → parent
            Assert.IsTrue(Discipline.Handled);
        }
        
        private class FieldPlayerMachine : UnityStateMachine<FieldPlayerMachine, FieldPlayerState> {
        }

        private abstract class FieldPlayerState : UnityState<FieldPlayerMachine, FieldPlayerState> {
            protected FieldPlayerState(FieldPlayerMachine StateMachine) : base(StateMachine) {
            }
        }

        private class ChasePuckState : FieldPlayerState, IHandle<PenaltyEvent> {
            public int Frames;
            public bool Penalized;
            public ChasePuckState(FieldPlayerMachine StateMachine) : base(StateMachine) {
            }

            public override void FrameUpdate() { Frames++; }
            public bool Handle(PenaltyEvent Event) { Penalized = true; return true; }
        }

        private class GoalkeeperMachine : UnityStateMachine<GoalkeeperMachine, GoalkeeperState> {
        }

        private abstract class GoalkeeperState : UnityState<GoalkeeperMachine, GoalkeeperState> {
            protected GoalkeeperState(GoalkeeperMachine StateMachine) : base(StateMachine) {
            }
        }

        private class GuardNetState : GoalkeeperState, IHandle<PenaltyEvent> {
            public int Frames;
            public bool Penalized;
            public GuardNetState(GoalkeeperMachine StateMachine) : base(StateMachine) {
            }

            public override void FrameUpdate() { Frames++; }
            public bool Handle(PenaltyEvent Event) { Penalized = true; return true; }
        }

        [Test]
        public void DifferentMachineTypes_ShareOneList_AndRespondThroughTheInterface() {
            FieldPlayerMachine Field = new();
            ChasePuckState Chase = new(Field);
            Field.SetStates(Chase);
            Field.StartStateMachine<ChasePuckState>();

            GoalkeeperMachine Goalie = new();
            GuardNetState Guard = new(Goalie);
            Goalie.SetStates(Guard);
            Goalie.StartStateMachine<GuardNetState>();

            // The line that was impossible before: two unrelated closed generics, one list.
            List<IUnityStateMachine> Roster = new() { Field, Goalie };

            foreach (IUnityStateMachine Player in Roster) {
                Player.FrameUpdate();
                Player.Send(new PenaltyEvent());
            }

            Assert.AreEqual(1, Chase.Frames);
            Assert.AreEqual(1, Guard.Frames);
            Assert.IsTrue(Chase.Penalized);
            Assert.IsTrue(Guard.Penalized);
        }
    }
}