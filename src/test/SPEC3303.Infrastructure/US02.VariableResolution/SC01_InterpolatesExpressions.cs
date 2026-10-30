using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US02.VariableResolution;

[Trait("SPEC", "3303"), Trait("US", "02"), Trait("SC", "01")]
[UserStory("02", "Variable resolver interpolates template expressions")]
public sealed class SC01_InterpolatesExpressions : IClassFixture<UseCases.VariableResolverUseCase>
{
    private readonly UseCases.VariableResolverUseCase useCase;
    private (string Name, string? Input, ExecutionContext Context)[] inputs = [];

    public SC01_InterpolatesExpressions(UseCases.VariableResolverUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("one public context per interpolation and null-coalescing case")]
    private void Given()
    {
        static ExecutionContext ContextWith(params (string Key, object? Value)[] values)
        {
            var context = new ExecutionContext();
            foreach (var value in values) context.Set(value.Key, value.Value);
            return context;
        }

        inputs = [("Simple", "Hello, ${Context.Name}!", ContextWith(("Name", "John"))),
            ("NullInput", null, ContextWith()),
            ("EmptyInput", "", ContextWith()),
            ("MultipleVariables", "${Context.FirstName} ${Context.LastName}", ContextWith(("FirstName", "John"), ("LastName", "Doe"))),
            ("NestedProperties", "${Context.Person.Address.City}", ContextWith(("Person", new { Name = "John", Address = new { City = "Seattle" } }))),
            ("CoalesceNull", "${Context.Name ?? \"Default\"}", ContextWith(("Name", null))),
            ("CoalesceEmpty", "${Context.Name ?? \"Default\"}", ContextWith(("Name", ""))),
            ("CoalesceValue", "${Context.Name ?? \"Default\"}", ContextWith(("Name", "John"))),
            ("CoalesceSingleQuotes", "${Context.Name ?? 'Default'}", ContextWith(("Name", null))),
            ("CoalesceWithoutQuotes", "${Context.Name ?? Default}", ContextWith(("Name", null)))];
    }

    [When("each interpolation is resolved once through the public resolver")]
    private void When()
    {
        var resolver = new VariableResolver();
        foreach (var input in inputs)
            useCase.Cases[input.Name] = resolver.ResolveString(input.Input!, input.Context);
    }

    [Then("Result.Simple should be Hello,_John!")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_Simple() => useCase.Cases["Simple"].Should().Be("Hello, John!");

    [Then("Result.NullInput should be null")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_NullInput() => useCase.Cases["NullInput"].Should().BeNull();

    [Then("Result.EmptyInput should be empty")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_EmptyInput() => useCase.Cases["EmptyInput"].Should().Be("");

    [Then("Result.MultipleVariables should be John_Doe")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_MultipleVariables() => useCase.Cases["MultipleVariables"].Should().Be("John Doe");

    [Then("Result.NestedProperties should be Seattle")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_NestedProperties() => useCase.Cases["NestedProperties"].Should().Be("Seattle");

    [Then("Result.CoalesceNull should be Default")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_CoalesceNull() => useCase.Cases["CoalesceNull"].Should().Be("Default");

    [Then("Result.CoalesceEmpty should be Default")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_CoalesceEmpty() => useCase.Cases["CoalesceEmpty"].Should().Be("Default");

    [Then("Result.CoalesceValue should be John")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_CoalesceValue() => useCase.Cases["CoalesceValue"].Should().Be("John");

    [Then("Result.CoalesceSingleQuotes should be Default")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_CoalesceSingleQuotes() => useCase.Cases["CoalesceSingleQuotes"].Should().Be("Default");

    [Then("Result.CoalesceWithoutQuotes should be Default")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_CoalesceWithoutQuotes() => useCase.Cases["CoalesceWithoutQuotes"].Should().Be("Default");
}
