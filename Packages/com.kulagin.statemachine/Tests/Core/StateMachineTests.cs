using System;
using NUnit.Framework;

namespace Kulagin.StateMachine.Core.Tests {
    class TestStateMachine : StateMachine<TestStateMachine, TestState> {
        public TestStateMachine() {
        }


        public TestStateMachine(params TestState[] StartStates) : base(StartStates) {
        }
    }

    class TestState : State<TestStateMachine, TestState> {
        public bool Entered;
        public bool Exited;
        public int EnterCount;
        public int ExitCount;
        public object ReceivedArgs;

        protected TestState(TestStateMachine TestStateMachine) : base(TestStateMachine) {
        }

        public override object EnterState(object StateEventArgs = null) {
            Entered = true;
            EnterCount++;
            ReceivedArgs = StateEventArgs;
            return StateEventArgs;
            
        }

        public override void ExitState() {
            Exited = true;
            ExitCount++;
        }
    }

    class IdleState : TestState {
        public IdleState(TestStateMachine TestStateMachine) : base(TestStateMachine) {
        }
    }

    class WalkingState : TestState {
        public WalkingState(TestStateMachine TestStateMachine) : base(TestStateMachine) {
        }
    }
    
    public class TestStateMachineTests {
        private TestStateMachine TestStateMachine;
        private IdleState IdleState;
        private WalkingState WalkingState;

        [SetUp]
        public void SetUp() {
            TestStateMachine = new TestStateMachine();
            IdleState = new IdleState(TestStateMachine);
            WalkingState = new WalkingState(TestStateMachine);
        }

        [Test]
        public void Constructor_WithEnumerable_RegistersStates() {
            TestStateMachine = new TestStateMachine(new TestState[] {
                IdleState,
                WalkingState
            });

            TestStateMachine.StartStateMachine<WalkingState>();

            Assert.AreEqual(WalkingState, TestStateMachine.CurrentState);
        }

        [Test]
        public void Constructor_WithParams_RegistersStates() {
            TestStateMachine = new TestStateMachine(IdleState, WalkingState);

            TestStateMachine.StartStateMachine<IdleState>();

            Assert.AreEqual(IdleState, TestStateMachine.CurrentState);
        }

        [Test]
        public void SetStates_WithEnumerable_RegistersStates() {
            TestStateMachine.SetStates(new TestState[] {
                IdleState,
                WalkingState
            });

            TestStateMachine.StartStateMachine<WalkingState>();

            Assert.AreEqual(WalkingState, TestStateMachine.CurrentState);
        }

        [Test]
        public void SetStates_WithParams_RegistersStates() {
            TestStateMachine.SetStates(IdleState, WalkingState);

            TestStateMachine.StartStateMachine<IdleState>();

            Assert.AreEqual(IdleState, TestStateMachine.CurrentState);
        }

        [Test]
        public void SetStates_ClearsPreviousStates() {
            TestStateMachine.SetStates(IdleState);
            TestStateMachine.SetStates(WalkingState);

            Assert.Throws<ArgumentException>(TestStateMachine.StartStateMachine<IdleState>);
        }

        [Test]
        public void StartStateMachine_Generic_StartsCorrectState() {
            TestStateMachine.SetStates(IdleState);

            TestStateMachine.StartStateMachine<IdleState>();

            Assert.AreEqual(IdleState, TestStateMachine.CurrentState);
        }

        [Test]
        public void StartStateMachine_Type_StartsCorrectState() {
            TestStateMachine.SetStates(IdleState);

            TestStateMachine.StartStateMachine(typeof(IdleState));

            Assert.AreEqual(IdleState, TestStateMachine.CurrentState);
        }

        [Test]
        public void StartStateMachine_Generic_CallsEnterState() {
            TestStateMachine.SetStates(IdleState);

            TestStateMachine.StartStateMachine<IdleState>();

            Assert.IsTrue(IdleState.Entered);
        }

        [Test]
        public void StartStateMachine_Type_CallsEnterState() {
            TestStateMachine.SetStates(IdleState);

            TestStateMachine.StartStateMachine(typeof(IdleState));

            Assert.IsTrue(IdleState.Entered);
        }

        [Test]
        public void StartStateMachine_CallsEnterStateExactlyOnce() {
            TestStateMachine.SetStates(IdleState);

            TestStateMachine.StartStateMachine<IdleState>();

            Assert.AreEqual(1, IdleState.EnterCount);
        }

        [Test]
        public void IsInState_Generic_ReturnsTrueForCurrentState() {
            TestStateMachine.SetStates(IdleState);

            TestStateMachine.StartStateMachine<IdleState>();

            Assert.IsTrue(TestStateMachine.IsInState<IdleState>());
        }

        [Test]
        public void IsInState_Generic_ReturnsFalseForDifferentState() {
            TestStateMachine.SetStates(IdleState, WalkingState);

            TestStateMachine.StartStateMachine<IdleState>();

            Assert.IsFalse(TestStateMachine.IsInState<WalkingState>());
        }

        [Test]
        public void IsInState_Type_ReturnsTrueForCurrentState() {
            TestStateMachine.SetStates(IdleState);
            TestStateMachine.StartStateMachine<IdleState>();

            Assert.IsTrue(TestStateMachine.IsInState(typeof(IdleState)));
        }

        [Test]
        public void IsInState_Type_ReturnsFalseForDifferentState() {
            TestStateMachine.SetStates(IdleState, WalkingState);
            TestStateMachine.StartStateMachine<IdleState>();

            Assert.IsFalse(TestStateMachine.IsInState(typeof(WalkingState)));
        }

