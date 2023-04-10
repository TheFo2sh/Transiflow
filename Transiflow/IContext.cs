namespace Transiflow;

public interface IContext<TState,TStateTag> where TState : IState<TStateTag> 

{
    public TState CurrentState { get; set; }

}