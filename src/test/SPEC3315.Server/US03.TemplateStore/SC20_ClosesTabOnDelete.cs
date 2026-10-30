#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "20")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC20_ClosesTabOnDelete
{
    private readonly Guid _id = Guid.NewGuid();
    private TemplateState _state = new();

    private string TabKey => $"tpl:{_id}";

    [Given("an open template tab with unsaved changes")]
    private void Given() => _state = new TemplateState
    {
        OpenTabs = [new TemplateOpenTab($"tpl:{_id}", "X", "template", _id, true)],
        ActiveTabKey = $"tpl:{_id}",
        IsDirty = true,
    };

    [When("the delete completes for that template")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplateDeleteCompleted($"tpl:{_id}"));

    public SC20_ClosesTabOnDelete()
    {
        Given();
        When();
    }

    [Then("the open tabs should be empty")]
    [Trait("UAC", "18"), Fact]
    public void UAC18_DeleteClosesTab() => _state.OpenTabs.Should().BeEmpty();
}
#endif
