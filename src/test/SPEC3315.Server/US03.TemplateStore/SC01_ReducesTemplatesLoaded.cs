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

    private readonly Dictionary<Guid, TemplateVersion?> _cache = new();

    private TemplateState _state = new();

    [Given("a store with no templates loaded")]
    private void Given() => _state = new TemplateState();

    [When("the loaded templates arrive")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplatesLoaded(_items, _cache));

    public SC01_ReducesTemplatesLoaded()
    {
        Given();
        When();
    }

    [Then("the loaded library should be stored")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_LoadedTemplatesAreStored() => new { Count = _state.AllItems.Count, Cache = _state.VersionCache }.Should().BeEquivalentTo(new { Count = 2, Cache = _cache });
}
#endif
