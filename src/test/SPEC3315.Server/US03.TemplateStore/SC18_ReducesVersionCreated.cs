#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Core.Entities;
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "18")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC18_ReducesVersionCreated
{
    private TemplateState _state = new();

    [Given("an editor showing one version")]
    private void Given() => _state = new TemplateState { Versions = [new() { Name = "v1" }] };

    [When("a new version is created")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplateVersionCreated(new TemplateVersion { Name = "v2" }));

    public SC18_ReducesVersionCreated()
    {
        Given();
        When();
    }

    [Then("the versions should have count 2")]
    [Trait("UAC", "16"), Fact]
    public void UAC16_CreatedVersionIsAdded() => _state.Versions.Count.Should().Be(2);
}
#endif