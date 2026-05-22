using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kulagin.StateMachine.Core {
    public class StateMachine<TStateMachine, TStateClass>
        where TStateMachine : StateMachine<TStateMachine, TStateClass>
        where TStateClass : State<TStateMachine, TStateClass> {
        protected Dictionary<System.Type, TStateClass> States = new();

        public TStateClass CurrentState { get; private set; }

        public StateMachine() {
        }

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

        public virtual void StartStateMachine(System.Type StartingState) {
            if (!States.ContainsKey(StartingState)) {
                Debug.Log($"Tried to start with a state that's not inside state machine: \"{StartingState}\"");
                return;
            }

            CurrentState = States[StartingState];
            CurrentState.EnterState();
        }

        public bool IsInState<T>() {
            return IsInState(typeof(T));
        }

        public bool IsInState(System.Type State) {
            return CurrentState == States[State];
        }

        public virtual void ApplyState<T>(object StateEventArgs = null)
            where T : TStateClass {
            ApplyState(typeof(T), StateEventArgs);
        }

        public virtual void ApplyState(System.Type StateID, object StateEventArgs = null) {
            if (!States.ContainsKey(StateID)) {
                Debug.Log($"Tried to switch to a state that's not inside state machine: \"{StateID}\"");
                return;
            }

            CurrentState.ExitState();
            var NewState = States[StateID];
            CurrentState = NewState;
            CurrentState.EnterState(StateEventArgs);
        }
    }
}