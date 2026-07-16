using System.Collections.Generic;
using NUnit.Framework;

namespace Kulagin.StateMachine.Core.Tests {
    public class HierarchicalStateMachineTests {
        private class TestStateMachine : HierarchicalStateMachine<TestStateMachine, TestState> {
            public List<string> Log = new();
        }

        private abstract class TestState : HierarchicalState<TestStateMachine, TestState> {
            protected TestState(TestStateMachine StateMachine) : base(StateMachine) {
            }

            public override object EnterState(object StateEventArgs = null) {
                StateMachine.Log.Add($"Enter:{GetType().Name}");
                return StateEventArgs;
            }

            public override void ExitState() {
                StateMachine.Log.Add($"Exit:{GetType().Name}");
            }
        }

        private class IdleState : TestState {
            public IdleState(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }

        private class WalkingState : TestState {
            public WalkingState(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }
        
        private class ParentState : TestState {
            public ParentState(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }

        private class ChildState : TestState {
            public ChildState(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }
        
        private class OtherState : TestState {
            public OtherState(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }
        
        private class ReroutingState : TestState {
            public ReroutingState(TestStateMachine StateMachine) : base(StateMachine) {
            }

            public override object EnterState(object StateEventArgs = null) {
                base.EnterState(StateEventArgs);
                StateMachine.ApplyState<IdleState>();
                return StateEventArgs;
            }
        }

        [Test]
        public void StartStateMachine_OnFlatState_SetsCurrentStateAndCallsEnter() {
            TestStateMachine StateMachine = new();
            IdleState IdleState = new(StateMachine);
            StateMachine.SetStates(IdleState);

            StateMachine.StartStateMachine<IdleState>();

            Assert.AreEqual(IdleState, StateMachine.CurrentState);
            Assert.AreEqual(new[] { "Enter:IdleState" }, StateMachine.Log);
        }
        
        [Test]
        public void StartStateMachine_OnChildState_EntersChildBeforeParent() {   // was: EntersParentBeforeChild
            TestStateMachine StateMachine = new();
            ParentState ParentState = new(StateMachine);
            ChildState ChildState = new(StateMachine);
            StateMachine.SetStates(ParentState, ChildState);
            StateMachine.SetParent<ChildState, ParentState>();

            StateMachine.StartStateMachine<ChildState>();

            Assert.AreEqual(new[] { "Enter:ChildState", "Enter:ParentState" }, StateMachine.Log);
        }
        
        [Test]
        public void ApplyState_FromChildState_ExitsChildBeforeParent() {
            TestStateMachine StateMachine = new();
            ParentState ParentState = new(StateMachine);
            ChildState ChildState = new(StateMachine);
            OtherState OtherState = new(StateMachine);
            StateMachine.SetStates(ParentState, ChildState, OtherState);
            StateMachine.SetParent<ChildState, ParentState>();
            StateMachine.StartStateMachine<ChildState>();
            StateMachine.Log.Clear();   // throw away the start noise; we are testing the transition

            StateMachine.ApplyState<OtherState>();

            Assert.AreEqual(
                new[] { "Exit:ChildState", "Exit:ParentState", "Enter:OtherState" },
                StateMachine.Log);
        }
        
        private class ChildAState : TestState {
            public ChildAState(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }

        private class ChildBState : TestState {
            public ChildBState(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }

        [Test]
        public void ApplyState_BetweenSiblings_DoesNotReExitSharedParent() {
            TestStateMachine StateMachine = new();
            ParentState ParentState = new(StateMachine);
            ChildAState ChildAState = new(StateMachine);
            ChildBState ChildBState = new(StateMachine);
            StateMachine.SetStates(ParentState, ChildAState, ChildBState);
            StateMachine.SetParent<ChildAState, ParentState>();
            StateMachine.SetParent<ChildBState, ParentState>();
            StateMachine.StartStateMachine<ChildAState>();
            StateMachine.Log.Clear();

            StateMachine.ApplyState<ChildBState>();

            Assert.AreEqual(
                new[] { "Exit:ChildAState", "Enter:ChildBState" },
                StateMachine.Log);
        }
        
        
        private class RootState : TestState {
            public RootState(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }

        private class Branch1State : TestState {
            public Branch1State(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }

        private class Branch2State : TestState {
            public Branch2State(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }

        private class Leaf1State : TestState {
            public Leaf1State(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }

