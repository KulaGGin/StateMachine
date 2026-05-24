using Kulagin.StateMachine.Core;

namespace Kulagin.StateMachine.Unity {
    public abstract class UnityState<TStateMachine, TState> :
        State<TStateMachine, TState>
        where TState : State<TStateMachine, TState>
        where TStateMachine : StateMachine<TStateMachine, TState> {
        protected UnityState(TStateMachine StateMachine) : base(StateMachine) {
        }

        public virtual void Awake() {
        }

        public virtual void Start() {
        }

        public virtual void FrameUpdate() {
        }

        public virtual void PhysicsUpdate() {
        }

        public virtual void LateUpdate() {
        }
    }
}