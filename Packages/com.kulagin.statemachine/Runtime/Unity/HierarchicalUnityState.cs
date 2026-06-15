using Kulagin.StateMachine.Core;

namespace Kulagin.StateMachine.Unity {
    public abstract class HierarchicalUnityState<TStateMachine, TState> : HierarchicalState<TStateMachine, TState>
        where TStateMachine : HierarchicalStateMachine<TStateMachine, TState>
        where TState : HierarchicalState<TStateMachine, TState> {
        protected HierarchicalUnityState(TStateMachine StateMachine) : base(StateMachine) { }

        public virtual void Awake() { }
        public virtual void Start() { }
        public virtual void FrameUpdate() { }
        public virtual void PhysicsUpdate() { }
        public virtual void LateUpdate() { }
    }
}