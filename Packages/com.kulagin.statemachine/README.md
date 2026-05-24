# Kulagin State Machine

A generic, strongly-typed state machine framework for Unity 6.

## Features

- Type-safe state transitions via generics — no string keys, no casting.
- Push-down automaton (`StackStateMachine`) for paused/modal flows.
- Unity-agnostic core (`Kulagin.StateMachine.Core`) plus thin Unity integration layer.
- Zero runtime dependencies.

## Installation

In Unity → Package Manager → `+` → **Install package from git URL**:

```
https://www.github.com/KulaGGin/StateMachine.git?path=Packages/com.kulagin.statemachine
```

Or add to `Packages/manifest.json`:

```json
{
    "dependencies": {
        "com.kulagin.statemachine": "https://www.github.com/KulaGGin/StateMachine.git?path=Packages/com.kulagin.statemachine"
    }
}
```

Requires Unity 6000.0 or later.

## Quick start

```csharp
using Kulagin.StateMachine.Core;

public class PlayerStateMachine : StateMachine<PlayerStateMachine, PlayerState> { }

public abstract class PlayerState : State<PlayerStateMachine, PlayerState> {
    protected PlayerState(PlayerStateMachine StateMacine) : base(StateMacine) { }
}

public class IdleState : PlayerState {
    public IdleState(PlayerStateMachine StateMacine) : base(StateMacine) { }
    public override void EnterState(object args = null) { /* idle setup */ }
}

public class WalkingState : PlayerState {
    public WalkingState(PlayerStateMachine StateMacine) : base(StateMacine) { }
}

// Usage:
var StateMacine = new PlayerStateMachine();
StateMacine.SetStates(new IdleState(StateMacine), new WalkingState(StateMacine));
StateMacine.StartStateMachine<IdleState>();
StateMacine.ApplyState<WalkingState>();
```

## Stack state machine

For modal flows (pause menus, dialog screens):

```csharp
var StateMacine = new GameStateMachine();
StateMacine.SetStates(new PlayingState(StateMacine), new PausedState(StateMacine));
StateMacine.StartStateMachine<PlayingState>();

StateMacine.ApplyState<PausedState>();   // pushes onto stack
StateMacine.TryPopState();               // returns to PlayingState
```

## Layers

- `Kulagin.StateMachine.Core` — engine-agnostic. Pure C#.
- `Kulagin.StateMachine.Unity` — adds `UnityStateMachine` and `GameplayState` with `Awake/Start/FrameUpdate/PhysicsUpdate/LateUpdate` hooks.

## License

[MIT](LICENSE.md)