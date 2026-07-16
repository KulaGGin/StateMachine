using System;
using System.Collections.Generic;

namespace Kulagin.StateMachine.Core {
    public abstract class HierarchicalState<TStateMachine, TState> : State<TStateMachine, TState>
        where TStateMachine : HierarchicalStateMachine<TStateMachine, TState>
        where TState : HierarchicalState<TStateMachine, TState> {

        protected HierarchicalState(TStateMachine StateMachine) : base(StateMachine) {
        }

        public virtual bool HandleEvent(object Event) {
            return false;
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
            List<TState> Path = PathFromRoot(NewState);
            object Arg = null;
            for (int Index = Path.Count - 1; Index >= 0; Index--) {
                Arg = Path[Index].EnterState(Arg);
            }
        }

        protected List<TState> PathFromRoot(TState State) {
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
            
            CurrentState = TargetState;
            
            
            object Arg = StateEventArgs;
            for (int Index = TargetPath.Count - 1; Index >= LowestCommonAncestorIndex; Index--) {
                Arg = TargetPath[Index].EnterState(Arg);
            }
        }
        
        public override bool Send<TEvent>(TEvent Event) {
            Type CurrentType = CurrentState.GetType();
            while (CurrentType != null) {
                if (States[CurrentType] is IHandle<TEvent> Handler && Handler.Handle(Event)) {
                    return true;                              // handled → stop
                }
                Parents.TryGetValue(CurrentType, out CurrentType);   // not mine → ask parent
            }
            return false;
        }
    }
}