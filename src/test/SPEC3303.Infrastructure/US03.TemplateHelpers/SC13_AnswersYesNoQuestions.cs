namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US03.TemplateHelpers;

[Trait("SPEC", "3303"), Trait("US", "03"), Trait("SC", "13")]
[UserStory("03", "Yesno helper answers with configurable labels")]
public sealed class SC13_AnswersYesNoQuestions : IClassFixture<UseCases.TemplateHelpersUseCase>
{
    private readonly UseCases.TemplateHelpersUseCase useCase;
    private (string Name, object? Value, string? Argument)[] inputs = [];

    public SC13_AnswersYesNoQuestions(UseCases.TemplateHelpersUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("one value and label argument for every yesno case")]
    private void Given()
    {
        inputs = [("YesNoNull", null, null), ("YesNoTrue", true, null),
            ("YesNoFalse", false, null), ("YesNoNonZeroInt", 42, null),
            ("YesNoZero", 0, null), ("YesNoNonEmptyString", "hello", null),
            ("YesNoEmptyString", "", null), ("YesNoFalseString", "false", null),
            ("YesNoFalseStringUpper", "FALSE", null), ("YesNoOtherObject", new { Name = "test" }, null),
            ("YesNoCustomLabels", true, "Active,Inactive"), ("YesNoCustomNoLabel", false, "On,Off"),
            ("YesNoOnlyYesLabel", false, "Active"), ("YesNoNegativeInt", -1, null)];
    }

    [When("each yesno input is applied once and the observed result is recorded")]
    private void When()
    {
        foreach (var input in inputs)
        {
            useCase.Cases[input.Name] = global::LowlandTech.TinyTools.TemplateHelpers.Apply(input.Value, "yesno", input.Argument);
        }
    }

    [Then("Result.YesNoNull should be No")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_YesNoNull() => useCase.Cases["YesNoNull"].Should().Be("No");

    [Then("Result.YesNoTrue should be Yes")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_YesNoTrue() => useCase.Cases["YesNoTrue"].Should().Be("Yes");

    [Then("Result.YesNoFalse should be No")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_YesNoFalse() => useCase.Cases["YesNoFalse"].Should().Be("No");

    [Then("Result.YesNoNonZeroInt should be Yes")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_YesNoNonZeroInt() => useCase.Cases["YesNoNonZeroInt"].Should().Be("Yes");

    [Then("Result.YesNoZero should be No")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_YesNoZero() => useCase.Cases["YesNoZero"].Should().Be("No");

    [Then("Result.YesNoNonEmptyString should be Yes")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_YesNoNonEmptyString() => useCase.Cases["YesNoNonEmptyString"].Should().Be("Yes");

    [Then("Result.YesNoEmptyString should be No")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_YesNoEmptyString() => useCase.Cases["YesNoEmptyString"].Should().Be("No");

    [Then("Result.YesNoFalseString should be No")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_YesNoFalseString() => useCase.Cases["YesNoFalseString"].Should().Be("No");

    [Then("Result.YesNoFalseStringUpper should be No")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_YesNoFalseStringUpper() => useCase.Cases["YesNoFalseStringUpper"].Should().Be("No");

    [Then("Result.YesNoOtherObject should be Yes")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_YesNoOtherObject() => useCase.Cases["YesNoOtherObject"].Should().Be("Yes");

    [Then("Result.YesNoCustomLabels should be Active")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_YesNoCustomLabels() => useCase.Cases["YesNoCustomLabels"].Should().Be("Active");

    [Then("Result.YesNoCustomNoLabel should be Off")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_YesNoCustomNoLabel() => useCase.Cases["YesNoCustomNoLabel"].Should().Be("Off");

    [Then("Result.YesNoOnlyYesLabel should be No")]
    [Trait("UAC", "13"), Fact]
    public void UAC13_YesNoOnlyYesLabel() => useCase.Cases["YesNoOnlyYesLabel"].Should().Be("No");

    [Then("Result.YesNoNegativeInt should be Yes")]
    [Trait("UAC", "14"), Fact]
    public void UAC14_YesNoNegativeInt() => useCase.Cases["YesNoNegativeInt"].Should().Be("Yes");
}
