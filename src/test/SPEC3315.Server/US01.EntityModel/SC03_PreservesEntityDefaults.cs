using LowlandTech.TinyTools.Core.Entities;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US01.EntityModel;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "03")]
[UserStory("01", "The Tinytools server shares one template entity model")]
public sealed class SC03_PreservesEntityDefaults : IClassFixture<UseCases.EntityModelUseCase>
{
    private readonly UseCases.EntityModelUseCase useCase;
    private readonly Template _template = new();
    private readonly TemplateVersion _version = new();
    private readonly TemplatePackage _package = new();
    private readonly TemplateFlow _flow = new();
    private readonly TemplateImport _import = new();
    private Guid _owner;

    public SC03_PreservesEntityDefaults(UseCases.EntityModelUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a fresh template aggregate")]
    private void Given() => _owner = Guid.NewGuid();

    [When("its defaults and version navigation are read")]
    private void When()
    {
        _template.Versions.Add(new TemplateVersion { TemplateId = _owner, Template = _template });
        useCase.Cases["Owner"] = _owner;
        useCase.Cases["Defaults"] = new { TemplateActive = _template.IsActive, VersionCount = _template.Versions.Count, PackageCount = _template.Packages.Count, VersionActive = _version.IsActive, VersionDataType = _version.DataType, StepCount = _flow.Steps.Count, ImportCount = _package.Imports.Count, ImportActive = _import.IsActive, ChildTemplateId = _template.Versions.Single().TemplateId };
    }

    [Then("the entity defaults should be preserved")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_PreservesDefaults() => useCase.Cases["Defaults"].Should().BeEquivalentTo(new { TemplateActive = true, VersionCount = 1, PackageCount = 0, VersionActive = true, VersionDataType = "Workspace", StepCount = 0, ImportCount = 0, ImportActive = true, ChildTemplateId = (Guid)useCase.Cases["Owner"]! });
}
