using System.Text.Json;
using Transiflow.Test.TestStates;

namespace Transiflow.Test.TestContext;

public class TestContext : IContext<TestState, TestStateTag>
{
    public TestState CurrentState { get; set; }
}
public class TestContextCodec : ICodec<TestContext>
{
    public TestContext Copy(TestContext context)
    {
        return new TestContext
        {
            CurrentState = context.CurrentState
        };
    }

    public TestContext Decode(byte[] encodedContext)
    {
        return JsonSerializer.Deserialize<TestContext>(encodedContext)!;
    }

    public byte[] Encode(TestContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(context);
    }
}