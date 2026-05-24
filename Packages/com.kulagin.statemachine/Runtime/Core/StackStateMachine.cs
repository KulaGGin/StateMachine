using System;
using System.Collections.Generic;

namespace Kulagin.StateMachine.Core {
    public abstract class StackStateMachine<TStateMachine, TStateClass> : StateMachine<TStateMachine, TStateClass>
        where TStateMachine : StateMachine<TStateMachine, TStateClass>
        where TStateClass : State<TStateMachine, TStateClass> {
        public readonly Stack<Type> StatesStack = new();

        public bool CanPop => StatesStack.Count > 1;

        public override void StartStateMachine(Type StartingState) {
            StatesStack.Push(StartingState);
            base.StartStateMachine(StartingState);
        }

        public override void ApplyState(Type StateID, object StateEventArgs = null) {
            StatesStack.Push(StateID);
            base.ApplyState(StateID, StateEventArgs);
        }

        public bool TryPopState() {
            if (!CanPop)
                return false;

            StatesStack.Pop();
            base.ApplyState(StatesStack.Peek());
            return true;
        }
    }
}