using System.Collections.Immutable;
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
        await InvokeStateExistHandlers();

        var handler =await GetNextHandler(@event, _transitions[_context.CurrentState.Tag][@event.Tag]);
        _context.CurrentState = await handler.HandleTransition(_context, _context.CurrentState, @event);

        try
        {
            await InvokeStateEntranceHandlers();
        }
        catch (Exception e)
        {
            await handler.CompensateTransition(_context, shadowContext.CurrentState,_context.CurrentState, @event, e);
            _context = shadowContext;
            throw;
        }
    }

    private async Task InvokeStateExistHandlers()
    {

        var stateExistHandlerType =
            typeof(IStateExistHandler<,>).MakeGenericType(typeof(TContext), _context.CurrentState.GetType());

        foreach (var stateExistHandler in _serviceProvider.GetServices(stateExistHandlerType))
        {
            if (stateExistHandler == null) continue;
            var result = stateExistHandler.GetType().GetMethod("HandleExist")
                ?.Invoke(stateExistHandler, new object[] { _context, _context.CurrentState });
            await (Task)result!;
        }
    }

    private async Task InvokeStateEntranceHandlers()
    {

        var stateEntranceHandlerType =
            typeof(IStateEntranceHandler<,>).MakeGenericType(typeof(TContext), _context.CurrentState.GetType());

        foreach (var stateEntranceHandler in _serviceProvider.GetServices(stateEntranceHandlerType))
        {
            if (stateEntranceHandler == null) continue;
            var result = stateEntranceHandler.GetType().GetMethod("HandleEntrance")
                ?.Invoke(stateEntranceHandler, new object[] { _context, _context.CurrentState });
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