#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LowlandTech.TinyTools.Api.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;

/// <summary>
/// Hosts the API in memory over its own SQLite file, so each scenario owns its
/// database and nothing is shared between facts.
/// </summary>
public sealed class TemplateApiFactory(string environment = "Development", string? apiKey = null)
    : WebApplicationFactory<Program>
{
    private readonly string _database = Path.Combine(Path.GetTempPath(), $"tinytools-api-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environment);
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_database}");
        if (apiKey is not null)
            builder.UseSetting("Tinytools:ApiKey", apiKey);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try
        {
            File.Delete(_database);
        }
        catch (IOException)
        {
            // A leftover temp file is not a test failure.
        }
    }
}

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "01")]
[UserStory("02", "The server serves the template contract over HTTP")]
public class WhenServingTheTemplateContract
{
    private const string VersionedTemplate = """
        {
          "name": "EntityClass",
          "versions": [
            {
              "name": "v1",
              "content": "Hello ${Name}!",
              "json": "{\"Name\":\"Ada\"}",
              "text": "Hello Ada!"
            }
          ]
        }
        """;

    [Trait(Spec.UAC, "01")]
    [Fact]
    public async Task ItShouldExposeTheTemplateRoutes()
    {
        using var factory = new TemplateApiFactory();
        using var client = factory.CreateClient();

        var empty = await client.GetAsync("/api/tinytools/templates");
        var created = await client.PostAsync("/api/tinytools/templates",
            new StringContent(VersionedTemplate, System.Text.Encoding.UTF8, "application/json"));

        var createdBody = await created.Content.ReadAsStringAsync();
        created.StatusCode.Should().Be(HttpStatusCode.Created, because: createdBody);
        empty.StatusCode.Should().Be(HttpStatusCode.OK);

        var location = created.Headers.Location!;
        var fetched = await client.GetAsync(location);
        fetched.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await fetched.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("EntityClass");
        body.GetProperty("versions").GetArrayLength().Should().Be(1);
    }

    [Trait(Spec.UAC, "02")]
    [Fact]
    public async Task ItShouldRenderThroughThisRepositorysEngine()
    {
        using var factory = new TemplateApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/tinytools/templates/render",
            new { template = "Hello ${Name}, ${Context.City}!", json = "{\"Name\":\"Ada\",\"City\":\"Delft\"}" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("output").GetString().Should().Be("Hello Ada, Delft!");
    }

    [Trait(Spec.UAC, "03")]
    [Fact]
    public void ItShouldSelectTheProviderFromTheConnectionStringAndCreateTheSchema()
    {
        using var factory = new TemplateApiFactory();
        using var scope = factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<TinyToolsDbContext>();

        database.Database.ProviderName.Should().Be("Microsoft.EntityFrameworkCore.Sqlite");
        database.Model.GetDefaultSchema().Should().Be(TinyToolsDbContext.Schema);
        database.Database.CanConnect().Should().BeTrue();
    }

    [Trait(Spec.UAC, "04")]
    [Fact]
    public async Task ItShouldApplyOneDeclaredAuthorizationPolicy()
    {
        using var development = new TemplateApiFactory();
        using var devClient = development.CreateClient();
        (await devClient.GetAsync("/api/tinytools/templates")).StatusCode.Should().Be(HttpStatusCode.OK);

        using var productionWithoutKey = new TemplateApiFactory("Production");
        using var anonymous = productionWithoutKey.CreateClient();
        var refused = await anonymous.GetAsync("/api/tinytools/templates");
        refused.StatusCode.Should().BeOneOf([HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden], because: await refused.Content.ReadAsStringAsync());

        using var production = new TemplateApiFactory("Production", "secret-key");
        using var authorised = production.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/tinytools/templates");
        request.Headers.Add("X-Api-Key", "secret-key");
        (await authorised.SendAsync(request)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Trait(Spec.UAC, "05")]
    [Fact]
    public async Task ItShouldPublishTheSevenOperations()
    {
        using var factory = new TemplateApiFactory();
        using var client = factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        var paths = document.GetProperty("paths");

        var operations = paths.EnumerateObject()
            .SelectMany(path => path.Value.EnumerateObject().Select(operation => operation.Name))
            .ToList();

        operations.Should().Contain(["get", "post", "put", "delete"]);
        var pathNames = paths.EnumerateObject().Select(path => path.Name).ToList();
        pathNames.Should().Contain("/api/tinytools/templates/{id}");
        pathNames.Should().Contain("/api/tinytools/templates/render");
        pathNames.Should().Contain("/api/tinytools/templates/versions/{id}/validate");
    }

    [Trait(Spec.UAC, "06")]
    [Fact]
    public async Task ItShouldDriveCreateListRenderValidateAndDelete()
    {
        using var factory = new TemplateApiFactory();
        using var client = factory.CreateClient();

        var created = await client.PostAsync("/api/tinytools/templates",
            new StringContent(VersionedTemplate, System.Text.Encoding.UTF8, "application/json"));
        created.StatusCode.Should().Be(HttpStatusCode.Created, because: await created.Content.ReadAsStringAsync());

        var template = await created.Content.ReadFromJsonAsync<JsonElement>();
        var templateId = template.GetProperty("id").GetGuid();
        var versionId = template.GetProperty("versions")[0].GetProperty("id").GetGuid();

        var listed = await client.GetFromJsonAsync<JsonElement>("/api/tinytools/templates");
        listed.GetArrayLength().Should().Be(1);

        var rendered = await client.PostAsJsonAsync("/api/tinytools/templates/render",
            new { template = "Hello ${Name}!", json = "{\"Name\":\"Ada\"}" });
        (await rendered.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("output").GetString().Should().Be("Hello Ada!");

        var validated = await client.PostAsync($"/api/tinytools/templates/versions/{versionId}/validate", null);
        validated.StatusCode.Should().Be(HttpStatusCode.OK);
        var validation = await validated.Content.ReadFromJsonAsync<JsonElement>();
        validation.GetProperty("valid").GetBoolean().Should().BeTrue();
        validation.GetProperty("expected").GetString().Should().Be("Hello Ada!");

        (await client.DeleteAsync($"/api/tinytools/templates/{templateId}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.GetAsync($"/api/tinytools/templates/{templateId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
#endif
