namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "16")]
[UserStory("01", "String interpolation uses TinyTemplateEngine")]
public sealed class SC16_InterpolatesEngineEdgeCases : TinyToolsScenario<string>, IClassFixture<UseCases.StringInterpolationUseCase>
{
    private readonly UseCases.StringInterpolationUseCase useCase;
    private Person _person = null!;

    public SC16_InterpolatesEngineEdgeCases(UseCases.StringInterpolationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override string For() => "Hello world, I'm ${Context.Model.FirstName}";

    [Given("a person with a first name")]
    protected override void Given() => _person = new Person { FirstName = "John" };

    [When("each engine edge case is interpolated once")]
    protected override void When()
    {
        base.When();

        useCase.Cases["Anonymous"] = "Hello ${Context.Model.Name}, you have ${Context.Model.Count} items".InterpolateWithEngine(new { Name = "John", Count = 5 });
        useCase.Cases["Dictionary"] = "Hello ${Context.Model.Name}".InterpolateWithEngine(new Dictionary<string, object> { { "Name", "John" } });
        useCase.Cases["Multiple"] = "${Context.Model.FirstName} ${Context.Model.FirstName} ${Context.Model.LastName}".InterpolateWithEngine(new Person { FirstName = "John", LastName = "Doe" });
        useCase.Cases["Literal"] = "Hello world! ${Context.Model.FirstName} says hi.".InterpolateWithEngine(_person);
        useCase.Cases["NoVariables"] = "Hello world!".InterpolateWithEngine(_person);
        useCase.Cases["NullTemplate"] = (Action)(() => ((string)null!).InterpolateWithEngine(_person));
        useCase.Cases["EmptyTemplate"] = (Action)(() => "".InterpolateWithEngine(_person));
        useCase.Cases["NullModel"] = (Action)(() => "Hello ${Context.Model.Name}".InterpolateWithEngine((Person)null!));
    }

    [Then("the anonymous model output should be exact")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_HandlesAnonymousTypes() => useCase.Cases["Anonymous"].Should().Be("Hello John, you have 5 items");

    [Then("the null template should fail with ArgumentException")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_ThrowsOnNullTemplate() => ((Action)useCase.Cases["NullTemplate"]!).Should().Throw<ArgumentException>();

    [Then("the empty template should fail with ArgumentException")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_ThrowsOnEmptyTemplate() => ((Action)useCase.Cases["EmptyTemplate"]!).Should().Throw<ArgumentException>();

    [Then("the null model should fail with ArgumentNullException")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_ThrowsOnNullModel() => ((Action)useCase.Cases["NullModel"]!).Should().Throw<ArgumentNullException>();

    [Then("the dictionary model output should be exact")]
    [Trait("UAC", "13"), Fact]
    public void UAC13_HandlesDictionaryModel() => useCase.Cases["Dictionary"].Should().Be("Hello John");

    [Then("the repeated interpolation should be exact")]
    [Trait("UAC", "14"), Fact]
    public void UAC14_HandlesMultipleInterpolations() => useCase.Cases["Multiple"].Should().Be("John John Doe");

    [Then("the literal text should be exact")]
    [Trait("UAC", "15"), Fact]
    public void UAC15_PreservesNonInterpolatedText() => useCase.Cases["Literal"].Should().Be("Hello world! John says hi.");

    [Then("the template without variables should be exact")]
    [Trait("UAC", "16"), Fact]
    public void UAC16_HandlesTemplateWithNoVariables() => useCase.Cases["NoVariables"].Should().Be("Hello world!");
}
