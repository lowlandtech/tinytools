using LowlandTech.TinyTools.Core.Entities;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US01.EntityModel;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "02")]
[UserStory("01", "The Tinytools server shares one template entity model")]
public sealed class SC02_ExposesTheFlowAbstractions
{
    private (bool FlowStep, bool DataContext, bool LegacyAlias) _surface;

    [Given("the Tinytools core assembly")]
    private void Given()
    {
    }

    [When("its exported abstraction types are read")]
    private void When()
    {
        var names = typeof(Template).Assembly.GetExportedTypes()
            .Where(type => type.Namespace == "LowlandTech.TinyTools.Core.Abstractions")
            .Select(type => type.Name)
            .ToList();

        _surface = (names.Contains("ITemplateFlowStep"), names.Contains("IFlowDataContext"), names.Contains("IFlowStep"));
    }

    public SC02_ExposesTheFlowAbstractions()
    {
        Given();
        When();
    }

    [Then("the flow abstractions should be exposed")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ExposesTheAbstractions() => _surface.Should().Be((true, true, false));
}
