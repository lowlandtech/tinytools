using LowlandTech.TinyTools.Generators;

namespace LowlandTech.TinyTools.Tests.SPEC3323.TypedValidation.US01.Generator;

[Trait(Spec.SPEC, "3323")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Template member paths are checked against the declared model at build time")]
public sealed class SC01_ValidatesMemberPaths : IClassFixture<UseCases.TypedValidationUseCase>
{
    private readonly UseCases.TypedValidationUseCase useCase;

    public SC01_ValidatesMemberPaths(UseCases.TypedValidationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a consumer model with a name, a nested address and a nullable owner")]
    private void Given()
    {
    }

    [When("the generator compiles a valid template and three invalid ones")]
    private void When()
    {
        useCase.Cases["Valid"] = Generate("Hello ${Context.Model.Name}!${Context.Model.Address.City}");
        useCase.Cases["Unknown"] = Generate("Hello ${Context.Model.Nmae}!");
        useCase.Cases["Nullable"] = Generate("Owner ${Context.Model.Owner.Name}");
        useCase.Cases["Lenient"] = Generate("Owner ${Context.Model.Owner.Name}", lenient: true);
    }

    private static string Generate(string template, bool lenient = false)
    {
        var metadata = new Dictionary<string, string> { ["TinyToolsModel"] = "Consumer.Model" };
        if (lenient) metadata["TinyToolsLenient"] = "true";

        var consumer = LowlandTech.TinyTools.Tests.SPEC3306.Generator.US02.BuildTime.GeneratorHarness.Build(
            [("greeting.tmpl", template, metadata)],
            LowlandTech.TinyTools.Tests.SPEC3306.Generator.US02.BuildTime.GeneratorHarness.ConsumerSource);

        var codes = consumer.Diagnostics.Select(diagnostic => diagnostic.Id).ToList();
        var errors = consumer.Output.GetDiagnostics()
            .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .Select(d => d.Id)
            .ToList();

        return string.Join(",", codes) + "|" + string.Join(",", errors);
    }

    [Then("the valid compile should be clean")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_AcceptsAKnownPath() => ((string)useCase.Cases["Valid"]!).Should().Be("|");

    [Then("the unknown member should be TTG005")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RefusesAnUnknownMember() => ((string)useCase.Cases["Unknown"]!).Should().StartWith("TTG005|");

    [Then("the nullable segment should be TTG006")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RefusesANullableDereference() => ((string)useCase.Cases["Nullable"]!).Should().StartWith("TTG006|");

    [Then("the lenient compile should be clean")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_HonoursTheLenientFlag() => ((string)useCase.Cases["Lenient"]!).Should().Be("|");
}
