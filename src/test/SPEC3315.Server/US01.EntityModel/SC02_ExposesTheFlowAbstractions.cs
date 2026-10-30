using LowlandTech.TinyTools.Core.Entities;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US01.EntityModel;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "02")]
[UserStory("01", "The Tinytools server shares one template entity model")]
public sealed class SC02_ExposesTheFlowAbstractions : IClassFixture<UseCases.EntityModelUseCase>
{
    private readonly UseCases.EntityModelUseCase useCase;

    public SC02_ExposesTheFlowAbstractions(UseCases.EntityModelUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

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

        useCase.Cases["Surface"] = (names.Contains("ITemplateFlowStep"), names.Contains("IFlowDataContext"), names.Contains("IFlowStep"));
    }

    [Then("the flow abstractions should be exposed")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ExposesTheAbstractions() => useCase.Cases["Surface"].Should().Be((true, true, false));
}
