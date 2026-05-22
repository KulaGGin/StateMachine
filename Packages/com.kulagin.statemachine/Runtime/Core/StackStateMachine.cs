using System;
using System.Collections.Generic;

namespace Kulagin.StateMachine.Core {
    public abstract class StackStateMachine<TStateMachine, TStateClass> : StateMachine<TStateMachine, TStateClass>
        where TStateMachine : StateMachine<TStateMachine, TStateClass>
        where TStateClass : State<TStateMachine, TStateClass> {
        public readonly Stack<Type> StatesStack = new();

        public override void StartStateMachine(Type StartingState) {
            StatesStack.Push(StartingState);
            base.StartStateMachine(StartingState);
        }

        public override void ApplyState(Type StateID, object StateEventArgs = null) {
            StatesStack.Push(StateID);
            base.ApplyState(StateID, StateEventArgs);
        }

        public void PopState() {
            StatesStack.Pop();
            base.ApplyState(StatesStack.Peek());
        }
    }
}