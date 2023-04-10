namespace Transiflow.Test.TestTransitionHandlers;

public class TestTransitionHandlerFail : ITransitionHandler<TestContext.TestContext, TestStateA, TestEventA, TestStateC>
{
    public ValueTask<bool> ValidateTransition(TestContext.TestContext context, TestStateA fromState, TestEventA @event)
    {
        return new ValueTask<bool>(false);
    }

    public Task<TestStateC> HandleTransition(TestContext.TestContext context, TestStateA fromState, TestEventA @event)
    {
        return Task.FromResult(new TestStateC());
    }

    public Task<TestStateC> CompensateTransition(TestContext.TestContext context, TestStateA fromState, TestEventA @event, Exception exception)
    {
        return Task.FromResult(new TestStateC());
    }
}