# StateMachineBuddy

A finite state machine library for .NET and MonoGame projects. Define states and messages, configure transitions between states, send messages to trigger transitions, and subscribe to events when state changes occur.

## Installation

Install via NuGet:

```bash
dotnet add package StateMachineBuddy
```

**NuGet Package:** `StateMachineBuddy`
**Version:** 5.0.2
**Target Framework:** .NET 8.0
**License:** MIT

### Dependencies

- `MonoGame.Framework.DesktopGL` (3.8.*)
- `FilenameBuddy` (5.*)
- `XmlBuddy` (5.*)

## Overview

StateMachineBuddy provides three state machine implementations:

| Class | State Type | Message Type | Use Case |
|-------|-----------|--------------|----------|
| `IntStateMachine` | `int` | `int` | High-performance scenarios with integer indices |
| `StringStateMachine` | `string` | `string` | Flexibility with string-based states |
| `EnumStateMachine<TState, TMessage>` | Enum | Enum | Type-safe enum-based state machines |

## Core Concepts

- **State**: A discrete condition the machine can be in
- **Message**: An input that may trigger a state transition
- **Transition**: A rule defining: when in state X and receiving message Y, move to state Z
- **Initial State**: The starting state of the machine

## Quick Start

### Using EnumStateMachine (Recommended)

```csharp
using StateMachineBuddy;

// Define your states and messages as enums
public enum PlayerState { Idle, Walking, Running, Jumping }
public enum PlayerMessage { Walk, Run, Jump, Stop, Land }

// Create the state machine
var fsm = new EnumStateMachine<PlayerState, PlayerMessage>(PlayerState.Idle);

// Define transitions
fsm.Set(PlayerState.Idle, PlayerMessage.Walk, PlayerState.Walking);
fsm.Set(PlayerState.Idle, PlayerMessage.Run, PlayerState.Running);
fsm.Set(PlayerState.Idle, PlayerMessage.Jump, PlayerState.Jumping);
fsm.Set(PlayerState.Walking, PlayerMessage.Stop, PlayerState.Idle);
fsm.Set(PlayerState.Walking, PlayerMessage.Run, PlayerState.Running);
fsm.Set(PlayerState.Running, PlayerMessage.Stop, PlayerState.Idle);
fsm.Set(PlayerState.Jumping, PlayerMessage.Land, PlayerState.Idle);

// Subscribe to state changes
fsm.StateChangedEvent += (sender, args) =>
{
    Console.WriteLine($"State changed from {args.OldState} to {args.NewState}");
};

// Send messages to trigger transitions
fsm.SendStateMessage("Walk");  // Transitions from Idle to Walking
fsm.SendStateMessage("Run");   // Transitions from Walking to Running
fsm.SendStateMessage("Stop");  // Transitions from Running to Idle
```

### Using IntStateMachine (Performance-Optimized)

```csharp
using StateMachineBuddy;

public enum GameState { Menu, Playing, Paused, GameOver }
public enum GameMessage { Start, Pause, Resume, Die, Restart }

var fsm = new IntStateMachine();

// Initialize with enum types
fsm.Set(typeof(GameState), typeof(GameMessage), (int)GameState.Menu);

// Define transitions using integer indices
fsm.SetEntry((int)GameState.Menu, (int)GameMessage.Start, (int)GameState.Playing);
fsm.SetEntry((int)GameState.Playing, (int)GameMessage.Pause, (int)GameState.Paused);
fsm.SetEntry((int)GameState.Playing, (int)GameMessage.Die, (int)GameState.GameOver);
fsm.SetEntry((int)GameState.Paused, (int)GameMessage.Resume, (int)GameState.Playing);
fsm.SetEntry((int)GameState.GameOver, (int)GameMessage.Restart, (int)GameState.Menu);

// Subscribe to state changes
fsm.StateChangedEvent += (sender, args) =>
{
    Console.WriteLine($"Changed from {fsm.GetStateName(args.OldState)} to {fsm.GetStateName(args.NewState)}");
};

// Send messages
fsm.SendStateMessage((int)GameMessage.Start);  // Menu -> Playing
```

### Using StringStateMachine

