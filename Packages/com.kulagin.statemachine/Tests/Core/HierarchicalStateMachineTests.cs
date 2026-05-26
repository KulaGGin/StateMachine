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

            public override void EnterState(object StateEventArgs = null) {
                StateMachine.Log.Add($"Enter:{GetType().Name}");
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
        public void StartStateMachine_OnChildState_EntersParentBeforeChild() {
            TestStateMachine StateMachine = new();
            ParentState ParentState = new(StateMachine);
            ChildState ChildState = new(StateMachine);
            StateMachine.SetStates(ParentState, ChildState);
            StateMachine.SetParent<ChildState, ParentState>();

            StateMachine.StartStateMachine<ChildState>();

            Assert.AreEqual(new[] { "Enter:ParentState", "Enter:ChildState" }, StateMachine.Log);
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
                "Enter:Branch2State", "Enter:Leaf2State"
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

        [Test]
        public void HandleEvent_WhenLeafDoesNotHandle_BubblesToParent() {
            TestStateMachine StateMachine = new();
            HandlingParentState ParentState = new(StateMachine);
            NonHandlingChildState ChildState = new(StateMachine);
            StateMachine.SetStates(ParentState, ChildState);
            StateMachine.SetParent<NonHandlingChildState, HandlingParentState>();
            StateMachine.StartStateMachine<NonHandlingChildState>();
            StateMachine.Log.Clear();

            StateMachine.HandleEvent("Attack");

            Assert.AreEqual(new[] {
                "HandleEvent:NonHandlingChildState:Attack",   // child saw it, said "not mine"
                "HandleEvent:HandlingParentState:Attack"      // parent picked it up
            }, StateMachine.Log);
            Assert.IsTrue(ParentState.HandledEvent);
        }
        
        private class HandlingChildState : TestState {
            public HandlingChildState(TestStateMachine StateMachine) : base(StateMachine) {
            }

            public override bool HandleEvent(object Event) {
                StateMachine.Log.Add($"HandleEvent:{GetType().Name}:{Event}");
                return true;
            }
        }

        [Test]
        public void HandleEvent_WhenLeafHandles_ParentDoesNotSeeEvent() {
            TestStateMachine StateMachine = new();
            HandlingParentState ParentState = new(StateMachine);
            HandlingChildState ChildState = new(StateMachine);
            StateMachine.SetStates(ParentState, ChildState);
            StateMachine.SetParent<HandlingChildState, HandlingParentState>();
            StateMachine.StartStateMachine<HandlingChildState>();
            StateMachine.Log.Clear();

            StateMachine.HandleEvent("Attack");

            Assert.AreEqual(new[] { "HandleEvent:HandlingChildState:Attack" }, StateMachine.Log);
            Assert.IsFalse(ParentState.HandledEvent);
        }
    }
}