namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US03.TemplateHelpers;

[Trait("SPEC", "3303"), Trait("US", "03"), Trait("SC", "01")]
[UserStory("03", "Template helpers format and transform values")]
public sealed class SC01_FormatsAndTransformsValues : IClassFixture<UseCases.TemplateHelpersUseCase>
{
    private readonly UseCases.TemplateHelpersUseCase useCase;
    private (string Name, object? Value, string Helper, string? Argument)[] inputs = [];

    public SC01_FormatsAndTransformsValues(UseCases.TemplateHelpersUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("one value, helper name and argument for every padleft and padright case")]
    private void Given()
    {
        inputs = [("PadLeftDefault", "test", "padleft", "10"), ("PadLeftCustom", "42", "padleft", "5,0"),
            ("PadLeftLonger", "hello world", "padleft", "5"), ("PadLeftNonNumeric", "test", "padleft", "abc"),
            ("PadLeftNullArgument", "test", "padleft", null), ("PadLeftNullValue", null, "padleft", "10"),
            ("PadLeftEmptyValue", "", "padleft", "5"), ("PadLeftExactWidth", "hello", "padleft", "5"),
            ("PadRightDefault", "test", "padright", "10"), ("PadRightCustom", "42", "padright", "5,-"),
            ("PadRightLonger", "hello world", "padright", "5"), ("PadRightNonNumeric", "test", "padright", "abc"),
            ("PadRightNullArgument", "test", "padright", null), ("PadRightNullValue", null, "padright", "10"),
            ("PadRightEmptyValue", "", "padright", "5"), ("PadRightExactWidth", "hello", "padright", "5")];
    }

    [When("each padding helper is applied once and the observed result is recorded")]
    private void When()
    {
        foreach (var input in inputs)
            useCase.Cases[input.Name] = global::LowlandTech.TinyTools.TemplateHelpers.Apply(input.Value, input.Helper, input.Argument);
    }

    [Then("Result.PadLeftDefault should be spaced_to_width_ten")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_PadLeftDefault() => useCase.Cases["PadLeftDefault"].Should().Be("      test");

    [Then("Result.PadLeftCustom should be 00042")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_PadLeftCustom() => useCase.Cases["PadLeftCustom"].Should().Be("00042");

    [Then("Result.PadLeftLonger should be hello_world")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_PadLeftLonger() => useCase.Cases["PadLeftLonger"].Should().Be("hello world");

    [Then("Result.PadLeftNonNumeric should be test")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_PadLeftNonNumeric() => useCase.Cases["PadLeftNonNumeric"].Should().Be("test");

    [Then("Result.PadLeftNullArgument should be test")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_PadLeftNullArgument() => useCase.Cases["PadLeftNullArgument"].Should().Be("test");

    [Then("Result.PadLeftNullValue should be null")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_PadLeftNullValue() => useCase.Cases["PadLeftNullValue"].Should().BeNull();

    [Then("Result.PadLeftEmptyValue should be empty")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_PadLeftEmptyValue() => useCase.Cases["PadLeftEmptyValue"].Should().Be("");

    [Then("Result.PadLeftExactWidth should be hello")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_PadLeftExactWidth() => useCase.Cases["PadLeftExactWidth"].Should().Be("hello");

    [Then("Result.PadRightDefault should be spaced_to_width_ten")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_PadRightDefault() => useCase.Cases["PadRightDefault"].Should().Be("test      ");

    [Then("Result.PadRightCustom should be 42---")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_PadRightCustom() => useCase.Cases["PadRightCustom"].Should().Be("42---");

    [Then("Result.PadRightLonger should be hello_world")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_PadRightLonger() => useCase.Cases["PadRightLonger"].Should().Be("hello world");

    [Then("Result.PadRightNonNumeric should be test")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_PadRightNonNumeric() => useCase.Cases["PadRightNonNumeric"].Should().Be("test");

    [Then("Result.PadRightNullArgument should be test")]
    [Trait("UAC", "13"), Fact]
    public void UAC13_PadRightNullArgument() => useCase.Cases["PadRightNullArgument"].Should().Be("test");

    [Then("Result.PadRightNullValue should be null")]
    [Trait("UAC", "14"), Fact]
    public void UAC14_PadRightNullValue() => useCase.Cases["PadRightNullValue"].Should().BeNull();

    [Then("Result.PadRightEmptyValue should be empty")]
    [Trait("UAC", "15"), Fact]
    public void UAC15_PadRightEmptyValue() => useCase.Cases["PadRightEmptyValue"].Should().Be("");

    [Then("Result.PadRightExactWidth should be hello")]
    [Trait("UAC", "16"), Fact]
    public void UAC16_PadRightExactWidth() => useCase.Cases["PadRightExactWidth"].Should().Be("hello");
}