```csharp
using StateMachineBuddy;

var fsm = new StringStateMachine();

// Add states and messages
fsm.AddStates(new[] { "idle", "active", "complete" });
fsm.AddMessages(new[] { "activate", "finish", "reset" });
fsm.SetInitialState("idle");

// Define transitions
fsm.Set("idle", "activate", "active");
fsm.Set("active", "finish", "complete");
fsm.Set("complete", "reset", "idle");

// Subscribe to state changes
fsm.StateChangedEvent += (sender, args) =>
{
    Console.WriteLine($"Changed from {args.OldState} to {args.NewState}");
};

// Send messages
fsm.SendStateMessage("activate");  // idle -> active
```

## API Reference

### IntStateMachine

High-performance state machine using integer indices internally stored in a 2D array.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `InitialState` | `int` | The starting state index |
| `InitialStateName` | `string` | Name of the initial state |
| `CurrentState` | `int` | Current state index |
| `CurrentStateName` | `string` | Name of current state |
| `PrevState` | `int` | Previous state index |
| `NumStates` | `int` | Total number of states |
| `NumMessages` | `int` | Total number of messages |
| `StateNames` | `string[]` | Array of state names |
| `MessageNames` | `string[]` | Array of message names |

#### Events

| Event | Args Type | Description |
|-------|-----------|-------------|
| `StateChangedEvent` | `StateChangeEventArgs<int>` | Fired when state changes via message or force |
| `ResetEvent` | `StateChangeEventArgs<int>` | Fired when `ResetToInitialState()` is called |

#### Initialization Methods

| Method | Description |
|--------|-------------|
| `Set(int numStates, int numMessages, int initialState = 0)` | Initialize with counts and initial state |
| `Set(Type statesEnum, Type messagesEnum, int initialState = 0)` | Initialize from enum types |
| `SetEntry(int state, int message, int nextState)` | Define a transition |
| `SetEntry(int state, string messageName, string nextStateName)` | Define transition using names |
| `SetStateName(int state, string name)` | Set name for a state index |
| `SetMessageName(int message, string name)` | Set name for a message index |
| `SetNames(Type enumType, bool isStates)` | Set names from enum |
| `Resize(int numStates, int numMessages)` | Resize the state machine |
| `RemoveState(int state)` | Remove a state |
| `RemoveMessage(int message)` | Remove a message |

#### Runtime Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `SendStateMessage(int message)` | `bool` | Send message; returns true if state changed |
| `ForceState(int state)` | `void` | Force transition to state |
| `ResetToInitialState()` | `void` | Reset to initial state |

#### Lookup Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `GetStateFromName(string name)` | `int` | Get state index from name (-1 if not found) |
| `GetMessageFromName(string name)` | `int` | Get message index from name (-1 if not found) |
| `GetStateName(int state)` | `string` | Get state name from index |
| `GetMessageName(int message)` | `string` | Get message name from index |
| `GetEntry(int state, int message)` | `int` | Get target state for a transition |
| `Compare(IntStateMachine other)` | `bool` | Check equality with another machine |

#### File I/O

| Method | Description |
|--------|-------------|
| `LoadXml(Filename file, ContentManager content)` | Load from XML via MonoGame content pipeline |

---

### StringStateMachine

Flexible state machine using string-based states and messages.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `InitialState` | `string` | The starting state |
| `CurrentState` | `string` | Current state |
| `PrevState` | `string` | Previous state |
| `StateTable` | `Dictionary<string, State>` | All states and their transitions |
| `Messages` | `HashSet<string>` | All valid message names |
| `States` | `HashSet<string>` | All valid state names |

#### Events

| Event | Args Type | Description |
|-------|-----------|-------------|
| `StateChangedEvent` | `StateChangeEventArgs<string>` | Fired when state changes |
| `ResetEvent` | `StateChangeEventArgs<string>` | Fired on reset |

#### Initialization Methods

| Method | Description |
|--------|-------------|
| `StringStateMachine()` | Default constructor |
| `StringStateMachine(StringStateMachine source)` | Copy constructor |
| `SetInitialState(string state)` | Set the initial state |
| `AddStates(IEnumerable<string> states)` | Add multiple states |
| `AddStates(Type statesEnum)` | Add states from enum |
| `AddMessages(IEnumerable<string> messages)` | Add multiple messages |
| `AddMessages(Type messagesEnum)` | Add messages from enum |
| `AddStateMachine(Type statesEnum, Type messagesEnum, string initialState)` | Full initialization from enums |
| `Set(string state, string message, string nextState)` | Define a transition |

