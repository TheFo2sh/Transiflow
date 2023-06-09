﻿using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Transiflow;

public interface ICodec<TContext>
{
    TContext Copy(TContext context);
    TContext Decode(Byte[] encodedContext);
    Byte[] Encode(TContext context);
}

public class StateMachineService<TState, TStateTag, TEvent, TEventTag, TContext>
    where TState : IState<TStateTag> 
    where TEvent : IEvent<TEventTag>     
    where TContext:IContext<TState,TStateTag>
    where TStateTag : notnull
    where TEventTag : notnull


{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICodec<TContext> _codec;
    private readonly
        ImmutableDictionary<TStateTag, Dictionary<TEventTag, List<Func<IServiceProvider,TContext,ITransitionHandler<TContext,TState, TEvent, TState>>>>>
        _transitions;

    private TContext _context;

    internal StateMachineService(
        IServiceProvider serviceProvider,
        ImmutableDictionary<TStateTag, Dictionary<TEventTag, List<Func<IServiceProvider,TContext,ITransitionHandler<TContext,TState, TEvent,  TState>>>>> transitions,
        TContext context)
    {
        _serviceProvider = serviceProvider;
        _transitions = transitions;
        _context = context;
        _codec = _serviceProvider.GetRequiredService<ICodec<TContext>>();
    }


    public TContext GetContext()
    {
        return _codec.Copy(_context);
    }

    public async Task SendEvent(TEvent @event)
    {
        if (!IsValidTransition(@event))
            throw new InvalidOperationException(
                $"Cannot handle event {@event.Tag} in state {_context.CurrentState.Tag}");

        var shadowContext = _codec.Copy(_context);
        var handler = await GetNextHandler(@event, _transitions[_context.CurrentState.Tag][@event.Tag]);

        try
        {
            await InvokeStateExistHandlers(shadowContext);
            shadowContext.CurrentState = await handler.HandleTransition(shadowContext, shadowContext.CurrentState, @event);
            await InvokeStateEntranceHandlers(shadowContext);
            _context = shadowContext;

        }
        
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            await handler.CompensateTransition(shadowContext, _context.CurrentState, shadowContext.CurrentState, @event, e.InnerException);
            throw e.InnerException;
        }
        catch (Exception e)
        {
            await handler.CompensateTransition(shadowContext, _context.CurrentState, shadowContext.CurrentState, @event, e);
            _context = shadowContext;
            throw;
        }
    }

    private async Task InvokeStateExistHandlers(TContext context)
    {

        var stateExistHandlerType =
            typeof(IStateExistHandler<,>).MakeGenericType(typeof(TContext), context.CurrentState.GetType());

        foreach (var stateExistHandler in _serviceProvider.GetServices(stateExistHandlerType))
        {
            if (stateExistHandler == null) continue;
            var result = stateExistHandler.GetType().GetMethod("HandleExist")
                ?.Invoke(stateExistHandler, new object[] { context, context.CurrentState });
            await (Task)result!;
        }
    }

    private async Task InvokeStateEntranceHandlers(TContext context)
    {

        var stateEntranceHandlerType =
            typeof(IStateEntranceHandler<,>).MakeGenericType(typeof(TContext), context.CurrentState.GetType());

        foreach (var stateEntranceHandler in _serviceProvider.GetServices(stateEntranceHandlerType))
        {
            if (stateEntranceHandler == null) continue;
            var result = stateEntranceHandler.GetType().GetMethod("HandleEntrance")
                ?.Invoke(stateEntranceHandler, new object[] { context, context.CurrentState });
            await (Task)result!;
        }
    }

    private async Task<ITransitionHandler<TContext, TState, TEvent, TState>> GetNextHandler(TEvent @event, List<Func<IServiceProvider, TContext, ITransitionHandler<TContext,TState, TEvent,  TState>>> factories)
    {
        foreach (var factory in factories)
        {
            var handler = factory(_serviceProvider, _context);
            if (await handler.ValidateTransition(_context, _context.CurrentState, @event))
            {
                return  handler;
            }
        }
        throw new Exception("No Handler that match the condition was found");
    }

    private bool IsValidTransition(TEvent @event)
    {
        return _transitions.ContainsKey(_context.CurrentState.Tag) &&
               _transitions[_context.CurrentState.Tag].ContainsKey(@event.Tag);
    }
}