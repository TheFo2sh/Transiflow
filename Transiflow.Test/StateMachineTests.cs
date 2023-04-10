using Moq;
using Microsoft.Extensions.DependencyInjection;
using Transiflow;
using Transiflow.Test.TestContext;
using Transiflow.Test.TestEvents;
using Transiflow.Test.TestStates;
using Transiflow.Test.TestTransitionHandlers;

public class StateMachineTests
{
    private readonly Mock<IStateEntranceHandler<TestContext, TestStateB>> _entranceHandlerMock;
    private readonly Mock<IStateExistHandler<TestContext, TestStateA>> _existHandlerMock;
    private readonly ServiceProvider _serviceProvider;

    public StateMachineTests()
    {
        _entranceHandlerMock = new();
        _existHandlerMock = new();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(typeof(IStateEntranceHandler<TestContext, TestStateB>), _entranceHandlerMock.Object);
        serviceCollection.AddSingleton(typeof(IStateExistHandler<TestContext, TestStateA>), _existHandlerMock.Object);
        serviceCollection.AddScoped<ICodec<TestContext>, TestContextCodec>();
        
        _serviceProvider = serviceCollection.BuildServiceProvider();

    }
    
    [Fact]
    public async Task StateMachineTest_StateChanged()
    {

        var stateMachine =
            new StateMachine<TestState, TestStateTag, TestEvent, TestEventTag, TestContext>(_serviceProvider)
                .AddTransition(TestStateTag.StateA, TestEventTag.EventA, (sp, ctx) => new TestTransitionHandler());

        var testContext = new TestContext { CurrentState = new TestStateA() };
        var stateMachineService = stateMachine.CreateService(testContext);

        // Act
        await stateMachineService.SendEvent(new TestEventA("123"));

        // Assert
        Assert.Equal(TestStateTag.StateB, stateMachineService.GetContext().CurrentState.Tag);
    }
    
    [Fact]
    public async Task StateMachineTest_HandleEntrance()
    {

        var stateMachine =
            new StateMachine<TestState, TestStateTag, TestEvent, TestEventTag, TestContext>(_serviceProvider)
                .AddTransition(TestStateTag.StateA, TestEventTag.EventA, (sp, ctx) => new TestTransitionHandler());

        var testContext = new TestContext { CurrentState = new TestStateA() };
        var stateMachineService = stateMachine.CreateService(testContext);

        // Act
        await stateMachineService.SendEvent(new TestEventA("123"));

        // Assert
        _entranceHandlerMock.Verify(handler => handler.HandleEntrance(It.IsAny<TestContext>(), It.IsAny<TestStateB>()),
            Times.Once);
    }

    [Fact]
    public async Task StateMachineTest_HandleExist()
    {
        var stateMachine =
            new StateMachine<TestState, TestStateTag, TestEvent, TestEventTag, TestContext>(_serviceProvider)
                .AddTransition<TestStateA, TestEventA, TestStateB>(TestStateTag.StateA, TestEventTag.EventA,
                    (sp, ctx) => new TestTransitionHandler());

        var testContext = new TestContext { CurrentState = new TestStateA() };
        var stateMachineService = stateMachine.CreateService(testContext);

        // Act
        await stateMachineService.SendEvent(new TestEventA("123"));

        // Assert
        _existHandlerMock.Verify(handler => handler.HandleExist(It.IsAny<TestContext>(), It.IsAny<TestStateA>()),
            Times.Once);
    }

