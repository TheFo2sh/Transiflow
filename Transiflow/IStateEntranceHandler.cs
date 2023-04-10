namespace Transiflow;

public interface IStateEntranceHandler<TContext, TState>
{
    Task HandleEntrance(TContext context, TState state);
}