        [Test]
        public void ApplyState_Generic_ChangesCurrentState() {
            TestStateMachine.SetStates(IdleState, WalkingState);
            TestStateMachine.StartStateMachine<IdleState>();

            TestStateMachine.ApplyState<WalkingState>();

            Assert.AreEqual(WalkingState, TestStateMachine.CurrentState);
        }

        [Test]
        public void ApplyState_Type_ChangesCurrentState() {
            TestStateMachine.SetStates(IdleState, WalkingState);
            TestStateMachine.StartStateMachine<IdleState>();

            TestStateMachine.ApplyState(typeof(WalkingState));

            Assert.AreEqual(WalkingState, TestStateMachine.CurrentState);
        }

        [Test]
        public void ApplyState_CallsExitStateOnPreviousState() {
            TestStateMachine.SetStates(IdleState, WalkingState);
            TestStateMachine.StartStateMachine<IdleState>();

            TestStateMachine.ApplyState<WalkingState>();

            Assert.IsTrue(IdleState.Exited);
        }

        [Test]
        public void ApplyState_CallsExitStateExactlyOnce() {
            TestStateMachine.SetStates(IdleState, WalkingState);
            TestStateMachine.StartStateMachine<IdleState>();

            TestStateMachine.ApplyState<WalkingState>();

            Assert.AreEqual(1, IdleState.ExitCount);
        }

        [Test]
        public void ApplyState_CallsEnterStateOnNewState() {
            TestStateMachine.SetStates(IdleState, WalkingState);
            TestStateMachine.StartStateMachine<IdleState>();

            TestStateMachine.ApplyState<WalkingState>();

            Assert.IsTrue(WalkingState.Entered);
        }

        [Test]
        public void ApplyState_CallsEnterStateExactlyOnceOnNewState() {
            TestStateMachine.SetStates(IdleState, WalkingState);
            TestStateMachine.StartStateMachine<IdleState>();

            TestStateMachine.ApplyState<WalkingState>();

            Assert.AreEqual(1, WalkingState.EnterCount);
        }

        [Test]
        public void ApplyState_ForwardsArgumentsToNewState() {
            TestStateMachine.SetStates(IdleState, WalkingState);
            TestStateMachine.StartStateMachine<IdleState>();

            TestStateMachine.ApplyState<WalkingState>("Hello");

            Assert.AreEqual("Hello", WalkingState.ReceivedArgs);
        }

        [Test]
        public void ApplyState_ToSameState_ExitsAndReEntersState() {
            TestStateMachine.SetStates(IdleState);
            TestStateMachine.StartStateMachine<IdleState>();

            IdleState.Entered = false;
            IdleState.Exited = false;

            TestStateMachine.ApplyState<IdleState>();

            Assert.IsTrue(IdleState.Exited);
            Assert.IsTrue(IdleState.Entered);
        }

        [Test]
        public void ApplyState_ToSameState_CallsLifecycleMethodsAgain() {
            TestStateMachine.SetStates(IdleState);
            TestStateMachine.StartStateMachine<IdleState>();

            TestStateMachine.ApplyState<IdleState>();

            Assert.AreEqual(2, IdleState.EnterCount);
            Assert.AreEqual(1, IdleState.ExitCount);
        }

        [Test]
        public void ApplyState_BeforeStarting_ThrowsNullReferenceException() {
            TestStateMachine.SetStates(IdleState);

            Assert.Throws<NullReferenceException>(() => { TestStateMachine.ApplyState<IdleState>(); });
        }

        [Test]
        public void StartStateMachine_Generic_InvalidState_ThrowsArgumentException() {
            TestStateMachine.SetStates(IdleState);

            var Ex = Assert.Throws<ArgumentException>(TestStateMachine.StartStateMachine<WalkingState>);

            Assert.That(Ex.Message, Contains.Substring("WalkingState"));
            Assert.That(Ex.ParamName, Is.EqualTo("StartingState"));
        }

        [Test]
        public void StartStateMachine_Type_InvalidState_ThrowsArgumentException() {
            TestStateMachine.SetStates(IdleState);

            var Ex = Assert.Throws<ArgumentException>(() => TestStateMachine.StartStateMachine(typeof(WalkingState)));

            Assert.That(Ex.Message, Contains.Substring("WalkingState"));
        }

        [Test]
        public void StartStateMachine_InvalidState_DoesNotSetCurrentState() {
            TestStateMachine.SetStates(IdleState);

            Assert.Throws<ArgumentException>(TestStateMachine.StartStateMachine<WalkingState>);

            Assert.IsNull(TestStateMachine.CurrentState);
        }

        [Test]
        public void ApplyState_InvalidState_ThrowsArgumentException() {
            TestStateMachine.SetStates(IdleState);
            TestStateMachine.StartStateMachine<IdleState>();

            var Ex = Assert.Throws<ArgumentException>(() => TestStateMachine.ApplyState<WalkingState>());

            Assert.That(Ex.Message, Contains.Substring("WalkingState"));
        }

        [Test]
        public void ApplyState_InvalidState_PreservesCurrentState() {
            TestStateMachine.SetStates(IdleState);
            TestStateMachine.StartStateMachine<IdleState>();

            Assert.Throws<ArgumentException>(() => TestStateMachine.ApplyState<WalkingState>());

            Assert.AreEqual(IdleState, TestStateMachine.CurrentState);
        }
    }
}