    [Fact]
    public async Task StateMachineTest_HandleTransitionCalled()
    {
        // Arrange
        var transitionHandlerMock = new Mock<ITransitionHandler<TestContext, TestStateA, TestEventA, TestStateB>>();
        transitionHandlerMock
            .Setup(handler =>
                handler.ValidateTransition(It.IsAny<TestContext>(), It.IsAny<TestStateA>(), It.IsAny<TestEventA>()))
            .ReturnsAsync(true);
        transitionHandlerMock
            .Setup(handler =>
                handler.HandleTransition(It.IsAny<TestContext>(), It.IsAny<TestStateA>(), It.IsAny<TestEventA>()))
            .ReturnsAsync(new TestStateB());

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<ICodec<TestContext>, TestContextCodec>();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var stateMachine =
            new StateMachine<TestState, TestStateTag, TestEvent, TestEventTag, TestContext>(serviceProvider)
                .AddTransition<TestStateA, TestEventA, TestStateB>(TestStateTag.StateA, TestEventTag.EventA,
                    (sp, ctx) => transitionHandlerMock.Object);

        var testContext = new TestContext { CurrentState = new TestStateA() };
        var stateMachineService = stateMachine.CreateService(testContext);

        // Act
        await stateMachineService.SendEvent(new TestEventA("123"));

        // Assert
        transitionHandlerMock.Verify(
            handler => handler.HandleTransition(It.IsAny<TestContext>(), It.IsAny<TestStateA>(),
                It.IsAny<TestEventA>()), Times.Once);
    }
    
    [Fact]
    public async Task StateMachineTest_ChooseCorrectTransition()
    {

        var stateMachine =
            new StateMachine<TestState, TestStateTag, TestEvent, TestEventTag, TestContext>(_serviceProvider)
                .AddTransition(TestStateTag.StateA, TestEventTag.EventC, (sp, ctx) => new TestTransitionHandlerFail())
                .AddTransition(TestStateTag.StateA, TestEventTag.EventA, (sp, ctx) => new TestTransitionHandler());

        var testContext = new TestContext { CurrentState = new TestStateA() };
        var stateMachineService = stateMachine.CreateService(testContext);

        // Act
        await stateMachineService.SendEvent(new TestEventA("123"));

        // Assert
        Assert.Equal(TestStateTag.StateB, stateMachineService.GetContext().CurrentState.Tag);
    }
    [Fact]
    public async Task StateMachineTest_CompensateTransitionWhenHandleEntranceThrowsException()
    {
        // Arrange
        var entranceHandlerMock = new Mock<IStateEntranceHandler<TestContext, TestStateB>>();
        entranceHandlerMock.Setup(handler => handler.HandleEntrance(It.IsAny<TestContext>(), It.IsAny<TestStateB>())).Throws<ArgumentException>();

        var serviceProvider = new ServiceCollection()
            .AddScoped<ICodec<TestContext>, TestContextCodec>()
            .AddSingleton(entranceHandlerMock.Object)
            .BuildServiceProvider();

        var transitionHandlerMock = new Mock<ITransitionHandler<TestContext, TestStateA, TestEventA, TestStateB>>();
        transitionHandlerMock.Setup(handler => handler.ValidateTransition(It.IsAny<TestContext>(), It.IsAny<TestStateA>(), It.IsAny<TestEventA>())).ReturnsAsync(true);
        transitionHandlerMock.Setup(handler => handler.HandleTransition(It.IsAny<TestContext>(), It.IsAny<TestStateA>(), It.IsAny<TestEventA>())).ReturnsAsync(new TestStateB());

        var stateMachine = new StateMachine<TestState, TestStateTag, TestEvent, TestEventTag, TestContext>(serviceProvider)
            .AddTransition<TestStateA, TestEventA, TestStateB>(TestStateTag.StateA, TestEventTag.EventA, (sp, ctx) => transitionHandlerMock.Object);

        var testContext = new TestContext { CurrentState = new TestStateA() };
        var stateMachineService = stateMachine.CreateService(testContext);

        // Act
        await Assert.ThrowsAsync<ArgumentException>(()=> stateMachineService.SendEvent(new TestEventA("123")));

        // Assert
        transitionHandlerMock.Verify(handler => handler.CompensateTransition(It.IsAny<TestContext>(), It.IsAny<TestStateA>(),It.IsAny<TestStateB>(), It.IsAny<TestEventA>(), It.IsAny<Exception>()), Times.Once);
    }

}