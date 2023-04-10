using Transiflow.Test.TestStates;

public class TestStateA : TestState
{
    public override TestStateTag Tag => TestStateTag.StateA;
}
public class TestStateC : TestState
{
    public override TestStateTag Tag => TestStateTag.StateC;
}