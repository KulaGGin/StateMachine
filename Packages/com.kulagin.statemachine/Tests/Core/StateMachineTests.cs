using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Kulagin.StateMachine.Core.Tests {
    public class StateMachineTests {
        private class TestStateMachine : StateMachine<TestStateMachine, TestState> {
            public TestStateMachine() {
            }


            public TestStateMachine(params TestState[] StartStates) : base(StartStates) {
            }
        }

        private class TestState : State<TestStateMachine, TestState> {
            public bool Entered;
            public bool Exited;
            public int EnterCount;
            public int ExitCount;
            public object ReceivedArgs;

            public TestState(TestStateMachine StateMachine) : base(StateMachine) {
            }

            public override void EnterState(object StateEventArgs = null) {
                Entered = true;
                EnterCount++;
                ReceivedArgs = StateEventArgs;
            }

            public override void ExitState() {
                Exited = true;
                ExitCount++;
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

        [Test]
        public void Constructor_WithEnumerable_RegistersStates() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);
            var WalkingState = new WalkingState(StateMachine);

            StateMachine = new TestStateMachine(new TestState[] {
                IdleState,
                WalkingState
            });

            StateMachine.StartStateMachine<WalkingState>();

            Assert.AreEqual(WalkingState, StateMachine.CurrentState);
        }

        [Test]
        public void Constructor_WithParams_RegistersStates() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);
            var WalkingState = new WalkingState(StateMachine);

            StateMachine = new TestStateMachine(IdleState, WalkingState);

            StateMachine.StartStateMachine<IdleState>();

            Assert.AreEqual(IdleState, StateMachine.CurrentState);
        }

        [Test]
        public void SetStates_WithEnumerable_RegistersStates() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);
            var WalkingState = new WalkingState(StateMachine);

            StateMachine.SetStates(new TestState[] {
                IdleState,
                WalkingState
            });

            StateMachine.StartStateMachine<WalkingState>();

            Assert.AreEqual(WalkingState, StateMachine.CurrentState);
        }

        [Test]
        public void SetStates_WithParams_RegistersStates() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);
            var WalkingState = new WalkingState(StateMachine);

            StateMachine.SetStates(IdleState, WalkingState);

            StateMachine.StartStateMachine<IdleState>();

            Assert.AreEqual(IdleState, StateMachine.CurrentState);
        }

        [Test]
        public void SetStates_ClearsPreviousStates() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);

            var WalkingState = new WalkingState(StateMachine);

            StateMachine.SetStates(WalkingState);

            Assert.Throws<ArgumentException>(() => {
                StateMachine.StartStateMachine<IdleState>();
            });
        }

        [Test]
        public void StartStateMachine_Generic_StartsCorrectState() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);

            StateMachine.StartStateMachine<IdleState>();

            Assert.AreEqual(IdleState, StateMachine.CurrentState);
        }

        [Test]
        public void StartStateMachine_Type_StartsCorrectState() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);

            StateMachine.StartStateMachine(typeof(IdleState));

            Assert.AreEqual(IdleState, StateMachine.CurrentState);
        }

        [Test]
        public void StartStateMachine_Generic_CallsEnterState() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);

            StateMachine.StartStateMachine<IdleState>();

            Assert.IsTrue(IdleState.Entered);
        }

        [Test]
        public void StartStateMachine_Type_CallsEnterState() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);

            StateMachine.StartStateMachine(typeof(IdleState));

            Assert.IsTrue(IdleState.Entered);
        }

        [Test]
        public void StartStateMachine_CallsEnterStateExactlyOnce() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);

            StateMachine.StartStateMachine<IdleState>();

            Assert.AreEqual(1, IdleState.EnterCount);
        }

        [Test]
        public void IsInState_Generic_ReturnsTrueForCurrentState() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);

            StateMachine.StartStateMachine<IdleState>();

            Assert.IsTrue(StateMachine.IsInState<IdleState>());
        }

        [Test]
        public void IsInState_Generic_ReturnsFalseForDifferentState() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);
            var WalkingState = new WalkingState(StateMachine);

            StateMachine.SetStates(IdleState, WalkingState);

            StateMachine.StartStateMachine<IdleState>();

            Assert.IsFalse(StateMachine.IsInState<WalkingState>());
        }

        [Test]
        public void IsInState_Type_ReturnsTrueForCurrentState() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);

            StateMachine.StartStateMachine<IdleState>();

            Assert.IsTrue(StateMachine.IsInState(typeof(IdleState)));
        }

        [Test]
        public void IsInState_Type_ReturnsFalseForDifferentState() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);
            var WalkingState = new WalkingState(StateMachine);

            StateMachine.SetStates(IdleState, WalkingState);

            StateMachine.StartStateMachine<IdleState>();

            Assert.IsFalse(StateMachine.IsInState(typeof(WalkingState)));
        }

        [Test]
        public void ApplyState_Generic_ChangesCurrentState() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);
            var WalkingState = new WalkingState(StateMachine);

            StateMachine.SetStates(IdleState, WalkingState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<WalkingState>();

            Assert.AreEqual(WalkingState, StateMachine.CurrentState);
        }

        [Test]
        public void ApplyState_Type_ChangesCurrentState() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);
            var WalkingState = new WalkingState(StateMachine);

            StateMachine.SetStates(IdleState, WalkingState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState(typeof(WalkingState));

            Assert.AreEqual(WalkingState, StateMachine.CurrentState);
        }

        [Test]
        public void ApplyState_CallsExitStateOnPreviousState() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);
            var WalkingState = new WalkingState(StateMachine);

            StateMachine.SetStates(IdleState, WalkingState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<WalkingState>();

            Assert.IsTrue(IdleState.Exited);
        }

        [Test]
        public void ApplyState_CallsExitStateExactlyOnce() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);
            var WalkingState = new WalkingState(StateMachine);

            StateMachine.SetStates(IdleState, WalkingState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<WalkingState>();

            Assert.AreEqual(1, IdleState.ExitCount);
        }

        [Test]
        public void ApplyState_CallsEnterStateOnNewState() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);
            var WalkingState = new WalkingState(StateMachine);

            StateMachine.SetStates(IdleState, WalkingState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<WalkingState>();

            Assert.IsTrue(WalkingState.Entered);
        }

        [Test]
        public void ApplyState_CallsEnterStateExactlyOnceOnNewState() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);
            var WalkingState = new WalkingState(StateMachine);

            StateMachine.SetStates(IdleState, WalkingState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<WalkingState>();

            Assert.AreEqual(1, WalkingState.EnterCount);
        }

        [Test]
        public void ApplyState_ForwardsArgumentsToNewState() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);
            var WalkingState = new WalkingState(StateMachine);

            StateMachine.SetStates(IdleState, WalkingState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<WalkingState>("Hello");

            Assert.AreEqual("Hello", WalkingState.ReceivedArgs);
        }

        [Test]
        public void ApplyState_ToSameState_ExitsAndReEntersState() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);

            StateMachine.StartStateMachine<IdleState>();

            IdleState.Entered = false;
            IdleState.Exited = false;

            StateMachine.ApplyState<IdleState>();

            Assert.IsTrue(IdleState.Exited);
            Assert.IsTrue(IdleState.Entered);
        }

        [Test]
        public void ApplyState_ToSameState_CallsLifecycleMethodsAgain() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<IdleState>();

            Assert.AreEqual(2, IdleState.EnterCount);
            Assert.AreEqual(1, IdleState.ExitCount);
        }

        [Test]
        public void ApplyState_BeforeStarting_ThrowsNullReferenceException() {
            var StateMachine = new TestStateMachine();

            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);

            Assert.Throws<NullReferenceException>(() => { StateMachine.ApplyState<IdleState>(); });
        }

        [Test]
        public void StartStateMachine_Generic_InvalidState_ThrowsArgumentException() {
            var StateMachine = new TestStateMachine();
            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);

            var Ex = Assert.Throws<ArgumentException>(() => StateMachine.StartStateMachine<WalkingState>());

            Assert.That(Ex.Message, Contains.Substring("WalkingState"));
            Assert.That(Ex.ParamName, Is.EqualTo("StartingState"));
        }

        [Test]
        public void StartStateMachine_Type_InvalidState_ThrowsArgumentException() {
            var StateMachine = new TestStateMachine();
            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);

            var Ex = Assert.Throws<ArgumentException>(() => StateMachine.StartStateMachine(typeof(WalkingState)));

            Assert.That(Ex.Message, Contains.Substring("WalkingState"));
        }

        [Test]
        public void StartStateMachine_InvalidState_DoesNotSetCurrentState() {
            var StateMachine = new TestStateMachine();
            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);

            Assert.Throws<ArgumentException>(() => StateMachine.StartStateMachine<WalkingState>());

            Assert.IsNull(StateMachine.CurrentState);
        }

        [Test]
        public void ApplyState_InvalidState_ThrowsArgumentException() {
            var StateMachine = new TestStateMachine();
            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);
            StateMachine.StartStateMachine<IdleState>();

            var Ex = Assert.Throws<ArgumentException>(() => StateMachine.ApplyState<WalkingState>());

            Assert.That(Ex.Message, Contains.Substring("WalkingState"));
        }

        [Test]
        public void ApplyState_InvalidState_PreservesCurrentState() {
            var StateMachine = new TestStateMachine();
            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);
            StateMachine.StartStateMachine<IdleState>();

            Assert.Throws<ArgumentException>(() => StateMachine.ApplyState<WalkingState>());

            Assert.AreEqual(IdleState, StateMachine.CurrentState);
        }
    }
}