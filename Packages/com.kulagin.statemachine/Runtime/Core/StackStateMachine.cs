using System;
using System.Collections.Generic;

namespace Kulagin.StateMachine.Core {
    public abstract class StackStateMachine<TStateMachine, TStateClass> : StateMachine<TStateMachine, TStateClass>
        where TStateMachine : StateMachine<TStateMachine, TStateClass>
        where TStateClass : State<TStateMachine, TStateClass> {
        private readonly Stack<Type> _StatesStack = new();
        public IReadOnlyCollection<Type> StatesStack => _StatesStack;

        public bool CanPop => _StatesStack.Count > 1;

        public override void StartStateMachine(Type StartingState) {
            base.StartStateMachine(StartingState);
            _StatesStack.Push(StartingState);
        }

        public override void ApplyState(Type StateID, object StateEventArgs = null) {
            base.ApplyState(StateID, StateEventArgs);
            _StatesStack.Push(StateID);
        }

        /// <summary>
        ///  <inheritdoc cref="StateMachine{TStateMachine, TState}.ApplyState(System.Type, object)"/>
        /// </summary>
        public bool TryPopState(object StateEventArgs = null) {
            if (!CanPop) {
                return false;
            }

            _StatesStack.Pop();
            base.ApplyState(_StatesStack.Peek(), StateEventArgs);
            return true;
        }
    }
}