        private class Leaf2State : TestState {
            public Leaf2State(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }

        [Test]
        public void ApplyState_BetweenCousins_ExitsUpToLowestCommonAncestorThenEntersDown() {
            TestStateMachine StateMachine = new();
            RootState RootState = new(StateMachine);
            Branch1State Branch1State = new(StateMachine);
            Branch2State Branch2State = new(StateMachine);
            Leaf1State Leaf1State = new(StateMachine);
            Leaf2State Leaf2State = new(StateMachine);
            StateMachine.SetStates(RootState, Branch1State, Branch2State, Leaf1State, Leaf2State);
            StateMachine.SetParent<Branch1State, RootState>();
            StateMachine.SetParent<Branch2State, RootState>();
            StateMachine.SetParent<Leaf1State, Branch1State>();
            StateMachine.SetParent<Leaf2State, Branch2State>();
            StateMachine.StartStateMachine<Leaf1State>();
            StateMachine.Log.Clear();

            StateMachine.ApplyState<Leaf2State>();

            Assert.AreEqual(new[] {
                "Exit:Leaf1State", "Exit:Branch1State",
                "Enter:Leaf2State", "Enter:Branch2State"          // was: Enter:Branch2State, Enter:Leaf2State
            }, StateMachine.Log);
        }
        
        private class HandlingParentState : TestState {
            public bool HandledEvent;

            public HandlingParentState(TestStateMachine StateMachine) : base(StateMachine) {
            }

            public override bool HandleEvent(object Event) {
                HandledEvent = true;
                StateMachine.Log.Add($"HandleEvent:{GetType().Name}:{Event}");
                return true;
            }
        }

        private class NonHandlingChildState : TestState {
            public NonHandlingChildState(TestStateMachine StateMachine) : base(StateMachine) {
            }

            public override bool HandleEvent(object Event) {
                StateMachine.Log.Add($"HandleEvent:{GetType().Name}:{Event}");
                return false;   // not mine — bubble up
            }
        }
        
        private class HandlingChildState : TestState {
            public HandlingChildState(TestStateMachine StateMachine) : base(StateMachine) {
            }

            public override bool HandleEvent(object Event) {
                StateMachine.Log.Add($"HandleEvent:{GetType().Name}:{Event}");
                return true;
            }
        }
        
        
        private class ArgParentState : TestState {
            public object ReceivedArg;
            public ArgParentState(TestStateMachine SM) : base(SM) {}
            public override object EnterState(object StateEventArgs = null) {
                ReceivedArg = StateEventArgs;
                StateMachine.Log.Add($"Enter:{GetType().Name}");
                return StateEventArgs;
            }
        }

        private class ArgChildState : TestState {
            public ArgChildState(TestStateMachine SM) : base(SM) {}
            public override object EnterState(object StateEventArgs = null) {
                StateMachine.Log.Add($"Enter:{GetType().Name}");
                return $"{StateEventArgs}-fromchild";   // transform, then hand up
            }
        }

        [Test]
        public void StartStateMachine_OnChildState_ChildReturnedArgReachesParent() {
            TestStateMachine StateMachine = new();
            ArgParentState ParentState = new(StateMachine);
            ArgChildState ChildState = new(StateMachine);
            StateMachine.SetStates(ParentState, ChildState);
            StateMachine.SetParent<ArgChildState, ArgParentState>();

            StateMachine.StartStateMachine<ArgChildState>();

            Assert.AreEqual("-fromchild", ParentState.ReceivedArg);   // seed null → "" → "-fromchild"
            Assert.AreEqual(new[] { "Enter:ArgChildState", "Enter:ArgParentState" }, StateMachine.Log);
        }
        
        [Test]
        public void ApplyState_OnChildState_TransformsArgUpThePath() {
            TestStateMachine StateMachine = new();
            ArgParentState ParentState = new(StateMachine);
            ArgChildState ChildState = new(StateMachine);
            IdleState IdleState = new(StateMachine);
            StateMachine.SetStates(ParentState, ChildState, IdleState);
            StateMachine.SetParent<ArgChildState, ArgParentState>();
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.Log.Clear();

            StateMachine.ApplyState<ArgChildState>("seed");

            Assert.AreEqual("seed-fromchild", ParentState.ReceivedArg);
            Assert.AreEqual(new[] {
                "Exit:IdleState", "Enter:ArgChildState", "Enter:ArgParentState"
            }, StateMachine.Log);
        }
        
