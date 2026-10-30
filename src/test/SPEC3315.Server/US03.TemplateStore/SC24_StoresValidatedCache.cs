#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Core.Entities;
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "24")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC24_StoresValidatedCache : IClassFixture<UseCases.TemplateStoreUseCase>
{
    private readonly UseCases.TemplateStoreUseCase useCase;
    private readonly Dictionary<Guid, TemplateVersion?> _cache = new();
    private TemplateState _state = new();

    [Given("a validation run over every template")]
    private void Given() => _state = new TemplateState();

    [When("the validate-all run completes")]
    private void When()
    {
        _state = TemplateReducer.Reduce(_state, new TemplateValidateAllStarted());
        var running = _state.IsValidatingAll;
        _state = TemplateReducer.Reduce(_state, new TemplateValidateAllCompleted(_cache));
        useCase.Cases["Cache"] = _cache;
        useCase.Cases["Validation"] = new { Started = running, _state.IsValidatingAll, Cache = _state.VersionCache };
    }

    public SC24_StoresValidatedCache(UseCases.TemplateStoreUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Then("the validated cache should be stored")]
    [Trait("UAC", "22"), Fact]
    public void UAC22_CompletedStoresCache() => useCase.Cases["Validation"].Should().BeEquivalentTo(new { Started = true, IsValidatingAll = false, Cache = (Dictionary<Guid, TemplateVersion?>)useCase.Cases["Cache"]! });
}
#endif
