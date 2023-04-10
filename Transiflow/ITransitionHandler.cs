namespace Transiflow;

public interface ITransitionHandler<TContext,TState, TEvent, TNewState>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="fromState"></param>
    /// <param name="event"></param>
    /// <returns></returns>
    ValueTask<bool> ValidateTransition(TContext context, TState fromState, TEvent @event);
    /// <summary>
    /// /
    /// </summary>
    /// <param name="context"></param>
    /// <param name="fromState"></param>
    /// <param name="event"></param>
    /// <returns></returns>
    Task<TNewState> HandleTransition(TContext context, TState fromState, TEvent @event);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="fromState"></param>
    /// <param name="event"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    Task<TNewState> CompensateTransition(TContext context, TState fromState, TEvent @event,Exception exception);
}