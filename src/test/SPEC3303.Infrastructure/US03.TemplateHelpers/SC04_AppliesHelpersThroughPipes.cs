namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US03.TemplateHelpers;

[Trait("SPEC", "3303"), Trait("US", "03"), Trait("SC", "04")]
[UserStory("03", "Template helpers reachable from pipe syntax and helper lookup")]
public sealed class SC04_AppliesHelpersThroughPipes : IClassFixture<UseCases.TemplateHelpersUseCase>
{
    private readonly UseCases.TemplateHelpersUseCase useCase;
    private (string Name, string Template, global::LowlandTech.TinyTools.ExecutionContext Context)[] pipes = [];
    private (string Name, string Helper)[] helpers = [];

    public SC04_AppliesHelpersThroughPipes(UseCases.TemplateHelpersUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("five pipe templates bound to their public context and four installed helper names")]
    private void Given()
    {
        var value = new global::LowlandTech.TinyTools.ExecutionContext(); value.Set("Value", "test");
        var number = new global::LowlandTech.TinyTools.ExecutionContext(); number.Set("Number", "42");
        var other = new global::LowlandTech.TinyTools.ExecutionContext(); other.Set("Value", "hi");
        pipes = [("PadLeftPipe", "${Context.Value | padleft:10}", value),
            ("PadRightPipe", "${Context.Value | padright:10}", value),
            ("PadLeftCustomPipe", "${Context.Number | padleft:5,0}", number),
            ("PadRightCustomPipe", "${Context.Value | padright:8,*}", other),
            ("ChainedPipe", "${Context.Value | upper | padleft:10}", value)];
        helpers = [("PadLeftExists", "padleft"), ("PadRightExists", "padright"),
            ("PadLeftUpperExists", "PADLEFT"), ("PadRightMixedExists", "PadRight")];
    }

    [When("each pipe template is resolved once and each helper name is looked up once")]
    private void When()
    {
        var resolver = new VariableResolver();
        foreach (var pipe in pipes)
        {
            useCase.Cases[pipe.Name] = resolver.ResolveString(pipe.Template, pipe.Context);
        }
        foreach (var helper in helpers)
        {
            useCase.Cases[helper.Name] = global::LowlandTech.TinyTools.TemplateHelpers.Exists(helper.Helper);
        }
    }

    [Then("Result.PadLeftPipe should be spaced_to_width_ten")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_PadLeftPipe() => useCase.Cases["PadLeftPipe"].Should().Be("      test");

    [Then("Result.PadRightPipe should be padded_right_to_width_ten")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_PadRightPipe() => useCase.Cases["PadRightPipe"].Should().Be("test      ");

    [Then("Result.PadLeftCustomPipe should be 00042")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_PadLeftCustomPipe() => useCase.Cases["PadLeftCustomPipe"].Should().Be("00042");

    [Then("Result.PadRightCustomPipe should be hi******")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_PadRightCustomPipe() => useCase.Cases["PadRightCustomPipe"].Should().Be("hi******");

    [Then("Result.ChainedPipe should be spaced_upper_test")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ChainedPipe() => useCase.Cases["ChainedPipe"].Should().Be("      TEST");

    [Then("Result.PadLeftExists should be true")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_PadLeftExists() => useCase.Cases["PadLeftExists"].Should().Be(true);

    [Then("Result.PadRightExists should be true")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_PadRightExists() => useCase.Cases["PadRightExists"].Should().Be(true);

    [Then("Result.PadLeftUpperExists should be true")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_PadLeftUpperExists() => useCase.Cases["PadLeftUpperExists"].Should().Be(true);

    [Then("Result.PadRightMixedExists should be true")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_PadRightMixedExists() => useCase.Cases["PadRightMixedExists"].Should().Be(true);
}
