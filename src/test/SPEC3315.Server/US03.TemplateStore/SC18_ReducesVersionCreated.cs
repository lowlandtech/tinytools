#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Core.Entities;
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "18")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC18_ReducesVersionCreated
{
    private readonly TemplateVersion _created = new() { Content = "new", Text = "txt" };
    private TemplateState _state = new();

    [Given("an editor showing one version")]
    private void Given() => _state = new TemplateState { Versions = [new() { Content = "old" }], VersionIndex = 0 };

    [When("a new version is created")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplateVersionCreated(_created));

    public SC18_ReducesVersionCreated()
    {
        Given();
        When();
    }

    [Then("the created version should become the visible edition")]
    [Trait("UAC", "16"), Fact]
    public void UAC16_CreatedVersionIsAdded() =>
        new { Count = _state.Versions.Count, First = _state.Versions[0], _state.VersionIndex, _state.VersionContent, _state.VersionText }
            .Should().BeEquivalentTo(new
            {
                Count = 2,
                First = _created,
                VersionIndex = 0,
                VersionContent = "new",
                VersionText = "txt",
            });
}
#endif
