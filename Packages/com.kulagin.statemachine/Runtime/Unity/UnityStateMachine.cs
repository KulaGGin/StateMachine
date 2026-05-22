using Kulagin.StateMachine.Core;
using System;

namespace Kulagin.StateMachine.Unity {
    public abstract class UnityStateMachine<TStateMachine, TStateClass> :
        StateMachine<TStateMachine, TStateClass>
        where TStateMachine : UnityStateMachine<TStateMachine, TStateClass>
        where TStateClass : GameplayState<TStateMachine, TStateClass> {
        public virtual void Awake() {
            EnsureStarted();
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

        private void EnsureStarted() {
            if (CurrentState == null)
                throw new InvalidOperationException("StateMachine not started.");
        }
    }
}