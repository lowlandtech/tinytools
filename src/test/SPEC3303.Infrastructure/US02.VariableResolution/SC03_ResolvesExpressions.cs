using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US02.VariableResolution;

[Trait("SPEC", "3303"), Trait("US", "02"), Trait("SC", "03")]
[UserStory("02", "Variable resolver resolves expressions and nested paths")]
public sealed class SC03_ResolvesExpressions : IClassFixture<UseCases.VariableResolverUseCase>
{
    private readonly UseCases.VariableResolverUseCase useCase;
    private (string Name, string Expression, ExecutionContext Context)[] inputs = [];

    public SC03_ResolvesExpressions(UseCases.VariableResolverUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("one expression and its own public context for every expression case")]
    private void Given()
    {
        static ExecutionContext ContextWith(params (string Key, object? Value)[] values)
        {
            var context = new ExecutionContext();
            foreach (var value in values) context.Set(value.Key, value.Value);
            return context;
        }

        var person = new { Name = "John", Details = new { Age = 30 } };
        var customer = new { Name = "ACME Corp", Contact = new { Email = "info@acme.com", Phone = "555-1234" } };
        inputs = [("ExpressionPrefixed", "Context.Name", ContextWith(("Name", "John"))),
            ("ExpressionBare", "Name", ContextWith(("Name", "Jane"))),
            ("ExpressionNested", "Context.Person.Details.Age", ContextWith(("Person", person))),
            ("ExpressionMissing", "Context.NonExistent", ContextWith()),
            ("ExpressionComplex", "Customer.Contact.Email", ContextWith(("Customer", customer)))];
    }

    [When("each expression is resolved once through the public resolver")]
    private void When()
    {
        var resolver = new VariableResolver();
        foreach (var input in inputs)
            useCase.Cases[input.Name] = resolver.ResolveExpression(input.Expression, input.Context);
    }

    [Then("Result.ExpressionPrefixed should be John")]
    [Trait("UAC", "18"), Fact]
    public void UAC18_ExpressionPrefixed() => useCase.Cases["ExpressionPrefixed"].Should().Be("John");

    [Then("Result.ExpressionBare should be Jane")]
    [Trait("UAC", "19"), Fact]
    public void UAC19_ExpressionBare() => useCase.Cases["ExpressionBare"].Should().Be("Jane");

    [Then("Result.ExpressionNested should be 30")]
    [Trait("UAC", "20"), Fact]
    public void UAC20_ExpressionNested() => useCase.Cases["ExpressionNested"].Should().Be(30);

    [Then("Result.ExpressionMissing should be null")]
    [Trait("UAC", "21"), Fact]
    public void UAC21_ExpressionMissing() => useCase.Cases["ExpressionMissing"].Should().BeNull();

    [Then("Result.ExpressionComplex should be info@acme.com")]
    [Trait("UAC", "22"), Fact]
    public void UAC22_ExpressionComplex() => useCase.Cases["ExpressionComplex"].Should().Be("info@acme.com");
}
