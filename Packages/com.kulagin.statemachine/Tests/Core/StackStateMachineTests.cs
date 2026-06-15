using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace Kulagin.StateMachine.Core.Tests {
    public class StackStateMachineTests {
        private class TestStackStateMachine : StackStateMachine<TestStackStateMachine, TestState> {
        }

        private class TestState : State<TestStackStateMachine, TestState> {
            public bool Entered;
            public bool Exited;
            public object ReceivedArgs;

            public TestState(TestStackStateMachine StateMachine) : base(StateMachine) {
            }

            public override object EnterState(object StateEventArgs = null) {
                Entered = true;
                ReceivedArgs = StateEventArgs;
                return StateEventArgs;
            }

            public override void ExitState() {
                Exited = true;
            }
        }

        private class IdleState : TestState {
            public IdleState(TestStackStateMachine StateMachine) : base(StateMachine) {
            }
        }

        private class PauseState : TestState {
            public PauseState(TestStackStateMachine StateMachine) : base(StateMachine) {
            }
        }

        private class MenuState : TestState {
            public MenuState(TestStackStateMachine StateMachine) : base(StateMachine) {
            }
        }

        private TestStackStateMachine StateMachine;
        private IdleState _IdleState;
        private PauseState _PauseState;
        private MenuState _MenuState;

        [SetUp]
        public void Setup() {
            StateMachine = new TestStackStateMachine();
            _IdleState = new IdleState(StateMachine);
            _PauseState = new PauseState(StateMachine);
            _MenuState = new MenuState(StateMachine);
        }

        [Test]
        public void StartStateMachine_PushesStateToStack() {
            StateMachine.SetStates(_IdleState);

            StateMachine.StartStateMachine<IdleState>();

            Assert.AreEqual(1, StateMachine.StatesStack.Count);
            Assert.AreEqual(typeof(IdleState), StateMachine.StatesStack.First());
        }

        [Test]
        public void StartStateMachine_SetsCurrentState() {
            StateMachine.SetStates(_IdleState);

            StateMachine.StartStateMachine<IdleState>();

            Assert.AreEqual(_IdleState, StateMachine.CurrentState);
        }

        [Test]
        public void ApplyState_PushesStateToStack() {
            StateMachine.SetStates(_IdleState, _PauseState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>();

            Assert.AreEqual(2, StateMachine.StatesStack.Count);
            Assert.AreEqual(typeof(PauseState), StateMachine.StatesStack.First());
        }

        [Test]
        public void ApplyState_ChangesCurrentState() {
            StateMachine.SetStates(new TestState[] {
                _IdleState,
                _PauseState
            });

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>();

            Assert.AreEqual(_PauseState, StateMachine.CurrentState);
        }

        [Test]
        public void ApplyState_ExitsPreviousState() {
            StateMachine.SetStates(_IdleState, _PauseState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>();

            Assert.IsTrue(_IdleState.Exited);
        }

        [Test]
        public void ApplyState_EntersNewState() {
            StateMachine.SetStates(_IdleState, _PauseState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>();

            Assert.IsTrue(_PauseState.Entered);
        }

        [Test]
        public void ApplyState_ForwardsArgumentsToNewState() {
            StateMachine.SetStates(_IdleState, _PauseState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>("PauseArgs");

            Assert.AreEqual("PauseArgs", _PauseState.ReceivedArgs);
        }

        [Test]
        public void PopState_RemovesTopStateFromStack() {
            StateMachine.SetStates(_IdleState, _PauseState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>();

            StateMachine.TryPopState();

            Assert.AreEqual(1, StateMachine.StatesStack.Count);
            Assert.AreEqual(typeof(IdleState), StateMachine.StatesStack.First());
        }

        [Test]
        public void PopState_ReturnsToPreviousState() {
            StateMachine.SetStates(_IdleState, _PauseState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>();

            StateMachine.TryPopState();

            Assert.AreEqual(_IdleState, StateMachine.CurrentState);
        }

        [Test]
        public void PopState_EntersPreviousStateAgain() {
            StateMachine.SetStates(_IdleState, _PauseState);

            StateMachine.StartStateMachine<IdleState>();

            _IdleState.Entered = false;

            StateMachine.ApplyState<PauseState>();

            StateMachine.TryPopState();

            Assert.IsTrue(_IdleState.Entered);
        }

        [Test]
        public void PopState_ExitsCurrentTopState() {
            StateMachine.SetStates(_IdleState, _PauseState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>();

            StateMachine.TryPopState();

            Assert.IsTrue(_PauseState.Exited);
        }

        [Test]
        public void MultipleApplyState_CreatesCorrectStackOrder() {
            StateMachine.SetStates(_IdleState, _PauseState, _MenuState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>();

            StateMachine.ApplyState<MenuState>();

            var States = StateMachine.StatesStack.ToArray();

            Assert.AreEqual(typeof(MenuState), States[0]);
            Assert.AreEqual(typeof(PauseState), States[1]);
            Assert.AreEqual(typeof(IdleState), States[2]);
        }

        [Test]
        public void PopState_AfterMultipleStates_ReturnsToPreviousState() { 
            StateMachine.SetStates(_IdleState, _PauseState, _MenuState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>();

            StateMachine.ApplyState<MenuState>();

            StateMachine.TryPopState();

            Assert.AreEqual(_PauseState, StateMachine.CurrentState);
        }
        
        [Test]
        public void TryPopState_WithMultipleStatesOnStack_RemovesTopState() { 
            StateMachine.SetStates(_IdleState, _PauseState);
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<PauseState>();
            
            var Result = StateMachine.TryPopState();
            
            Assert.IsTrue(Result);
            Assert.AreEqual(_IdleState, StateMachine.CurrentState);
        }

        [Test]
        public void TryPopState_WithOnlyRootState_ReturnsFalse() {
            StateMachine.SetStates(_IdleState);
            StateMachine.StartStateMachine<IdleState>();
            
            var Result = StateMachine.TryPopState();
            
            Assert.IsFalse(Result);
        }

        [Test]
        public void TryPopState_WithOnlyRootState_PreservesCurrentState() {
            StateMachine.SetStates(_IdleState);
            StateMachine.StartStateMachine<IdleState>();
            
            StateMachine.TryPopState();
            
            Assert.AreEqual(_IdleState, StateMachine.CurrentState);
        }

        [Test]
        public void TryPopState_WithOnlyRootState_DoesNotExitState() {
            StateMachine.SetStates(_IdleState);
            StateMachine.StartStateMachine<IdleState>();
            
            StateMachine.TryPopState();
            
            Assert.IsFalse(_IdleState.Exited);
        }

        [Test]
        public void TryPopState_AfterMultiplePushes_ReturnsToCorrectPreviousState() {
            StateMachine.SetStates(_IdleState, _PauseState, _MenuState);
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<PauseState>();
            StateMachine.ApplyState<MenuState>();
            
            var Result = StateMachine.TryPopState();
            
            Assert.IsTrue(Result);
            Assert.AreEqual(_PauseState, StateMachine.CurrentState);
        }

        [Test]
        public void TryPopState_CallsExitOnPoppedState() {
            StateMachine.SetStates(_IdleState, _PauseState);
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<PauseState>();
            
            StateMachine.TryPopState();
            
            Assert.IsTrue(_PauseState.Exited);
        }

        [Test]
        public void TryPopState_CallsEnterOnPreviousState() {
            StateMachine.SetStates(_IdleState, _PauseState);
            StateMachine.StartStateMachine<IdleState>();

            _IdleState.Entered = false;

            StateMachine.ApplyState<PauseState>();
            StateMachine.TryPopState();
            
            Assert.IsTrue(_IdleState.Entered);
        }
        
        [Test]
        public void CanPop_WithOnlyRootState_ReturnsFalse() {
            StateMachine.SetStates(_IdleState);
            StateMachine.StartStateMachine<IdleState>();

            Assert.IsFalse(StateMachine.CanPop);
        }

        [Test]
        public void CanPop_WithMultipleStatesOnStack_ReturnsTrue() {
            StateMachine.SetStates(_IdleState, _PauseState);
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<PauseState>();

            Assert.IsTrue(StateMachine.CanPop);
        }

        [Test]
        public void CanPop_AfterTryPopState_ReturnsFalse() {
            StateMachine.SetStates(_IdleState, _PauseState);
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<PauseState>();
            StateMachine.TryPopState();

            Assert.IsFalse(StateMachine.CanPop);
        }

        [Test]
        public void CanPop_IsConsistentWithTryPopStateReturnValue() {
            StateMachine.SetStates(_IdleState, _PauseState);
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<PauseState>();

            Assert.AreEqual(StateMachine.CanPop, StateMachine.TryPopState());
        }
        
        [Test]
        public void StatesStack_IsExposedAsReadOnlyCollection() {
            StateMachine.SetStates(_IdleState);
            StateMachine.StartStateMachine<IdleState>();
            
            IReadOnlyCollection<Type> Stack = StateMachine.StatesStack;

            Assert.AreEqual(1, Stack.Count);
        }
        
                
        [Test]
        public void TryPopState_ForwardsArgumentsToPreviousState() {
            StateMachine.SetStates(_IdleState, _PauseState);
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<PauseState>();

            StateMachine.TryPopState("ResumeContext");

            Assert.AreEqual("ResumeContext", _IdleState.ReceivedArgs);
        }

        [Test]
        public void TryPopState_WithoutArguments_PassesNullToPreviousState() {
            StateMachine.SetStates(_IdleState, _PauseState);
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<PauseState>();

            // Poison the field so we can detect that pop overwrites it with null.
            _IdleState.ReceivedArgs = new object();

            StateMachine.TryPopState();

            Assert.IsNull(_IdleState.ReceivedArgs);
        }

        [Test]
        public void TryPopState_AtRoot_DoesNotInvokePreviousStateWithArgs() {
            StateMachine.SetStates(_IdleState);
            StateMachine.StartStateMachine<IdleState>();

            _IdleState.ReceivedArgs = null;
            _IdleState.Entered = false;

            var Result = StateMachine.TryPopState("ShouldBeIgnored");

            Assert.IsFalse(Result);
            Assert.IsFalse(_IdleState.Entered);
            Assert.IsNull(_IdleState.ReceivedArgs);
        }
    }
}