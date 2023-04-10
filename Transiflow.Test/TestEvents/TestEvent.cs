namespace Transiflow.Test.TestEvents;

public class TestEvent : IEvent<TestEventTag>
{
    public virtual TestEventTag Tag { get; }
}