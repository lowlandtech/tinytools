#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Core.Entities;
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "24")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC24_StoresValidatedCache
{
    private readonly Dictionary<Guid, TemplateVersion?> _cache = new();
    private TemplateState _state = new();

    [Given("a validation run over every template")]
    private void Given()
    {
        _state = TemplateReducer.Reduce(new TemplateState(), new TemplateValidateAllStarted());
    }

    [When("the validate-all run completes")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplateValidateAllCompleted(_cache));

    public SC24_StoresValidatedCache()
    {
        Given();
        When();
    }

    [Then("the version cache should be the validated cache")]
    [Trait("UAC", "22"), Fact]
    public void UAC22_CompletedStoresCache() => _state.VersionCache.Should().BeSameAs(_cache);
}
#endif
