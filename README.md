# Transiflow

A C# implementation of a generic, flexible state machine with support for validating and handling state transitions, as well as custom entrance and exit actions for each state.

## Features

- Generic implementation to support any state, event, and context types.
- Extensible transition handling with custom actions.
- Entrance and exit actions for each state.
- Easy integration with dependency injection and extensibility through `IServiceProvider`.

## Getting Started

1. Install the required dependencies:

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Transiflow;
```
2-Include the provided state machine and related interfaces in your project.

3-Create your state, event, and context classes that implement IState<TStateTag>, IEvent<TEventTag>, and IContext<TState, TStateTag> interfaces, respectively.

4-Implement the necessary ITransitionHandler<TContext, TState, TEvent, TNewState>, IStateEntranceHandler<TContext, TState>, and IStateExistHandler<TContext, TState> classes for your state machine.

5-Set up your state machine configuration
    
```csharp
var services = new ServiceCollection();
ServiceCollection.AddScoped<ICodec<Context>, ContextCodec>
var serviceProvider = services.BuildServiceProvider();

var stateMachine = new StateMachine<TState, TStateTag, TEvent, TEventTag, TContext>(serviceProvider)
.AddTransition<TCurrentState, TTriggerEvent, TNewState>(sourceStateTag, eventTag, (sp, ctx) => new TransitionHandler());
```
6-Create a service instance for your state machine:
    
 ```csharp
var stateMachineService = stateMachine.CreateService(context);
```
7-Use the SendEvent method to trigger state transitions:
        
```csharp
stateMachineService.SendEvent(new TEvent());
``` 
## Custom Entrance and Exit Actions

TransiFlow supports executing custom entrance and exit actions for each state by implementing the `IStateEntranceHandler<TContext, TState>` and `IStateExistHandler<TContext, TState>` interfaces, respectively.

### IStateEntranceHandler

Implement this interface to define custom actions that should be executed when entering a specific state.

```csharp
public class CustomStateEntranceHandler : IStateEntranceHandler<TContext, TState>
{
    public async Task HandleEntrance(TContext context, TState state)
    {
        // Your custom entrance action logic here
    }
}
```
### IStateExistHandler
Implement this interface to define custom actions that should be executed when exiting a specific state.

```csharp
public class CustomStateExitHandler : IStateExitHandler<TContext, TState>
{
    public async Task HandleExit(TContext context, TState state)
    {
        // Your custom exit action logic here
    }
}
```
Register your custom entrance and exit handlers with the dependency injection container to ensure they are executed during the state transitions.

```csharp
var serviceCollection = new ServiceCollection();

serviceCollection
    .AddSingleton<IStateExistHandler<TContext, TState>, CustomStateExitHandler>()
    .AddSingleton<IStateEntranceHandler<TContext, TState>, CustomStateEntranceHandler>();

var serviceProvider = serviceCollection.BuildServiceProvider();
```
## Examples
Check the unit tests provided in the project for examples of setting up state machines and using them in various scenarios.

## License
This project is licensed under the MIT License.