        private readonly struct AttackEvent { public readonly string Tag; public AttackEvent(string tag) { Tag = tag; } }
        private readonly struct JumpEvent { }

        private class SendHandlingParentState : TestState, IHandle<AttackEvent> {
            public bool Handled;
            public SendHandlingParentState(TestStateMachine SM) : base(SM) {}
            public bool Handle(AttackEvent Event) {
                Handled = true;
                StateMachine.Log.Add($"Handle:{GetType().Name}:{Event.Tag}");
                return true;
            }
        }

        private class SendBubblingChildState : TestState, IHandle<AttackEvent> {
            public SendBubblingChildState(TestStateMachine SM) : base(SM) {}
            public bool Handle(AttackEvent Event) {
                StateMachine.Log.Add($"Handle:{GetType().Name}:{Event.Tag}");
                return false;   // not mine — bubble
            }
        }

        [Test]
        public void Send_WhenLeafReturnsFalse_BubblesToParent() {
            TestStateMachine StateMachine = new();
            SendHandlingParentState ParentState = new(StateMachine);
            SendBubblingChildState ChildState = new(StateMachine);
            StateMachine.SetStates(ParentState, ChildState);
            StateMachine.SetParent<SendBubblingChildState, SendHandlingParentState>();
            StateMachine.StartStateMachine<SendBubblingChildState>();
            StateMachine.Log.Clear();

            bool Result = StateMachine.Send(new AttackEvent("hit"));

            Assert.IsTrue(Result);
            Assert.IsTrue(ParentState.Handled);
            Assert.AreEqual(new[] {
                "Handle:SendBubblingChildState:hit",
                "Handle:SendHandlingParentState:hit"
            }, StateMachine.Log);
        }
        
        private class PlainChildState : TestState {   // implements no IHandle<>
            public PlainChildState(TestStateMachine SM) : base(SM) {}
        }

        [Test]
        public void Send_WhenStateDoesNotImplementHandler_SkipsItAndBubbles() {
            TestStateMachine StateMachine = new();
            SendHandlingParentState ParentState = new(StateMachine);
            PlainChildState ChildState = new(StateMachine);
            StateMachine.SetStates(ParentState, ChildState);
            StateMachine.SetParent<PlainChildState, SendHandlingParentState>();
            StateMachine.StartStateMachine<PlainChildState>();
            StateMachine.Log.Clear();

            bool Result = StateMachine.Send(new AttackEvent("hit"));

            Assert.IsTrue(Result);
            Assert.AreEqual(new[] { "Handle:SendHandlingParentState:hit" }, StateMachine.Log);
        }
        
        [Test]
        public void Send_WhenNobodyHandles_ReturnsFalse() {
            TestStateMachine StateMachine = new();
            PlainChildState ChildState = new(StateMachine);
            StateMachine.SetStates(ChildState);
            StateMachine.StartStateMachine<PlainChildState>();
            StateMachine.Log.Clear();

            Assert.IsFalse(StateMachine.Send(new AttackEvent("hit")));
            Assert.IsEmpty(StateMachine.Log);
        }
        
        private class MultiEventState : TestState, IHandle<AttackEvent>, IHandle<JumpEvent> {
            public MultiEventState(TestStateMachine SM) : base(SM) {}
            public bool Handle(AttackEvent Event) { StateMachine.Log.Add("Attack"); return true; }
            public bool Handle(JumpEvent Event)   { StateMachine.Log.Add("Jump");   return true; }
        }

        [Test]
        public void Send_RoutesToHandlerOfMatchingEventType() {
            TestStateMachine StateMachine = new();
            MultiEventState State = new(StateMachine);
            StateMachine.SetStates(State);
            StateMachine.StartStateMachine<MultiEventState>();
            StateMachine.Log.Clear();

            StateMachine.Send(new JumpEvent());
            StateMachine.Send(new AttackEvent("x"));

            Assert.AreEqual(new[] { "Jump", "Attack" }, StateMachine.Log);
        }
        
        private class HierMachine : HierarchicalStateMachine<HierMachine, HierState> {
        }

        private abstract class HierState : HierarchicalState<HierMachine, HierState> {
            protected HierState(HierMachine StateMachine) : base(StateMachine) {
            }
        }
        
        


