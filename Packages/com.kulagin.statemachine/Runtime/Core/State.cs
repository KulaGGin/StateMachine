using System;

namespace Kulagin.StateMachine.Core {
    public abstract class State<TStateMachine, TState>
        where TStateMachine : StateMachine<TStateMachine, TState>
        where TState : State<TStateMachine, TState> {
        protected State(TStateMachine StateMachine) {
            this.StateMachine = StateMachine;
        }

        public readonly TStateMachine StateMachine;

        public virtual object EnterState(object StateEventArgs = null) {
            return StateEventArgs;
        }

        public virtual void ExitState() {
        }

        public void ApplyState<TNextState>(object StateEventArgs = null)
            where TNextState : TState {
            ApplyState(typeof(TNextState), StateEventArgs);
        }

        public void ApplyState(Type NextState, object StateEventArgs = null) {
            StateMachine.ApplyState(NextState, StateEventArgs);
        }
    }
}