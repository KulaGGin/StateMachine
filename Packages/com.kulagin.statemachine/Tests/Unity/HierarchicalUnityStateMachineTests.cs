using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Kulagin.StateMachine.Unity.Tests {
    public class HierarchicalUnityStateMachineTests {
        private class TestStateMachine : HierarchicalUnityStateMachine<TestStateMachine, TestState> {
            public List<string> Log = new();
        }

        private abstract class TestState : HierarchicalUnityState<TestStateMachine, TestState> {
            protected TestState(TestStateMachine SM) : base(SM) {}
            public override void PhysicsUpdate() => StateMachine.Log.Add($"Physics:{GetType().Name}");
            public override void FrameUpdate()   => StateMachine.Log.Add($"Frame:{GetType().Name}");
            public override void LateUpdate()    => StateMachine.Log.Add($"Late:{GetType().Name}");
        }

        private class ParentState : TestState { public ParentState(TestStateMachine SM) : base(SM) {} }
        private class ChildState  : TestState { public ChildState(TestStateMachine SM)  : base(SM) {} }

        [Test]
        public void PhysicsUpdate_RunsWholePathLeafToRoot() {
            TestStateMachine StateMachine = new();
            ParentState ParentState = new(StateMachine);
            ChildState ChildState = new(StateMachine);
            StateMachine.SetStates(ParentState, ChildState);
            StateMachine.SetParent<ChildState, ParentState>();
            StateMachine.StartStateMachine<ChildState>();

            StateMachine.PhysicsUpdate();

            Assert.AreEqual(new[] { "Physics:ChildState", "Physics:ParentState" }, StateMachine.Log);
        }
        
        private class GrandparentState : TestState { public GrandparentState(TestStateMachine SM) : base(SM) {} }
        private class OtherLeafState   : TestState { public OtherLeafState(TestStateMachine SM)   : base(SM) {} }

        private class TransitioningLeafState : TestState {
            public TransitioningLeafState(TestStateMachine SM) : base(SM) {}
            public override void PhysicsUpdate() {
                StateMachine.Log.Add($"Physics:{GetType().Name}");
                ApplyState<OtherLeafState>();   // transition during the tick
            }
        }

        [Test]
        public void PhysicsUpdate_StopsWhenStateTransitionsMidTick() {
            TestStateMachine StateMachine = new();
            GrandparentState GrandparentState = new(StateMachine);
            ParentState ParentState = new(StateMachine);
            TransitioningLeafState LeafState = new(StateMachine);
            OtherLeafState OtherLeafState = new(StateMachine);
            StateMachine.SetStates(GrandparentState, ParentState, LeafState, OtherLeafState);
            StateMachine.SetParent<ParentState, GrandparentState>();
            StateMachine.SetParent<TransitioningLeafState, ParentState>();
            StateMachine.StartStateMachine<TransitioningLeafState>();
            StateMachine.Log.Clear();

            StateMachine.PhysicsUpdate();

            Assert.AreEqual(new[] { "Physics:TransitioningLeafState" }, StateMachine.Log);   // parent + grandparent skipped
            Assert.IsTrue(StateMachine.IsInState<OtherLeafState>());
        }
        
        [Test]
        public void FrameUpdate_RunsWholePathLeafToRoot() {
            TestStateMachine StateMachine = new();
            ParentState ParentState = new(StateMachine);
            ChildState ChildState = new(StateMachine);
            StateMachine.SetStates(ParentState, ChildState);
            StateMachine.SetParent<ChildState, ParentState>();
            StateMachine.StartStateMachine<ChildState>();

            StateMachine.FrameUpdate();
            StateMachine.LateUpdate();

            Assert.AreEqual(new[] {
                "Frame:ChildState", "Frame:ParentState",
                "Late:ChildState",  "Late:ParentState"
            }, StateMachine.Log);
        }
        
        [Test]
        public void PhysicsUpdate_BeforeStart_Throws() {
            TestStateMachine StateMachine = new();
            StateMachine.SetStates(new ParentState(StateMachine));

            Assert.Throws<NullReferenceException>(StateMachine.PhysicsUpdate);
        }
    }
}