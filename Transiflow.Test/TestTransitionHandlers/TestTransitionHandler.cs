namespace Transiflow.Test.TestTransitionHandlers;

public class TestTransitionHandler : ITransitionHandler<TestContext.TestContext, TestStateA, TestEventA, TestStateB>
{
    public ValueTask<bool> ValidateTransition(TestContext.TestContext context, TestStateA fromState, TestEventA @event)
    {
        return new ValueTask<bool>(true);
    }

    public Task<TestStateB> HandleTransition(TestContext.TestContext context, TestStateA fromState, TestEventA @event)
    {
        return Task.FromResult(new TestStateB());
    }

    public Task CompensateTransition(TestContext.TestContext context, TestStateA fromState, TestStateB toState, TestEventA @event, Exception exception)
    {
        return Task.CompletedTask;
    }
}