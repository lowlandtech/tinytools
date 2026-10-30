using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US02.VariableResolution;

[Trait("SPEC", "3303"), Trait("US", "02"), Trait("SC", "04")]
[UserStory("02", "Variable resolver resolves input dictionaries and typed values")]
public sealed class SC04_ResolvesInputsAndValues : IClassFixture<UseCases.VariableResolverUseCase>
{
    private readonly UseCases.VariableResolverUseCase useCase;
    private (string Name, Dictionary<string, object?> Inputs, ExecutionContext Context)[] inputSets = [];
    private (string Name, object? Value, ExecutionContext Context)[] values = [];

    public SC04_ResolvesInputsAndValues(UseCases.VariableResolverUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("one input dictionary or bare value with its own public context")]
    private void Given()
    {
        static ExecutionContext ContextWith(params (string Key, object? Value)[] values)
        {
            var context = new ExecutionContext();
            foreach (var value in values) context.Set(value.Key, value.Value);
            return context;
        }

        var named = ContextWith(("Name", "John"), ("Age", 30));
        var prefix = ContextWith(("Prefix", "Mr."));
        inputSets = [("InputsDictionary", new Dictionary<string, object?> { ["Greeting"] = "Hello, ${Context.Name}", ["Info"] = "Age: ${Context.Age}" }, named),
            ("InputsEmpty", new Dictionary<string, object?>(), named),
            ("InputsMixed", new Dictionary<string, object?> { ["StringValue"] = "Hello, ${Context.Name}", ["IntValue"] = 42, ["NullValue"] = null }, named)];
        values = [("ValueNull", null, ContextWith()),
            ("ValueInterpolatedString", "Hello, ${Context.Name}!", named),
            ("ValueSingleExpression", "${Context.Name}", named),
            ("ValuePlainString", "Plain text", ContextWith()),
            ("ValueList", new List<object> { "${Context.Prefix} John", "${Context.Prefix} Jane", 42 }, prefix)];
    }

    [When("each input dictionary and bare value is resolved once through the public resolver")]
    private void When()
    {
        var resolver = new VariableResolver();
        foreach (var set in inputSets)
            useCase.Cases[set.Name] = resolver.ResolveInputs(set.Inputs, set.Context);
        foreach (var value in values)
            useCase.Cases[value.Name] = resolver.ResolveValue(value.Value, value.Context);
    }

    [Then("Result.InputsDictionary should be Hello,_John_and_Age_30")]
    [Trait("UAC", "23"), Fact]
    public void UAC23_InputsDictionary() => ((Dictionary<string, object?>)useCase.Cases["InputsDictionary"]!).Should().BeEquivalentTo(new Dictionary<string, object?> { ["Greeting"] = "Hello, John", ["Info"] = "Age: 30" });

    [Then("Result.InputsEmpty should be empty")]
    [Trait("UAC", "24"), Fact]
    public void UAC24_InputsEmpty() => ((Dictionary<string, object?>)useCase.Cases["InputsEmpty"]!).Should().BeEmpty();

    [Then("Result.InputsMixed should be Hello,_John_42_null")]
    [Trait("UAC", "25"), Fact]
    public void UAC25_InputsMixed() => ((Dictionary<string, object?>)useCase.Cases["InputsMixed"]!).Should().BeEquivalentTo(new Dictionary<string, object?> { ["StringValue"] = "Hello, John", ["IntValue"] = 42, ["NullValue"] = null });

    [Then("Result.ValueNull should be null")]
    [Trait("UAC", "26"), Fact]
    public void UAC26_ValueNull() => useCase.Cases["ValueNull"].Should().BeNull();

    [Then("Result.ValueInterpolatedString should be Hello,_John!")]
    [Trait("UAC", "27"), Fact]
    public void UAC27_ValueInterpolatedString() => useCase.Cases["ValueInterpolatedString"].Should().Be("Hello, John!");

    [Then("Result.ValueSingleExpression should be John")]
    [Trait("UAC", "28"), Fact]
    public void UAC28_ValueSingleExpression() => useCase.Cases["ValueSingleExpression"].Should().Be("John");

    [Then("Result.ValuePlainString should be Plain_text")]
    [Trait("UAC", "29"), Fact]
    public void UAC29_ValuePlainString() => useCase.Cases["ValuePlainString"].Should().Be("Plain text");

    [Then("Result.ValueList should have count 3")]
    [Trait("UAC", "30"), Fact]
    public void UAC30_ValueList() => ((List<object>)useCase.Cases["ValueList"]!).Should().BeEquivalentTo(new List<object> { "Mr. John", "Mr. Jane", 42 }, options => options.WithStrictOrdering());
}
