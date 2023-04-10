namespace Transiflow;

public interface IStateExistHandler<TContext, TState>
{
    Task HandleExist(TContext context, TState state);
}