using System;
using System.Collections.Generic;

namespace Kulagin.StateMachine.Core {
    public abstract class HierarchicalState<TStateMachine, TState> : State<TStateMachine, TState>
        where TStateMachine : HierarchicalStateMachine<TStateMachine, TState>
        where TState : HierarchicalState<TStateMachine, TState> {

        protected HierarchicalState(TStateMachine StateMachine) : base(StateMachine) {
        }

        public virtual bool HandleEvent(object Event) {
            return false;   // default: nobody handles anything
        }
    }

    public abstract class HierarchicalStateMachine<TStateMachine, TState> : StateMachine<TStateMachine, TState>
        where TStateMachine : HierarchicalStateMachine<TStateMachine, TState>
        where TState : HierarchicalState<TStateMachine, TState> {
        protected Dictionary<Type, Type> Parents = new();

        public void SetParent<TChild, TParent>()
            where TChild : TState
            where TParent : TState {
            Parents[typeof(TChild)] = typeof(TParent);
        }

        public override void StartStateMachine(Type StartingState) {
            if (StartingState == null) {
                throw new ArgumentNullException(nameof(StartingState), "Starting state cannot be null.");
            }
            
            if (!States.TryGetValue(StartingState, out TState NewState)) {
                throw new ArgumentException(
                    $"State '{StartingState.Name}' is not registered in this state machine. " +
                    $"Register it first using SetStates().",
                    nameof(StartingState));
            }
            
            CurrentState = NewState;
            foreach (TState StateInPath in PathFromRoot(NewState)) {
                StateInPath.EnterState();
            }
        }

        List<TState> PathFromRoot(TState State) {
            List<TState> Path = new();
            Type CurrentType = State.GetType();
            while (CurrentType != null) {
                Path.Add(States[CurrentType]);
                Parents.TryGetValue(CurrentType, out CurrentType);
            }
            Path.Reverse();   // we walked leaf → root; we want root → leaf
            return Path;
        }
        
        public override void ApplyState(Type StateID, object StateEventArgs = null) {
            if (StateID == null) {
                throw new ArgumentNullException(nameof(StateID), "Target state cannot be null.");
            }

            if (!States.TryGetValue(StateID, out TState TargetState)) {
                throw new ArgumentException(
                    $"State '{StateID.Name}' is not registered in this state machine. " +
                    $"Register it first using SetStates().",
                    nameof(StateID));
            }

            List<TState> SourcePath = PathFromRoot(CurrentState);
            List<TState> TargetPath = PathFromRoot(TargetState);

            // Walk both paths together; LowestCommonAncestorIndex is the first divergence.
            int LowestCommonAncestorIndex = 0;
            while (LowestCommonAncestorIndex < SourcePath.Count
                   && LowestCommonAncestorIndex < TargetPath.Count
                   && SourcePath[LowestCommonAncestorIndex] == TargetPath[LowestCommonAncestorIndex]) {
                LowestCommonAncestorIndex++;
            }

            // Exit from source leaf UP to (but not past) the Lowest Common Ancestor.
            for (int Index = SourcePath.Count - 1; Index >= LowestCommonAncestorIndex; Index--) {
                SourcePath[Index].ExitState();
            }

            // Enter from Lowest Common Ancestor DOWN to target leaf. Only the leaf gets the args.
            for (int Index = LowestCommonAncestorIndex; Index < TargetPath.Count; Index++) {
                if (Index == TargetPath.Count - 1) {
                    TargetPath[Index].EnterState(StateEventArgs);
                } else {
                    TargetPath[Index].EnterState();
                }
            }

            CurrentState = TargetState;
        }
        
        public void HandleEvent(object Event) {
            Type CurrentType = CurrentState.GetType();
            while (CurrentType != null) {
                if (States[CurrentType].HandleEvent(Event)) return;       // handled → stop
                Parents.TryGetValue(CurrentType, out CurrentType);        // not handled → ask parent
            }
        }
        
        
    }
}