        private class BubblingLeafState : HierState, IHandle<HitEvent> {
            public BubblingLeafState(HierMachine StateMachine) : base(StateMachine) {
            }

            public bool Handle(HitEvent Event) {
                return false;   // not mine — bubble up
            }
        }

        private class RootHandlerState : HierState, IHandle<HitEvent> {
            public bool Handled;
            public RootHandlerState(HierMachine StateMachine) : base(StateMachine) {
            }

            public bool Handle(HitEvent Event) {
                Handled = true;
                return true;
            }
        }

        [Test]
        public void Send_ThroughBaseReference_OnHierarchicalMachine_StillBubbles() {
            HierMachine Machine = new();
            RootHandlerState Parent = new(Machine);
            BubblingLeafState Child = new(Machine);
            Machine.SetStates(Parent, Child);
            Machine.SetParent<BubblingLeafState, RootHandlerState>();
            Machine.StartStateMachine<BubblingLeafState>();

            // Hold it through the FLAT base type — this is the polymorphism we're proving.
            StateMachine<HierMachine, HierState> BaseReference = Machine;
            bool Result = BaseReference.Send(new HitEvent("hit"));

            Assert.IsTrue(Result);          // bubbled from child up to parent
            Assert.IsTrue(Parent.Handled);
        }

        readonly struct WhistleEvent {
            public readonly int Second;
            public WhistleEvent(int Second) { this.Second = Second; }
        }

        class FaceoffState : TestState, IHandle<WhistleEvent> {
            public bool Whistled;
            public int LastSecond;
            public FaceoffState(TestStateMachine StateMachine) : base(StateMachine) {
            }

            public bool Handle(WhistleEvent Event) {
                Whistled = true;
                LastSecond = Event.Second;
                return true;
            }
        }

        [Test]
        public void Send_ThroughIStateMachine_ReachesCurrentStateHandler() {
            TestStateMachine Machine = new();
            FaceoffState Faceoff = new(Machine);
            Machine.SetStates(Faceoff);
            Machine.StartStateMachine<FaceoffState>();

            IStateMachine StateMachine = Machine;               // erased handle
            bool Result = StateMachine.Send(new WhistleEvent(12));

            Assert.IsTrue(Result);
            Assert.IsTrue(Faceoff.Whistled);
            Assert.AreEqual(12, Faceoff.LastSecond);
        }
        
        [Test]
        public void ApplyState_WhenEnterStateTransitionsAway_NestedTargetBecomesCurrentState() {
            TestStateMachine StateMachine = new();
            ReroutingState ReroutingState = new(StateMachine);
            IdleState IdleState = new(StateMachine);
            WalkingState WalkingState = new(StateMachine);
            StateMachine.SetStates(ReroutingState, IdleState, WalkingState);
            StateMachine.StartStateMachine<WalkingState>();
            StateMachine.Log.Clear();

            StateMachine.ApplyState<ReroutingState>();

            Assert.AreEqual(IdleState, StateMachine.CurrentState);
        }

        [Test]
        public void ApplyState_WhenEnterStateTransitionsAway_ExitsTheReroutingStateNotTheSourceAgain() {
            TestStateMachine StateMachine = new();
            ReroutingState ReroutingState = new(StateMachine);
            IdleState IdleState = new(StateMachine);
            WalkingState WalkingState = new(StateMachine);
            StateMachine.SetStates(ReroutingState, IdleState, WalkingState);
            StateMachine.StartStateMachine<WalkingState>();
            StateMachine.Log.Clear();

            StateMachine.ApplyState<ReroutingState>();

            Assert.AreEqual(new[] {
                "Exit:WalkingState", "Enter:ReroutingState", "Exit:ReroutingState", "Enter:IdleState"
            }, StateMachine.Log);
        }

        [Test]
        public void ApplyState_WhenEnterStateDoesNotTransition_StillSetsCurrentState() {
            TestStateMachine StateMachine = new();
            IdleState IdleState = new(StateMachine);
            WalkingState WalkingState = new(StateMachine);
            StateMachine.SetStates(IdleState, WalkingState);
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.Log.Clear();

            StateMachine.ApplyState<WalkingState>();

            Assert.AreEqual(WalkingState, StateMachine.CurrentState);
            Assert.AreEqual(new[] { "Exit:IdleState", "Enter:WalkingState" }, StateMachine.Log);
        }
    }
}