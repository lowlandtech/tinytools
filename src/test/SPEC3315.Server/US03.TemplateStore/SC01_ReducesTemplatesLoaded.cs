#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Core.Entities;
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "01")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC01_ReducesTemplatesLoaded
{
    private readonly List<Template> _items =
    [
        new() { Id = Guid.NewGuid(), Name = "Foo" },
        new() { Id = Guid.NewGuid(), Name = "Bar" },
    ];

    private TemplateState _state = new();

    [Given("a store with no templates loaded")]
    private void Given() => _state = new TemplateState();

    [When("the loaded templates arrive")]
    private void When() => _state = TemplateReducer.Reduce(
        _state, new TemplatesLoaded(_items, new Dictionary<Guid, TemplateVersion?>()));

    public SC01_ReducesTemplatesLoaded()
    {
        Given();
        When();
    }

    [Then("the templates should have count 2")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_LoadedTemplatesAreStored() => _state.AllItems.Should().HaveCount(2);
}
#endif
