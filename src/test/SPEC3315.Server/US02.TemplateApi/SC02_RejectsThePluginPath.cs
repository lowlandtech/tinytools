#if NET10_0_OR_GREATER
using System.Net;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "02")]
[UserStory("02", "The server serves the template contract over HTTP")]
public sealed class SC02_RejectsThePluginPath : IAsyncLifetime
{
    private TemplateApiFactory _development = null!;
    private HttpStatusCode _status;

    [Given("a served template library in development")]
    private void Given() => _development = new TemplateApiFactory();

    [When("the obsolete plugin prefixed template path is requested")]
    private async Task WhenAsync() => _status = (await _development.CreateClient().GetAsync("/api/tinytools/templates")).StatusCode;

    public async ValueTask InitializeAsync()
    {
        Given();
        await WhenAsync();
    }

    public ValueTask DisposeAsync()
    {
        _development?.Dispose();
        return ValueTask.CompletedTask;
    }

    [Then("the plugin path status should be NotFound")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_ObsoletePathDoesNotDispatch() => _status.Should().Be(HttpStatusCode.NotFound);
}
#endif
