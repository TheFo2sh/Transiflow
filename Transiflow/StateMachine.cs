using System.Collections.Immutable;

namespace Transiflow;

public class StateMachine<TState, TStateTag, TEvent, TEventTag, TContext> where TState : IState<TStateTag>
    where TEvent : IEvent<TEventTag>
    where TStateTag : notnull
    where TContext:IContext<TState,TStateTag>
{
    
    private readonly IServiceProvider _serviceProvider;
    private readonly
        Dictionary<TStateTag, Dictionary<TEventTag, List<Func<IServiceProvider,TContext,ITransitionHandler<TContext,TState, TEvent,  TState>>>>>
        _transitions = new();


    public StateMachine(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public StateMachine<TState, TStateTag, TEvent, TEventTag, TContext> AddTransition<TCurrentState, TTriggerEvent, TNewState>
    (TStateTag sourceStateTag, TEventTag eventTag,Func<IServiceProvider,TContext,ITransitionHandler<TContext,TCurrentState, TTriggerEvent,  TNewState>> transitionHandler)
        where TCurrentState : TState
        where TTriggerEvent : TEvent
        where TNewState : TState
    
    {
        if (!_transitions.ContainsKey(sourceStateTag))
        {
            _transitions.Add(sourceStateTag, new());
        }
        if (!_transitions[sourceStateTag].ContainsKey(eventTag))
        {
            _transitions[sourceStateTag].Add(eventTag, new());
        }
        
        _transitions[sourceStateTag][eventTag].Add((sp,context)=> new TransitionAdapter< TState,TStateTag, TEvent,TEventTag,TContext,TCurrentState,TTriggerEvent,TNewState>(transitionHandler(sp,context)));
        return this;
    }

    public StateMachineService<TState, TStateTag, TEvent, TEventTag, TContext> CreateService(TContext context)
    {
        return new(_serviceProvider, _transitions.ToImmutableDictionary(), context);
    }
}