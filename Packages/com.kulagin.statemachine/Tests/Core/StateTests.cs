using System;
using NUnit.Framework;

namespace Kulagin.StateMachine.Core.Tests {
    public class StateTests {
        private class TestStateMachine : StateMachine<TestStateMachine, TestState> {
            public Type LastAppliedType;
            public object LastArgs;

            public override void ApplyState(Type StateID, object StateEventArgs = null) {
                LastAppliedType = StateID;
                LastArgs = StateEventArgs;
            }
        }

        private class TestState : State<TestStateMachine, TestState> {
            public bool Entered;
            public bool Exited;

            public TestState(TestStateMachine TestStateMachine) : base(TestStateMachine) {
            }

            public override void EnterState(object StateEventArgs = null) {
                Entered = true;
            }

            public override void ExitState() {
                Exited = true;
            }
        }

        private class AnotherState : TestState {
            public AnotherState(TestStateMachine TestStateMachine) : base(TestStateMachine) {
            }
        }

        [Test]
        public void Constructor_StoresStateMachineReference() {
            var StateMachine = new TestStateMachine();
            var State = new TestState(StateMachine);

            Assert.AreEqual(StateMachine, State.StateMachine);
        }

        [Test]
        public void EnterState_DefaultDoesNothingButCanBeOverridden() {
            var StateMachine = new TestStateMachine();
            var State = new TestState(StateMachine);

            State.EnterState();

            Assert.IsTrue(State.Entered);
        }

        [Test]
        public void ExitState_DefaultDoesNothingButCanBeOverridden() {
            var StateMachine = new TestStateMachine();
            var State = new TestState(StateMachine);

            State.ExitState();

            Assert.IsTrue(State.Exited);
        }

        [Test]
        public void ApplyState_Generic_ForwardsToStateMachine() {
            var StateMachine = new TestStateMachine();
            var State = new TestState(StateMachine);

            State.ApplyState<AnotherState>("Hello");

            Assert.AreEqual(typeof(AnotherState), StateMachine.LastAppliedType);
            Assert.AreEqual("Hello", StateMachine.LastArgs);
        }

        [Test]
        public void ApplyState_Type_ForwardsToStateMachine() {
            var StateMachine = new TestStateMachine();
            var State = new TestState(StateMachine);

            State.ApplyState(typeof(AnotherState), 123);

            Assert.AreEqual(typeof(AnotherState), StateMachine.LastAppliedType);
            Assert.AreEqual(123, StateMachine.LastArgs);
        }
    }
}