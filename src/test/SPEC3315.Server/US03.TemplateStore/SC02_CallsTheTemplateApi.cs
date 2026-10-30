#if NET10_0_OR_GREATER
using System.Net;
using System.Text;
using System.Text.Json;
using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Core.Entities;
using LowlandTech.TinyTools.Frontend.Services.Http;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

/// <summary>Records the request and answers with one canned response.</summary>
public sealed class StubHandler(HttpStatusCode status, string body = "[]") : HttpMessageHandler
{
    public HttpRequestMessage? Request { get; private set; }
    public string? RequestBody { get; private set; }
    public string? RequestUri { get; private set; }
    public string? RequestPath => Request?.RequestUri?.AbsolutePath;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Request = request;
        RequestUri = request.RequestUri?.ToString();
        if (request.Content is not null)
            RequestBody = await request.Content.ReadAsStringAsync(cancellationToken);

        return new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
    }
}

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "02")]
[UserStory("03", "The templates store reaches the server through one HTTP client")]
public class WhenCallingTheTemplateApi
{
    private static (HttpTemplateLibrary Library, StubHandler Handler) Create(HttpStatusCode status, string body = "[]")
    {
        var handler = new StubHandler(status, body);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://tinytools.test") };
        return (new HttpTemplateLibrary(client), handler);
    }

    [Trait(Spec.UAC, "01")]
    [Fact]
    public async Task ItShouldListThroughTheDocumentedRoute()
    {
        var (library, handler) = Create(HttpStatusCode.OK, """
            [{"id":"5f1b2c3d-0000-4000-8000-000000000001","name":"EntityClass","isActive":true,"versions":[],"packages":[]}]
            """);

        var templates = await library.ListAsync();

        handler.RequestPath.Should().Be("/api/tinytools/templates");
        handler.Request!.Method.Should().Be(HttpMethod.Get);
        templates.Should().ContainSingle().Which.Name.Should().Be("EntityClass");
    }

    [Trait(Spec.UAC, "01")]
    [Fact]
    public async Task ItShouldCreateThroughTheRouteAndReturnTheStoredTemplate()
    {
        var (library, handler) = Create(HttpStatusCode.Created, """
            {"id":"5f1b2c3d-0000-4000-8000-000000000002","name":"DbContextClass","isActive":true,"versions":[],"packages":[]}
            """);

        var created = await library.CreateAsync(new Template { Name = "DbContextClass" });

        handler.Request!.Method.Should().Be(HttpMethod.Post);
        handler.RequestBody.Should().Contain("DbContextClass");
        created.Id.Should().Be(Guid.Parse("5f1b2c3d-0000-4000-8000-000000000002"));
    }

    [Trait(Spec.UAC, "01")]
    [Fact]
    public async Task ItShouldReportAMissingTemplateAsNull()
    {
        var (library, _) = Create(HttpStatusCode.NotFound);

        var template = await library.GetAsync(Guid.NewGuid());

        template.Should().BeNull();
    }

    [Trait(Spec.UAC, "01")]
    [Fact]
    public async Task ItShouldDeleteThroughTheRouteAndReportRefusal()
    {
        var (deleted, deletedHandler) = Create(HttpStatusCode.NoContent, "");
        (await deleted.DeleteAsync(Guid.NewGuid())).Should().BeTrue();
        deletedHandler.Request!.Method.Should().Be(HttpMethod.Delete);

        var (missing, _) = Create(HttpStatusCode.NotFound);
        (await missing.DeleteAsync(Guid.NewGuid())).Should().BeFalse();
    }

    [Trait(Spec.UAC, "01")]
    [Fact]
    public async Task ItShouldValidateAVersionThroughTheVersionRoute()
    {
        var versionId = Guid.NewGuid();
        var (library, handler) = Create(HttpStatusCode.OK, """
            {"valid":true,"output":"Hello Ada!","expected":"Hello Ada!"}
            """);

        var validation = await library.ValidateVersionAsync(versionId);

        handler.RequestPath.Should().Be($"/api/tinytools/templates/versions/{versionId}/validate");
        handler.Request!.Method.Should().Be(HttpMethod.Post);
        validation.Should().NotBeNull();
        validation!.Valid.Should().BeTrue();
        validation.Output.Should().Be("Hello Ada!");
    }

    [Trait(Spec.UAC, "01")]
    [Fact]
    public async Task ItShouldSurfaceTheServersRefusalReason()
    {
        var (library, _) = Create(HttpStatusCode.BadRequest, """
            {"error":"The version carries no test JSON or expected text."}
            """);

        var act = () => library.ValidateVersionAsync(Guid.NewGuid());

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*no test JSON*");
    }
}
#endif
