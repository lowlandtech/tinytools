using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US02.VariableResolution;

[Trait("SPEC", "3303"), Trait("US", "02"), Trait("SC", "05")]
[UserStory("02", "Variable resolver resolves dictionaries and edge-case paths")]
public sealed class SC05_ResolvesPathsAndEdges : IClassFixture<UseCases.VariableResolverUseCase>
{
    private readonly UseCases.VariableResolverUseCase useCase;
    private (string Name, object? Value, ExecutionContext Context)[] values = [];
    private (string Name, string Input, ExecutionContext Context)[] interpolations = [];

    public SC05_ResolvesPathsAndEdges(UseCases.VariableResolverUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("one dictionary or boundary template with its own public context")]
    private void Given()
    {
        static ExecutionContext ContextWith(params (string Key, object? Value)[] values)
        {
            var context = new ExecutionContext();
            foreach (var value in values) context.Set(value.Key, value.Value);
            return context;
        }

        var named = ContextWith(("Name", "John"));
        values = [("NestedDictionary", new Dictionary<string, object?> { ["Greeting"] = "${Context.Name}" }, named),
            ("ObjectDictionary", new Dictionary<object, object?> { ["Key"] = "${Context.Value}" }, ContextWith(("Value", "Test"))),
            ("OtherTypes", 42, ContextWith())];
        interpolations = [("NullPropertyAccess", "${Context.Object.Property}", ContextWith(("Object", null))),
            ("MissingProperty", "${Context.Object.NonExistent}", ContextWith(("Object", new { Name = "John" }))),
            ("EmptyExpression", "${}", ContextWith()),
            ("CaseInsensitiveProperty", "${Context.Object.name}", ContextWith(("Object", new { Name = "John" }))),
            ("FieldAccess", "${Context.Object.PublicField}", ContextWith(("Object", new TestClassWithField { PublicField = "FieldValue" }))),
            ("ComplexNestedPath", "${Context.Root.Level1.Level2.Level3.Value}", ContextWith(("Root", new { Level1 = new { Level2 = new { Level3 = new { Value = "DeepValue" } } } }))),
            ("NumericValues", "Count: ${Context.Count}, Price: ${Context.Price}", ContextWith(("Count", 42), ("Price", 19.99))),
            ("BooleanValues", "Active: ${Context.IsActive}, Deleted: ${Context.IsDeleted}", ContextWith(("IsActive", true), ("IsDeleted", false)))];
    }

    [When("each dictionary is resolved once and each boundary template is interpolated once")]
    private void When()
    {
        var resolver = new VariableResolver();
        foreach (var value in values)
            useCase.Cases[value.Name] = resolver.ResolveValue(value.Value, value.Context);
        foreach (var interpolation in interpolations)
            useCase.Cases[interpolation.Name] = resolver.ResolveString(interpolation.Input, interpolation.Context);
    }

    [Then("Result.NestedDictionary should be John")]
    [Trait("UAC", "31"), Fact]
    public void UAC31_NestedDictionary() => ((Dictionary<string, object?>)useCase.Cases["NestedDictionary"]!).Should().BeEquivalentTo(new Dictionary<string, object?> { ["Greeting"] = "John" });

    [Then("Result.ObjectDictionary should be Test")]
    [Trait("UAC", "32"), Fact]
    public void UAC32_ObjectDictionary() => ((Dictionary<string, object?>)useCase.Cases["ObjectDictionary"]!).Should().BeEquivalentTo(new Dictionary<string, object?> { ["Key"] = "Test" });

    [Then("Result.OtherTypes should be 42")]
    [Trait("UAC", "33"), Fact]
    public void UAC33_OtherTypes() => useCase.Cases["OtherTypes"].Should().Be(42);

    [Then("Result.NullPropertyAccess should be empty")]
    [Trait("UAC", "34"), Fact]
    public void UAC34_NullPropertyAccess() => useCase.Cases["NullPropertyAccess"].Should().Be("");

    [Then("Result.MissingProperty should be empty")]
    [Trait("UAC", "35"), Fact]
    public void UAC35_MissingProperty() => useCase.Cases["MissingProperty"].Should().Be("");

    [Then("Result.EmptyExpression should be ${}")]
    [Trait("UAC", "36"), Fact]
    public void UAC36_EmptyExpression() => useCase.Cases["EmptyExpression"].Should().Be("${}");

    [Then("Result.CaseInsensitiveProperty should be John")]
    [Trait("UAC", "37"), Fact]
    public void UAC37_CaseInsensitiveProperty() => useCase.Cases["CaseInsensitiveProperty"].Should().Be("John");

    [Then("Result.FieldAccess should be FieldValue")]
    [Trait("UAC", "38"), Fact]
    public void UAC38_FieldAccess() => useCase.Cases["FieldAccess"].Should().Be("FieldValue");

    [Then("Result.ComplexNestedPath should be DeepValue")]
    [Trait("UAC", "39"), Fact]
    public void UAC39_ComplexNestedPath() => useCase.Cases["ComplexNestedPath"].Should().Be("DeepValue");

    [Then("Result.NumericValues should contain Count: 42")]
    [Trait("UAC", "40"), Fact]
    public void UAC40_NumericValues() => ((string)useCase.Cases["NumericValues"]!).Should().Contain("Count: 42").And.Contain("Price:").And.Contain("19");

    [Then("Result.BooleanValues should be Active_True_Deleted_False")]
    [Trait("UAC", "41"), Fact]
    public void UAC41_BooleanValues() => useCase.Cases["BooleanValues"].Should().Be("Active: True, Deleted: False");
}
