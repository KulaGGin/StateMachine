using Kulagin.StateMachine.Core;
using System;

namespace Kulagin.StateMachine.Unity {
    public abstract class UnityStateMachine<TStateMachine, TStateClass> :
        StateMachine<TStateMachine, TStateClass>
        where TStateMachine : UnityStateMachine<TStateMachine, TStateClass>
        where TStateClass : UnityState<TStateMachine, TStateClass> {
        public virtual void Awake() {
            CurrentState.Awake();
        }

        public virtual void Start() {
            CurrentState.Start();
        }

        public virtual void PhysicsUpdate() {
            CurrentState.PhysicsUpdate();
        }

        public virtual void FrameUpdate() {
            CurrentState.FrameUpdate();
        }

        public virtual void LateUpdate() {
            CurrentState.LateUpdate();
        }
    }
}