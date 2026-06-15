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

            public override object EnterState(object StateEventArgs = null) {
                Entered = true;
                return StateEventArgs;
            }

            public override void ExitState() {
                Exited = true;
            }
        }

        private class AnotherState : TestState {
            public AnotherState(TestStateMachine TestStateMachine) : base(TestStateMachine) {
            }
        }

        private TestStateMachine StateMachine;
        private TestState State;

        [SetUp]
        public void Setup() {
            StateMachine = new TestStateMachine();
            State = new TestState(StateMachine);
        }

        [Test]
        public void Constructor_StoresStateMachineReference() {
            Assert.AreEqual(StateMachine, State.StateMachine);
        }

        [Test]
        public void EnterState_DefaultDoesNothingButCanBeOverridden() {
            State.EnterState();

            Assert.IsTrue(State.Entered);
        }

        [Test]
        public void ExitState_DefaultDoesNothingButCanBeOverridden() {
            State.ExitState();

            Assert.IsTrue(State.Exited);
        }

        [Test]
        public void ApplyState_Generic_ForwardsToStateMachine() {
            State.ApplyState<AnotherState>("Hello");

            Assert.AreEqual(typeof(AnotherState), StateMachine.LastAppliedType);
            Assert.AreEqual("Hello", StateMachine.LastArgs);
        }

        [Test]
        public void ApplyState_Type_ForwardsToStateMachine() {
            State.ApplyState(typeof(AnotherState), 123);

            Assert.AreEqual(typeof(AnotherState), StateMachine.LastAppliedType);
            Assert.AreEqual(123, StateMachine.LastArgs);
        }
    }
}