using System;
using System.Collections.Generic;

namespace Kulagin.StateMachine.Core {
    public class StateMachine<TStateMachine, TStateClass>
        where TStateMachine : StateMachine<TStateMachine, TStateClass>
        where TStateClass : State<TStateMachine, TStateClass> {
        protected Dictionary<Type, TStateClass> States = new();

        public TStateClass CurrentState { get; protected set; }

        protected StateMachine(IEnumerable<TStateClass> StartStates) {
            SetStates(StartStates);
        }

        protected StateMachine(params TStateClass[] StartStates) {
            SetStates(StartStates);
        }

        public void SetStates(IEnumerable<TStateClass> NewStates) {
            States.Clear();
            foreach (TStateClass State in NewStates) {
                States[State.GetType()] = State;
            }
        }

        public void SetStates(params TStateClass[] NewStates) {
            SetStates((IEnumerable<TStateClass>)NewStates);
        }

        public virtual void StartStateMachine<T>()
            where T : TStateClass {
            StartStateMachine(typeof(T));
        }

        public virtual void StartStateMachine(Type StartingState) {
            if (StartingState == null)
                throw new ArgumentNullException(nameof(StartingState), "Starting state cannot be null.");

            if (!States.TryGetValue(StartingState, out var State)) {
                throw new ArgumentException(
                    $"State '{StartingState.Name}' is not registered in this state machine. " +
                    $"Register it first using SetStates().",
                    nameof(StartingState)
                );
            }

            CurrentState = State;
            CurrentState.EnterState();
        }

        public bool IsInState<T>() {
            return IsInState(typeof(T));
        }

        public bool IsInState(Type State) {
            return States.TryGetValue(State, out var state) && CurrentState == state;
        }

        // Remember: boxing for structs.
        // Heap allocation when a struct or primitive is supplied.
        public virtual void ApplyState<T>(object StateEventArgs = null)
            where T : TStateClass {
            ApplyState(typeof(T), StateEventArgs);
        }
        
        public virtual void ApplyState(Type StateID, object StateEventArgs = null) {
            if (StateID == null) {
                throw new ArgumentNullException(nameof(StateID), "Target state cannot be null.");
            }

            if (!States.TryGetValue(StateID, out var NewState)) {
                throw new ArgumentException(
                    $"State '{StateID.Name}' is not registered in this state machine. " +
                    $"Register it first using SetStates().",
                    nameof(StateID)
                );
            }

            CurrentState.ExitState();
            CurrentState = NewState;
            CurrentState.EnterState(StateEventArgs);
        }
        
        public virtual bool Send<TEvent>(TEvent Event) {
            return CurrentState is IHandle<TEvent> Handler && Handler.Handle(Event);
        }
    }
}