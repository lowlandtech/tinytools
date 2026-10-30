using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US02.VariableResolution;

[Trait("SPEC", "3303"), Trait("US", "02"), Trait("SC", "06")]
[UserStory("02", "Variable resolver resolves services, delegates and reflection members")]
public sealed class SC06_ResolvesServicesAndDelegates : IClassFixture<UseCases.VariableResolverUseCase>
{
    private readonly UseCases.VariableResolverUseCase useCase;
    private (string Name, string Input, ExecutionContext Context)[] inputs = [];

    public SC06_ResolvesServicesAndDelegates(UseCases.VariableResolverUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("one service, delegate or reflection template with its own public context")]
    private void Given()
    {
        static ExecutionContext ContextWith(params (string Key, object? Value)[] values)
        {
            var context = new ExecutionContext();
            foreach (var value in values) context.Set(value.Key, value.Value);
            return context;
        }

        var complex = ContextWith(("Customer", new { FirstName = "john", LastName = "doe", Email = "john.doe@example.com", IsActive = true }), ("CompanyName", null));
        var failing = new ExecutionContext(); failing.RegisterService("test", input => int.Parse(input?.ToString() ?? "0") * 2);
        var nested = new ExecutionContext(); nested.RegisterService("getNested", _ => new TestServiceReturningNestedObject().GetNestedObject());
        inputs = [
            ("ComplexScenario", "Welcome ${Context.Customer.FirstName | upper} ${Context.Customer.LastName | upper}! Email: ${Context.Customer.Email} Company: ${Context.CompanyName ?? \"Not Specified\"}", complex),
            ("CoalescingWithPipes", "${Context.Name | upper ?? \"Default\"}", ContextWith(("Name", null))),
            ("DelegateFailure", "${Context.Services('test')('not-a-number')}", failing),
            ("NestedAfterMethodCall", "${Context.Services('getNested')('test').Level1.Level2.Value}", nested),
            ("DelegateProperty", "${Context.Object.MyDelegate('test')}", ContextWith(("Object", new TestClassWithDelegateProperty()))),
            ("ReflectionProperty", "${Context.Object.StringProperty}-${Context.Object.IntProperty}", ContextWith(("Object", new TestClassWithProperties { StringProperty = "PropertyValue", IntProperty = 42 })))];
    }

    [When("each service, delegate or reflection template is interpolated once")]
    private void When()
    {
        var resolver = new VariableResolver();
        foreach (var input in inputs)
            useCase.Cases[input.Name] = resolver.ResolveString(input.Input, input.Context);
    }

    [Then("Result.ComplexScenario should contain Welcome JOHN DOE!")]
    [Trait("UAC", "42"), Fact]
    public void UAC42_ComplexScenario() => ((string)useCase.Cases["ComplexScenario"]!).Should().Contain("Welcome JOHN DOE!").And.Contain("Email: john.doe@example.com").And.Contain("Company: Not Specified");

    [Then("Result.CoalescingWithPipes should be empty")]
    [Trait("UAC", "43"), Fact]
    public void UAC43_CoalescingWithPipes() => useCase.Cases["CoalescingWithPipes"].Should().Be("");

    [Then("Result.DelegateFailure should be empty")]
    [Trait("UAC", "44"), Fact]
    public void UAC44_DelegateFailure() => useCase.Cases["DelegateFailure"].Should().Be("");

    [Then("Result.NestedAfterMethodCall should be DeepValue")]
    [Trait("UAC", "45"), Fact]
    public void UAC45_NestedAfterMethodCall() => useCase.Cases["NestedAfterMethodCall"].Should().Be("DeepValue");

    [Then("Result.DelegateProperty should be Delegate_called_with:_test")]
    [Trait("UAC", "46"), Fact]
    public void UAC46_DelegateProperty() => useCase.Cases["DelegateProperty"].Should().Be("Delegate called with: test");

    [Then("Result.ReflectionProperty should be PropertyValue-42")]
    [Trait("UAC", "47"), Fact]
    public void UAC47_ReflectionProperty() => useCase.Cases["ReflectionProperty"].Should().Be("PropertyValue-42");
}
