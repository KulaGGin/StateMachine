using System;
using System.Collections.Generic;
using Kulagin.StateMachine.Core;

namespace Kulagin.StateMachine.Unity {
    public abstract class HierarchicalUnityStateMachine<TStateMachine, TState> : HierarchicalStateMachine<TStateMachine, TState>
        where TStateMachine : HierarchicalUnityStateMachine<TStateMachine, TState>
        where TState : HierarchicalUnityState<TStateMachine, TState> {

        public void PhysicsUpdate() => Broadcast(static State => State.PhysicsUpdate());
        public void FrameUpdate()   => Broadcast(static State => State.FrameUpdate());
        public void LateUpdate()    => Broadcast(static State => State.LateUpdate());
        public void Awake()         => Broadcast(static State => State.Awake());
        public void Start()         => Broadcast(static State => State.Start());

        void Broadcast(Action<TState> Call) {
            TState Leaf = CurrentState;
            List<TState> Path = PathFromRoot(Leaf);
            for (int Index = Path.Count - 1; Index >= 0; Index--) {   // leaf → root
                Call(Path[Index]);
                if (CurrentState != Leaf) return;                      // a state transitioned this tick → stop
            }
        }
    }
}