namespace Transiflow;

public interface IEvent<TEventTag>
{
    public TEventTag Tag { get; }
}