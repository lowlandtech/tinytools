#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Core.Entities;
using LowlandTech.TinyTools.Frontend.Services.Http;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "02")]
[UserStory("03", "The templates store reaches the server through one HTTP client")]
public sealed class SC02_CallsTheTemplateApi : IAsyncLifetime, IClassFixture<UseCases.TemplateStoreUseCase>
{
    private readonly UseCases.TemplateStoreUseCase useCase;
    private RoutedTemplateHandler _handler = null!;
    private HttpTemplateLibrary _library = null!;

    public SC02_CallsTheTemplateApi(UseCases.TemplateStoreUseCase useCase) => this.useCase = useCase;

    [Given("a routed template API behind one HTTP client")]
    private void Given()
    {
        _handler = new RoutedTemplateHandler();
        var client = new HttpClient(_handler) { BaseAddress = new Uri("http://tinytools.test") };
        _library = new HttpTemplateLibrary(client);
    }

    [When("list, create, get-missing, delete, validate and the refusal are called")]
    private async Task WhenAsync()
    {
        useCase.Cases["Listed"] = (await _library.ListAsync()).Count;
        useCase.Cases["Created"] = (await _library.CreateAsync(new Template { Name = "EntityClass" })).Name;
        useCase.Cases["MissingIsNull"] = await _library.GetAsync(Guid.NewGuid()) is null;
        useCase.Cases["Deleted"] = await _library.DeleteAsync(Guid.NewGuid());
        useCase.Cases["Valid"] = (await _library.ValidateVersionAsync(Guid.NewGuid()))!.Valid;
        try { await _library.ValidateVersionAsync(RoutedTemplateHandler.RefusingVersion); }
        catch (InvalidOperationException exception) { useCase.Cases["Message"] = exception.Message; }
        useCase.Cases["Calls"] = string.Join(" | ", _handler.Calls);
    }

    public ValueTask InitializeAsync()
    {
        useCase.RunOnce(Given, () => WhenAsync().GetAwaiter().GetResult());
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Then("the listed templates should have count 1")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ListsThroughTheDocumentedRoute() => ((int)useCase.Cases["Listed"]!).Should().Be(1);

    [Then("the created name should contain EntityClass")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_CreatesThroughTheRoute() => ((string?)useCase.Cases["Created"]!).Should().Contain("EntityClass");

    [Then("the missing template should be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ReportsMissingAsNull() => ((bool)useCase.Cases["MissingIsNull"]!).Should().Be(true);

    [Then("the delete should be true")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_DeletesThroughTheRoute() => ((bool)useCase.Cases["Deleted"]!).Should().Be(true);

    [Then("the validation should be true")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ValidatesAVersion() => ((bool)useCase.Cases["Valid"]!).Should().Be(true);

    [Then("the refusal message should contain no test data")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_SurfacesTheServersRefusal() => ((string)useCase.Cases["Message"]!).Should().Contain("no test data");

    [Then("the calls should contain delete")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_UsesTheDocumentedMethods() => ((string)useCase.Cases["Calls"]!).Should().Contain("DELETE");
}
#endif
