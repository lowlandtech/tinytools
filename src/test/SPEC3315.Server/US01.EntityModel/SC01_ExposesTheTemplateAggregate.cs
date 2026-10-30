using System.Reflection;
using LowlandTech.TinyTools.Core.Entities;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US01.EntityModel;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "01")]
[UserStory("01", "The Tinytools server shares one template entity model")]
public sealed class SC01_ExposesTheTemplateAggregate
{
    private readonly string[] _expected =
    [
        "Template", "TemplateVersion", "TemplatePackage", "TemplateImport",
        "TemplateFlow", "TemplateStep", "StepIntent", "StepKind", "StepStatus",
    ];

    private HashSet<string> _names = [];

    [Given("the Tinytools core assembly")]
    private void Given()
    {
    }

    [When("its exported entity types are read")]
    private void When() => _names = typeof(Template).Assembly.GetExportedTypes()
        .Where(type => type.Namespace == "LowlandTech.TinyTools.Core.Entities")
        .Select(type => type.Name)
        .ToHashSet(StringComparer.Ordinal);

    public SC01_ExposesTheTemplateAggregate()
    {
        Given();
        When();
    }

    [Then("the exported entities should contain Template")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ExposesTheAggregate() => _names.Should().Contain(_expected);
}
