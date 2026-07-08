using System;
using NUnit.Framework;

namespace Kulagin.StateMachine.Core.Tests {
    public class StateMachineInterfaceTests {
        private class TestStateMachine : StateMachine<TestStateMachine, TestState> {
        }

        private abstract class TestState : State<TestStateMachine, TestState> {
            protected TestState(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }

        private class IdleState : TestState {
            public IdleState(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }

        [Test]
        public void ClosedMachine_HeldAsIStateMachine_StartsAndReportsState() {
            TestStateMachine Machine = new();
            IdleState Idle = new(Machine);
            Machine.SetStates(Idle);

            // The whole point: erase the closed generic behind the non-generic handle.
            IStateMachine StateMachine = Machine;
            StateMachine.StartStateMachine(typeof(IdleState));

            Assert.IsTrue(StateMachine.IsInState(typeof(IdleState)));
        }
        
        private class SkatingState : TestState {
            public SkatingState(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }

        [Test]
        public void CurrentStateType_ThroughHandle_IsNullBeforeStartThenTracksState() {
            TestStateMachine Machine = new();
            SkatingState Skating = new(Machine);
            Machine.SetStates(Skating);

            IStateMachine StateMachine = Machine;

            Assert.IsNull(StateMachine.CurrentStateType);

            Machine.StartStateMachine<SkatingState>();

            Assert.AreEqual(typeof(SkatingState), StateMachine.CurrentStateType);
        }
    }
}