namespace LowlandTech.TinyTools.Tests.SPEC3321.Strict.US01.UnknownVariables;

[Trait(Spec.SPEC, "3321")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Strict rendering refuses unknown variables instead of rendering empty text")]
public sealed class SC01_RefusesUnknownVariables : IClassFixture<UseCases.StrictVariablesUseCase>
{
    private readonly UseCases.StrictVariablesUseCase useCase;
    private readonly TinyTemplateEngine engine = new();
    private ExecutionContext context = null!;

    public SC01_RefusesUnknownVariables(UseCases.StrictVariablesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a caller context with a known name, a null alias and no other keys")]
    private void Given()
    {
        context = new ExecutionContext();
        context.Set("Name", "Ada");
        context.Set("Alias", null);
    }

    [When("the same templates render under lenient and strict policies")]
    private void When()
    {
        useCase.Cases["LenientMissing"] = engine.Render("Hello ${Context.Missing}!", context);

        useCase.Cases["StrictMissing"] = Capture(() => engine.Render("Hello ${Context.Missing}!", context, TemplateRenderOptions.Strict));
        useCase.Cases["SecondLine"] = Capture(() => engine.Render("first\nsecond ${Context.Missing}", context, TemplateRenderOptions.Strict));
        useCase.Cases["StrictHelper"] = Capture(() => engine.Render("${Context.Name | nosuchhelper}", context, TemplateRenderOptions.Strict));
        useCase.Cases["LenientHelper"] = engine.Render("${Context.Name | nosuchhelper}", context, TemplateRenderOptions.Strict with { StrictVariables = false });
        useCase.Cases["StrictNull"] = Capture(() => engine.Render("${Context.Alias}", context, TemplateRenderOptions.Strict));
        useCase.Cases["NullAllowed"] = engine.Render("${Context.Alias}", context, TemplateRenderOptions.Strict with { FailOnNull = false });
        useCase.Cases["KnownPaths"] = engine.Render("${Context.Name} and ${Context.Name | upper}", context, TemplateRenderOptions.Strict);
    }

    private static string Capture(Action render)
    {
        try
        {
            render();
            return "no-throw";
        }
        catch (TemplateResolutionException error)
        {
            return $"{error.Code}|{error.Line}|{error.Column}|{error.Message}";
        }
    }

    [Then("the strict result should be TR001")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RefusesAnUnknownKey() => ((string)useCase.Cases["StrictMissing"]!).Should().StartWith("TR001|1|7|Unknown context key 'Missing'");

    [Then("the lenient output should be exact")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_KeepsTheLenientDefault() => ((string)useCase.Cases["LenientMissing"]!).Should().Be("Hello !");

    [Then("the helper result should be TR002")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RefusesAnUnknownHelper() => ((string)useCase.Cases["StrictHelper"]!).Should().StartWith("TR002|1|1|Unknown pipe helper 'nosuchhelper'");

    [Then("the lenient helper output should be Ada")]
    [Trait("UAC", "07"), Fact]
    public void UAC03b_PassesUnknownHelpersWhenLenient() => ((string)useCase.Cases["LenientHelper"]!).Should().Be("Ada");

    [Then("the strict null should be TR003")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RefusesNullWhenAsked() => ((string)useCase.Cases["StrictNull"]!).Should().StartWith("TR003|1|1|'Alias' resolved to null");

    [Then("the null output should be none")]
    [Trait("UAC", "08"), Fact]
    public void UAC04b_AllowsNullWhenNotAsked() => ((string)useCase.Cases["NullAllowed"]!).Should().BeEmpty();

    [Then("the second line position should be 2-8")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ReportsTheTokenPosition() => ((string)useCase.Cases["SecondLine"]!).Should().StartWith("TR001|2|8|");

    [Then("the known paths should be exact")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_AcceptsKnownPaths() => ((string)useCase.Cases["KnownPaths"]!).Should().Be("Ada and ADA");
}
