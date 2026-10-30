namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "16")]
[UserStory("01", "String interpolation uses TinyTemplateEngine")]
public sealed class SC16_InterpolatesEngineEdgeCases : TinyToolsScenario<string>
{
    private Person _person = null!;
    private string _anonymous = string.Empty;
    private string _dictionary = string.Empty;
    private string _multiple = string.Empty;
    private string _literal = string.Empty;
    private string _noVariables = string.Empty;
    private Action? _nullTemplate;
    private Action? _emptyTemplate;
    private Action? _nullModel;

    protected override string For() => "Hello world, I'm ${Context.Model.FirstName}";

    [Given("a person with a first name")]
    protected override void Given() => _person = new Person { FirstName = "John" };

    [When("each engine edge case is interpolated once")]
    protected override void When()
    {
        base.When();

        _anonymous = "Hello ${Context.Model.Name}, you have ${Context.Model.Count} items".InterpolateWithEngine(new { Name = "John", Count = 5 });
        _dictionary = "Hello ${Context.Model.Name}".InterpolateWithEngine(new Dictionary<string, object> { { "Name", "John" } });
        _multiple = "${Context.Model.FirstName} ${Context.Model.FirstName} ${Context.Model.LastName}".InterpolateWithEngine(new Person { FirstName = "John", LastName = "Doe" });
        _literal = "Hello world! ${Context.Model.FirstName} says hi.".InterpolateWithEngine(_person);
        _noVariables = "Hello world!".InterpolateWithEngine(_person);
        _nullTemplate = () => ((string)null!).InterpolateWithEngine(_person);
        _emptyTemplate = () => "".InterpolateWithEngine(_person);
        _nullModel = () => "Hello ${Context.Model.Name}".InterpolateWithEngine((Person)null!);
    }

    public SC16_InterpolatesEngineEdgeCases() => ArrangeAndAct();

    [Then("the anonymous model output should be exact")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_HandlesAnonymousTypes() => _anonymous.Should().Be("Hello John, you have 5 items");

    [Then("the null template should fail with ArgumentException")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_ThrowsOnNullTemplate() => _nullTemplate.Should().Throw<ArgumentException>();

    [Then("the empty template should fail with ArgumentException")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_ThrowsOnEmptyTemplate() => _emptyTemplate.Should().Throw<ArgumentException>();

    [Then("the null model should fail with ArgumentNullException")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_ThrowsOnNullModel() => _nullModel.Should().Throw<ArgumentNullException>();

    [Then("the dictionary model output should be exact")]
    [Trait("UAC", "13"), Fact]
    public void UAC13_HandlesDictionaryModel() => _dictionary.Should().Be("Hello John");

    [Then("the repeated interpolation should be exact")]
    [Trait("UAC", "14"), Fact]
    public void UAC14_HandlesMultipleInterpolations() => _multiple.Should().Be("John John Doe");

    [Then("the literal text should be exact")]
    [Trait("UAC", "15"), Fact]
    public void UAC15_PreservesNonInterpolatedText() => _literal.Should().Be("Hello world! John says hi.");

    [Then("the template without variables should be exact")]
    [Trait("UAC", "16"), Fact]
    public void UAC16_HandlesTemplateWithNoVariables() => _noVariables.Should().Be("Hello world!");
}
