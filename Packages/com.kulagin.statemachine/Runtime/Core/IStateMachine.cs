using System;

namespace Kulagin.StateMachine.Core {
    // TODO Possibly add the Generic ApplyState<T>, StartStateMachine<T> etc.
    public interface IStateMachine {
        void StartStateMachine(Type StartingState);
        bool Send<TEvent>(TEvent Event);
        bool IsInState(Type State);
        Type CurrentStateType { get; }
    }
}