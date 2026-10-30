using System.Reflection;
using LowlandTech.TinyTools.Core.Entities;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US01.EntityModel;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "01")]
[UserStory("01", "The Tinytools server shares one template entity model")]
public sealed class SC01_ExposesTheTemplateAggregate : IClassFixture<UseCases.EntityModelUseCase>
{
    private static readonly string[] Expected =
    [
        "Template", "TemplateVersion", "TemplatePackage", "TemplateImport",
        "TemplateFlow", "TemplateStep", "StepIntent", "StepKind", "StepStatus",
    ];

    private readonly UseCases.EntityModelUseCase useCase;

    public SC01_ExposesTheTemplateAggregate(UseCases.EntityModelUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("the Tinytools core assembly")]
    private void Given()
    {
    }

    [When("its exported entity types are read")]
    private void When() => useCase.Cases["Names"] = typeof(Template).Assembly.GetExportedTypes()
        .Where(type => type.Namespace == "LowlandTech.TinyTools.Core.Entities")
        .Select(type => type.Name)
        .ToHashSet(StringComparer.Ordinal);

    [Then("the exported entities should contain Template")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ExposesTheAggregate() => ((HashSet<string>)useCase.Cases["Names"]!).Should().Contain(Expected);
}
