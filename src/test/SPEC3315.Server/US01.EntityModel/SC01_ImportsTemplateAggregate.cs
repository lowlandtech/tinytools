using System.Reflection;
using LowlandTech.TinyTools.Core.Entities;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US01.EntityModel;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "01")]
[UserStory("01", "The Tinytools server shares one template entity model")]
public class WhenImportingTheTemplateEntityModel
{
    private static readonly Assembly CoreAssembly = typeof(Template).Assembly;

    [Trait(Spec.UAC, "01")]
    [Fact]
    public void ItShouldExposeTheTemplateAggregateAndPipelineEntities()
    {
        var expected = new[]
        {
            "Template", "TemplateVersion", "TemplatePackage", "TemplateImport",
            "TemplateFlow", "TemplateStep", "StepIntent", "StepKind", "StepStatus",
        };

        var names = CoreAssembly.GetExportedTypes()
            .Where(type => type.Namespace == "LowlandTech.TinyTools.Core.Entities")
            .Select(type => type.Name)
            .ToHashSet(StringComparer.Ordinal);

        names.Should().Contain(expected);
    }

    [Trait(Spec.UAC, "02")]
    [Fact]
    public void ItShouldExposeTheFlowAbstractionsWithoutTheLegacyAlias()
    {
        var abstractions = CoreAssembly.GetExportedTypes()
            .Where(type => type.Namespace == "LowlandTech.TinyTools.Core.Abstractions")
            .Select(type => type.Name)
            .ToList();

        abstractions.Should().Contain(["ITemplateFlowStep", "IFlowDataContext"]);
        abstractions.Should().NotContain("IFlowStep");
    }

    [Trait(Spec.UAC, "03")]
    [Fact]
    public void ItShouldPreserveEntityDefaultsAndNavigation()
    {
        var template = new Template();
        var version = new TemplateVersion();
        var package = new TemplatePackage();
        var flow = new TemplateFlow();
        var import = new TemplateImport();

        template.IsActive.Should().BeTrue();
        template.Versions.Should().BeEmpty();
        template.Packages.Should().BeEmpty();

        version.IsActive.Should().BeTrue();
        version.DataType.Should().Be("Workspace");

        flow.Steps.Should().BeEmpty();
        package.Imports.Should().BeEmpty();
        import.IsActive.Should().BeTrue();

        var owner = Guid.NewGuid();
        var child = new TemplateVersion { TemplateId = owner, Template = template };
        template.Versions.Add(child);

        template.Versions.Should().ContainSingle().Which.Template.Should().BeSameAs(template);
        template.Versions.Single().TemplateId.Should().Be(owner);
    }

    [Trait(Spec.UAC, "04")]
    [Fact]
    public void ItShouldNotReferenceAHostFramework()
    {
        var referenced = CoreAssembly.GetReferencedAssemblies()
            .Select(name => name.Name ?? string.Empty)
            .ToList();

        referenced.Should().NotContain(name => name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));
        referenced.Should().NotContain(name => name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
    }
}
