using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US02.VariableResolution;

[Trait("SPEC", "3303"), Trait("US", "02"), Trait("SC", "02")]
[UserStory("02", "Variable resolver applies pipe helpers and context lookups")]
public sealed class SC02_ResolvesPipesAndLookups : IClassFixture<UseCases.VariableResolverUseCase>
{
    private readonly UseCases.VariableResolverUseCase useCase;
    private (string Name, string Input, ExecutionContext Context)[] inputs = [];

    public SC02_ResolvesPipesAndLookups(UseCases.VariableResolverUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("one pipe or lookup template with its own public context")]
    private void Given()
    {
        static ExecutionContext ContextWith(params (string Key, object? Value)[] values)
        {
            var context = new ExecutionContext();
            foreach (var value in values) context.Set(value.Key, value.Value);
            return context;
        }

        var dictionary = new Dictionary<string, object?> { ["Name"] = "John", ["Age"] = 30 };
        var objectDictionary = new Dictionary<object, object?> { ["City"] = "Seattle" };
        inputs = [("PipeSingle", "${Context.Name | upper}", ContextWith(("Name", "john"))),
            ("PipeMultiple", "${Context.Text | trim | upper}", ContextWith(("Text", "  hello world  "))),
            ("PipeArgument", "${Context.Text | truncate:5}", ContextWith(("Text", "Hello World"))),
            ("ContextGetDouble", "${Context.Get(\"UserName\")}", ContextWith(("UserName", "John"))),
            ("ContextGetSingle", "${Context.Get('UserName')}", ContextWith(("UserName", "Jane"))),
            ("DictionaryString", "${Context.Data.Name}", ContextWith(("Data", dictionary))),
            ("DictionaryObject", "${Context.Data.City}", ContextWith(("Data", objectDictionary)))];
    }

    [When("each pipe or lookup template is resolved once through the public resolver")]
    private void When()
    {
        var resolver = new VariableResolver();
        foreach (var input in inputs)
            useCase.Cases[input.Name] = resolver.ResolveString(input.Input, input.Context);
    }

    [Then("Result.PipeSingle should be JOHN")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_PipeSingle() => useCase.Cases["PipeSingle"].Should().Be("JOHN");

    [Then("Result.PipeMultiple should be HELLO_WORLD")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_PipeMultiple() => useCase.Cases["PipeMultiple"].Should().Be("HELLO WORLD");

    [Then("Result.PipeArgument should be He...")]
    [Trait("UAC", "13"), Fact]
    public void UAC13_PipeArgument() => useCase.Cases["PipeArgument"].Should().Be("He...");

    [Then("Result.ContextGetDouble should be John")]
    [Trait("UAC", "14"), Fact]
    public void UAC14_ContextGetDouble() => useCase.Cases["ContextGetDouble"].Should().Be("John");

    [Then("Result.ContextGetSingle should be Jane")]
    [Trait("UAC", "15"), Fact]
    public void UAC15_ContextGetSingle() => useCase.Cases["ContextGetSingle"].Should().Be("Jane");

    [Then("Result.DictionaryString should be John")]
    [Trait("UAC", "16"), Fact]
    public void UAC16_DictionaryString() => useCase.Cases["DictionaryString"].Should().Be("John");

    [Then("Result.DictionaryObject should be Seattle")]
    [Trait("UAC", "17"), Fact]
    public void UAC17_DictionaryObject() => useCase.Cases["DictionaryObject"].Should().Be("Seattle");
}
