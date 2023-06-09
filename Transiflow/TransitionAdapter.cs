﻿namespace Transiflow;

internal class TransitionAdapter<TState, TStateTag, TEvent, TEventTag, TContext,TCurrentState,TTriggerEvent,TNewState>:ITransitionHandler<TContext,TState, TEvent,  TState>
    where TState : IState<TStateTag>
    where TEvent : IEvent<TEventTag>
    where TStateTag : notnull
    where TContext:IContext<TState,TStateTag>
    where TCurrentState : TState
    where TTriggerEvent : TEvent
    where TNewState : TState
{
    private readonly ITransitionHandler<TContext, TCurrentState, TTriggerEvent, TNewState> _transitionHandler;

    public TransitionAdapter(ITransitionHandler<TContext, TCurrentState, TTriggerEvent, TNewState> transitionHandler)
    {
        _transitionHandler = transitionHandler;
    }

    public  ValueTask<bool> ValidateTransition(TContext context, TState fromState, TEvent @event)
    {
        return _transitionHandler.ValidateTransition(context, (TCurrentState)fromState, (TTriggerEvent)@event);
    }

    public async Task<TState> HandleTransition(TContext context, TState fromState, TEvent @event)
    {
        return await _transitionHandler.HandleTransition(context, (TCurrentState)fromState, (TTriggerEvent)@event);
    }


    public  Task CompensateTransition(TContext context, TState oldState, TState newState, TEvent @event, Exception exception)
    {
         return _transitionHandler.CompensateTransition(context, (TCurrentState)oldState, (TNewState)newState, (TTriggerEvent)@event, exception);
    }


}