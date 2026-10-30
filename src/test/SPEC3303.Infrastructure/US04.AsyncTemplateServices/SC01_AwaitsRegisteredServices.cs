namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US04.AsyncTemplateServices;

[Trait(Spec.SPEC, "3303")]
[Trait(Spec.SC, "01")]
[UserStory("04", "Templates await explicitly registered asynchronous services")]
public sealed class SC01_AwaitsRegisteredServices : IClassFixture<UseCases.AsyncTemplateServicesUseCase>
{
    private readonly UseCases.AsyncTemplateServicesUseCase useCase;
    private dynamic engine = null!;
    private dynamic context = null!;
    private CancellationTokenSource cancellation = null!;

    public SC01_AwaitsRegisteredServices(UseCases.AsyncTemplateServicesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("registered asynchronous and synchronous services and a cancellation token")]
    private void Given()
    {
        engine = new TinyTemplateEngine();
        context = new ExecutionContext();
        context.Set("Value", "Ada");
        useCase.Cases["Order"] = new List<string>();
        Func<object?, CancellationToken, ValueTask<object?>> first = (input, _) => { ((List<string>)useCase.Cases["Order"]!).Add("first"); return ValueTask.FromResult<object?>(input); };
        Func<object?, CancellationToken, ValueTask<object?>> second = (input, _) => { ((List<string>)useCase.Cases["Order"]!).Add("second"); return ValueTask.FromResult<object?>(input); };
        Func<object?, CancellationToken, ValueTask<object?>> failure = (_, _) => ValueTask.FromException<object?>(new InvalidOperationException("private service detail"));
        context.RegisterAsyncService("first", first);
        context.RegisterAsyncService("second", second);
        context.RegisterAsyncService("failure", failure);
        LowlandTech.TinyTools.TemplateServiceFunc synchronous = input => "sync:" + input;
        context.RegisterService("sync", synchronous);
        cancellation = new CancellationTokenSource();
        cancellation.Cancel();
    }

    [When("the public asynchronous and synchronous render paths are called")]
    private void When()
    {
        try { useCase.Cases["AsyncServiceOutput"] = (string)((Task<string>)engine.RenderAsync(@"@service(""first"", Context.Value)", context, CancellationToken.None)).GetAwaiter().GetResult(); }
        catch (Exception error) { useCase.Cases["AsyncServiceOutput"] = error.GetType().Name; }
        ((List<string>)useCase.Cases["Order"]!).Clear();
        try { useCase.Cases["AsyncServiceOrderOutput"] = (string)((Task<string>)engine.RenderAsync(@"@service(""first"", Context.Value)
@service(""second"", Context.Value)", context, CancellationToken.None)).GetAwaiter().GetResult(); }
        catch (Exception error) { useCase.Cases["AsyncServiceOrderOutput"] = error.GetType().Name; }
        useCase.Cases["AsyncServiceOrder"] = ((List<string>)useCase.Cases["Order"]!).ToArray();
        try { useCase.Cases["ServiceFailureResult"] = (string)((Task<string>)engine.RenderAsync(@"prefix
@service(""failure"", Context.Value)", context, CancellationToken.None)).GetAwaiter().GetResult(); }
        catch (Exception error) { useCase.Cases["ServiceFailureResult"] = error.GetType().Name + ":" + error.Message; }
        try { useCase.Cases["CancellationResult"] = (string)((Task<string>)engine.RenderAsync(@"@service(""first"", Context.Value)", context, cancellation.Token)).GetAwaiter().GetResult(); }
        catch (Exception error) { useCase.Cases["CancellationResult"] = error.GetType().Name; }
        useCase.Cases["SynchronousOutput"] = (string)engine.Render(@"${Context.Services(""sync"")(""Ada"")}", context);
        try { useCase.Cases["SyncAsyncServiceResult"] = (string)engine.Render(@"@service(""first"", Context.Value)", context); }
        catch (Exception error) { useCase.Cases["SyncAsyncServiceResult"] = error.GetType().Name; }
    }

    [Then("Result.AsyncServiceOutput should be Ada")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_AwaitsService() => useCase.Cases["AsyncServiceOutput"].Should().Be("Ada");

    [Then("Result.AsyncServiceOrder should have count 2")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_PreservesSourceOrder() => ((string[])useCase.Cases["AsyncServiceOrder"]!).Should().Equal("first", "second");

    [Then("Service failure should fail with TemplateServiceException")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HidesServiceDetail() => ((string)useCase.Cases["ServiceFailureResult"]!).Should().Contain("TemplateServiceException").And.NotContain("private service detail");

    [Then("Cancellation should fail with OperationCanceledException")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_PropagatesCancellation() => useCase.Cases["CancellationResult"].Should().Be("OperationCanceledException");

    [Then("Result.SynchronousOutput should be sync:Ada")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_PreservesSynchronousServices() => useCase.Cases["SynchronousOutput"].Should().Be("sync:Ada");

    [Then("Sync async service should fail with InvalidOperationException")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_DoesNotBlockSynchronously() => useCase.Cases["SyncAsyncServiceResult"].Should().Be("InvalidOperationException");
}