#### Runtime Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `SendStateMessage(string message)` | `bool` | Send message; returns true if state changed |
| `ForceState(string state)` | `void` | Force transition to state |
| `ResetToInitialState()` | `void` | Reset to initial state |

#### File I/O

| Method | Description |
|--------|-------------|
| `LoadXml(Filename file, ContentManager content)` | Load from XML |
| `LoadStateMachine(StateMachineModel model)` | Load from model object |

---

### EnumStateMachine<TState, TMessage>

Type-safe wrapper around StringStateMachine for enum-based states and messages. Inherits from `StringStateMachine`.

#### Constructor

```csharp
EnumStateMachine(TState initialState)
```

#### Methods

| Method | Description |
|--------|-------------|
| `Set(TState state, TMessage message, TState nextState)` | Define a transition using enum values |

Inherits all properties, events, and methods from `StringStateMachine`. Use string versions of enum names when calling inherited methods like `SendStateMessage("EnumValueName")`.

---

### StateChangeEventArgs<T>

Event arguments passed to state change events.

| Property | Type | Description |
|----------|------|-------------|
| `OldState` | `T` | The previous state |
| `NewState` | `T` | The new current state |

---

### State

Internal class representing a state and its transitions.

| Property | Type | Description |
|----------|------|-------------|
| `StateChanges` | `Dictionary<string, string>` | Map of message -> target state |

---

## XML Configuration

State machines can be loaded from XML files using the MonoGame content pipeline.

### XML Schema

```xml
<StateMachine initial="InitialStateName">
  <states>
    <state name="State1" />
    <state name="State2" />
    <state name="State3" />
  </states>
  <messages>
    <message name="Message1" />
    <message name="Message2" />
  </messages>
  <stateChanges>
    <state name="State1">
      <transitions>
        <transition message="Message1" state="State2" />
        <transition message="Message2" state="State3" />
      </transitions>
    </state>
    <state name="State2">
      <transitions>
        <transition message="Message1" state="State3" />
      </transitions>
    </state>
  </stateChanges>
</StateMachine>
```

### Loading XML

```csharp
var fsm = new StringStateMachine();
fsm.LoadXml(new Filename("path/to/statemachine.xml"), contentManager);
```

## Model Classes

For programmatic construction or serialization:

### StateMachineModel

| Property | Type | Description |
|----------|------|-------------|
| `Initial` | `string` | Initial state name |
| `StateNames` | `List<string>` | All state names |
| `MessageNames` | `List<string>` | All message names |
| `States` | `List<StateModel>` | State transition definitions |

### StateModel

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | State name |
| `Transitions` | `List<StateChangeModel>` | Transitions from this state |

### StateChangeModel

| Property | Type | Description |
|----------|------|-------------|
| `Message` | `string` | Message that triggers this transition |
| `TargetState` | `string` | State to transition to |

## Behavior Notes

1. **No transition defined**: If no transition exists for the current state + message combination, `SendStateMessage` returns `false` and the state remains unchanged.

2. **Self-transitions ignored**: If a transition would result in the same state, no state change occurs and `SendStateMessage` returns `false`.

3. **ForceState**: Bypasses the transition table and directly sets the state. Fires `StateChangedEvent` if the state actually changes.

4. **Invalid states/messages**: `Set()` throws exceptions if states or messages haven't been added first.

5. **Reset behavior**: `ResetToInitialState()` fires `ResetEvent` instead of `StateChangedEvent`.

## Project Structure

```
StateMachineBuddy/
├── IntStateMachine.cs       # Performance-optimized integer-based FSM
├── StringStateMachine.cs    # Flexible string-based FSM
├── EnumStateMachine.cs      # Type-safe enum wrapper
├── State.cs                 # State with transitions dictionary
├── StateChangeEventArgs.cs  # Event args for state changes
└── Models/
    ├── StateMachineModel.cs   # XML serialization model
    ├── StateModel.cs          # State definition model
    └── StateChangeModel.cs    # Transition definition model
```

## Repository

**GitHub:** https://github.com/dmanning23/StateMachine

## License

MIT License