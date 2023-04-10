namespace Transiflow.Test.TestStates;

public class TestState : IState<TestStateTag>
{
    public virtual TestStateTag Tag { get; }
}