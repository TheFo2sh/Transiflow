using Transiflow.Test.TestEvents;

public class TestEventA : TestEvent
{
    public TestEventA(string id) { Id = id; }
    public string Id { get; }
    public override TestEventTag Tag => TestEventTag.EventA;
}
public class TestEventC : TestEvent
{
    public TestEventC(string id) { Id = id; }
    public string Id { get; }
    public override TestEventTag Tag => TestEventTag.EventC;
}