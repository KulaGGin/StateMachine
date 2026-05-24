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

            public override void EnterState(object StateEventArgs = null) {
                Entered = true;
                ReceivedArgs = StateEventArgs;
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

        [Test]
        public void StartStateMachine_PushesStateToStack() {
            var StateMachine = new TestStackStateMachine();

            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);

            StateMachine.StartStateMachine<IdleState>();

            Assert.AreEqual(1, StateMachine.StatesStack.Count);
            Assert.AreEqual(typeof(IdleState), StateMachine.StatesStack.First());
        }

        [Test]
        public void StartStateMachine_SetsCurrentState() {
            var StateMachine = new TestStackStateMachine();

            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);

            StateMachine.StartStateMachine<IdleState>();

            Assert.AreEqual(IdleState, StateMachine.CurrentState);
        }

        [Test]
        public void ApplyState_PushesStateToStack() {
            var StateMachine = new TestStackStateMachine();

            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>();

            Assert.AreEqual(2, StateMachine.StatesStack.Count);
            Assert.AreEqual(typeof(PauseState), StateMachine.StatesStack.First());
        }

        [Test]
        public void ApplyState_ChangesCurrentState() {
            var StateMachine = new TestStackStateMachine();

            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);

            StateMachine.SetStates(new TestState[] {
                IdleState,
                PauseState
            });

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>();

            Assert.AreEqual(PauseState, StateMachine.CurrentState);
        }

        [Test]
        public void ApplyState_ExitsPreviousState() {
            var StateMachine = new TestStackStateMachine();

            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>();

            Assert.IsTrue(IdleState.Exited);
        }

        [Test]
        public void ApplyState_EntersNewState() {
            var StateMachine = new TestStackStateMachine();

            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>();

            Assert.IsTrue(PauseState.Entered);
        }

        [Test]
        public void ApplyState_ForwardsArgumentsToNewState() {
            var StateMachine = new TestStackStateMachine();

            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>("PauseArgs");

            Assert.AreEqual("PauseArgs", PauseState.ReceivedArgs);
        }

        [Test]
        public void PopState_RemovesTopStateFromStack() {
            var StateMachine = new TestStackStateMachine();

            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>();

            StateMachine.TryPopState();

            Assert.AreEqual(1, StateMachine.StatesStack.Count);
            Assert.AreEqual(typeof(IdleState), StateMachine.StatesStack.First());
        }

        [Test]
        public void PopState_ReturnsToPreviousState() {
            var StateMachine = new TestStackStateMachine();

            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>();

            StateMachine.TryPopState();

            Assert.AreEqual(IdleState, StateMachine.CurrentState);
        }

        [Test]
        public void PopState_EntersPreviousStateAgain() {
            var StateMachine = new TestStackStateMachine();

            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState);

            StateMachine.StartStateMachine<IdleState>();

            IdleState.Entered = false;

            StateMachine.ApplyState<PauseState>();

            StateMachine.TryPopState();

            Assert.IsTrue(IdleState.Entered);
        }

        [Test]
        public void PopState_ExitsCurrentTopState() {
            var StateMachine = new TestStackStateMachine();

            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>();

            StateMachine.TryPopState();

            Assert.IsTrue(PauseState.Exited);
        }

        [Test]
        public void MultipleApplyState_CreatesCorrectStackOrder() {
            var StateMachine = new TestStackStateMachine();

            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);
            var MenuState = new MenuState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState, MenuState);

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
            var StateMachine = new TestStackStateMachine();

            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);
            var MenuState = new MenuState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState, MenuState);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.ApplyState<PauseState>();

            StateMachine.ApplyState<MenuState>();

            StateMachine.TryPopState();

            Assert.AreEqual(PauseState, StateMachine.CurrentState);
        }
        
        [Test]
        public void TryPopState_WithMultipleStatesOnStack_RemovesTopState() {
            var StateMachine = new TestStackStateMachine();
            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState);
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<PauseState>();
            
            var Result = StateMachine.TryPopState();
            
            Assert.IsTrue(Result);
            Assert.AreEqual(IdleState, StateMachine.CurrentState);
        }

        [Test]
        public void TryPopState_WithOnlyRootState_ReturnsFalse() {
            var StateMachine = new TestStackStateMachine();
            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);
            StateMachine.StartStateMachine<IdleState>();
            
            var Result = StateMachine.TryPopState();
            
            Assert.IsFalse(Result);
        }

        [Test]
        public void TryPopState_WithOnlyRootState_PreservesCurrentState() {
            var StateMachine = new TestStackStateMachine();
            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);
            StateMachine.StartStateMachine<IdleState>();
            
            StateMachine.TryPopState();
            
            Assert.AreEqual(IdleState, StateMachine.CurrentState);
        }

        [Test]
        public void TryPopState_WithOnlyRootState_DoesNotExitState() {
            var StateMachine = new TestStackStateMachine();
            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);
            StateMachine.StartStateMachine<IdleState>();
            
            StateMachine.TryPopState();
            
            Assert.IsFalse(IdleState.Exited);
        }

        [Test]
        public void TryPopState_AfterMultiplePushes_ReturnsToCorrectPreviousState() {
            var StateMachine = new TestStackStateMachine();
            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);
            var MenuState = new MenuState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState, MenuState);
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<PauseState>();
            StateMachine.ApplyState<MenuState>();
            
            var Result = StateMachine.TryPopState();
            
            Assert.IsTrue(Result);
            Assert.AreEqual(PauseState, StateMachine.CurrentState);
        }

        [Test]
        public void TryPopState_CallsExitOnPoppedState() {
            var StateMachine = new TestStackStateMachine();
            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState);
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<PauseState>();
            
            StateMachine.TryPopState();
            
            Assert.IsTrue(PauseState.Exited);
        }

        [Test]
        public void TryPopState_CallsEnterOnPreviousState() {
            var StateMachine = new TestStackStateMachine();
            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState);
            StateMachine.StartStateMachine<IdleState>();

            IdleState.Entered = false;

            StateMachine.ApplyState<PauseState>();
            StateMachine.TryPopState();
            
            Assert.IsTrue(IdleState.Entered);
        }
        
        [Test]
        public void CanPop_WithOnlyRootState_ReturnsFalse() {
            var StateMachine = new TestStackStateMachine();
            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);
            StateMachine.StartStateMachine<IdleState>();

            Assert.IsFalse(StateMachine.CanPop);
        }

        [Test]
        public void CanPop_WithMultipleStatesOnStack_ReturnsTrue() {
            var StateMachine = new TestStackStateMachine();
            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState);
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<PauseState>();

            Assert.IsTrue(StateMachine.CanPop);
        }

        [Test]
        public void CanPop_AfterTryPopState_ReturnsFalse() {
            var StateMachine = new TestStackStateMachine();
            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState);
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<PauseState>();
            StateMachine.TryPopState();

            Assert.IsFalse(StateMachine.CanPop);
        }

        [Test]
        public void CanPop_IsConsistentWithTryPopStateReturnValue() {
            var StateMachine = new TestStackStateMachine();
            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState);
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<PauseState>();

            Assert.AreEqual(StateMachine.CanPop, StateMachine.TryPopState());
        }
        
        [Test]
        public void StatesStack_IsExposedAsReadOnlyCollection() {
            var StateMachine = new TestStackStateMachine();
            var IdleState = new IdleState(StateMachine);
            StateMachine.SetStates(IdleState);
            StateMachine.StartStateMachine<IdleState>();
            
            IReadOnlyCollection<Type> Stack = StateMachine.StatesStack;

            Assert.AreEqual(1, Stack.Count);
        }
        
                
        [Test]
        public void TryPopState_ForwardsArgumentsToPreviousState() {
            var StateMachine = new TestStackStateMachine();
            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState);
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<PauseState>();

            StateMachine.TryPopState("ResumeContext");

            Assert.AreEqual("ResumeContext", IdleState.ReceivedArgs);
        }

        [Test]
        public void TryPopState_WithoutArguments_PassesNullToPreviousState() {
            var StateMachine = new TestStackStateMachine();
            var IdleState = new IdleState(StateMachine);
            var PauseState = new PauseState(StateMachine);

            StateMachine.SetStates(IdleState, PauseState);
            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<PauseState>();

            // Poison the field so we can detect that pop overwrites it with null.
            IdleState.ReceivedArgs = new object();

            StateMachine.TryPopState();

            Assert.IsNull(IdleState.ReceivedArgs);
        }

        [Test]
        public void TryPopState_AtRoot_DoesNotInvokePreviousStateWithArgs() {
            var StateMachine = new TestStackStateMachine();
            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);
            StateMachine.StartStateMachine<IdleState>();

            IdleState.ReceivedArgs = null;
            IdleState.Entered = false;

            var Result = StateMachine.TryPopState("ShouldBeIgnored");

            Assert.IsFalse(Result);
            Assert.IsFalse(IdleState.Entered);
            Assert.IsNull(IdleState.ReceivedArgs);
        }
    }
}