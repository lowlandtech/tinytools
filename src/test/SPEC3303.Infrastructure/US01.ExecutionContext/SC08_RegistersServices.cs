namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US01.ExecutionContext;

[Trait("SPEC", "3303"), Trait("US", "01"), Trait("SC", "08")]
[UserStory("01", "Tool context manages variables and state")]
public sealed class SC08_RegistersServices : IClassFixture<UseCases.ExecutionContextUseCase>
{
    private readonly UseCases.ExecutionContextUseCase useCase;
    private global::LowlandTech.TinyTools.ExecutionContext keyed = null!;
    private global::LowlandTech.TinyTools.ExecutionContext instance = null!;
    private global::LowlandTech.TinyTools.ExecutionContext multiple = null!;
    private global::LowlandTech.TinyTools.ExecutionContext missing = null!;
    private global::LowlandTech.TinyTools.ExecutionContext cased = null!;
    private global::LowlandTech.TinyTools.ExecutionContext overwritten = null!;

    public SC08_RegistersServices(UseCases.ExecutionContextUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("six contexts, one of them already carrying a first service for the same key")]
    private void Given()
    {
        keyed = new global::LowlandTech.TinyTools.ExecutionContext();
        instance = new global::LowlandTech.TinyTools.ExecutionContext();
        multiple = new global::LowlandTech.TinyTools.ExecutionContext();
        missing = new global::LowlandTech.TinyTools.ExecutionContext();
        cased = new global::LowlandTech.TinyTools.ExecutionContext();
        overwritten = new global::LowlandTech.TinyTools.ExecutionContext();
        overwritten.RegisterService("test", input => "first");
    }

    [When("each registration is applied once and every service is invoked once")]
    private void When()
    {
        keyed.RegisterService("test", input => $"Processed: {input}");
        useCase.Cases["Keyed"] = keyed.Services("test")("data");
        instance.RegisterService(new TestService());
        useCase.Cases["Instance"] = instance.Services("testservice")("input");
        multiple.RegisterServices(new List<ITemplateService> { new TestService(), new AnotherTestService() });
        useCase.Cases["MultipleFirst"] = multiple.Services("testservice")("a");
        useCase.Cases["MultipleSecond"] = multiple.Services("another")("b");
        useCase.Cases["Missing"] = missing.Services("nonexistent")("test");
        cased.RegisterService("MyService", input => "result");
        useCase.Cases["Lower"] = cased.Services("myservice")("test");
        useCase.Cases["Upper"] = cased.Services("MYSERVICE")("test");
        useCase.Cases["Mixed"] = cased.Services("MyService")("test");
        overwritten.RegisterService("test", input => "second");
        useCase.Cases["Overwritten"] = overwritten.Services("test")("data");
    }

    [Then("the keyed service should be processed-data")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RegistersByKey() => useCase.Cases["Keyed"].Should().Be("Processed: data");

    [Then("the registered instance should be TEST:input")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RegistersAnInstance() => useCase.Cases["Instance"].Should().Be("TEST:input");

    [Then("the first registered service should be TEST:a")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RegistersTheFirstOfMany() => useCase.Cases["MultipleFirst"].Should().Be("TEST:a");

    [Then("the second registered service should be ANOTHER:b")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RegistersTheSecondOfMany() => useCase.Cases["MultipleSecond"].Should().Be("ANOTHER:b");

    [Then("the missing service should be nonexistent-not-registered")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_AnswersForAMissingService() => useCase.Cases["Missing"].Should().Be("{nonexistent not registered}");

    [Then("the lower service key should be result")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_ReadsALowerCaseServiceKey() => useCase.Cases["Lower"].Should().Be("result");

    [Then("the upper service key should be result")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_ReadsAnUpperCaseServiceKey() => useCase.Cases["Upper"].Should().Be("result");

    [Then("the mixed service key should be result")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_ReadsTheDeclaredServiceKey() => useCase.Cases["Mixed"].Should().Be("result");

    [Then("the overwritten service should be second")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_OverwritesAServiceKey() => useCase.Cases["Overwritten"].Should().Be("second");
}
