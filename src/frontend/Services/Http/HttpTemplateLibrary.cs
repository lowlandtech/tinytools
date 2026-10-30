using System.Net;
using System.Net.Http.Json;
using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Core.Entities;

namespace LowlandTech.TinyTools.Frontend.Services.Http;

/// <summary>
/// The store's data path: the same library contract the server implements, over
/// HTTP. The page never talks to a <see cref="HttpClient"/> itself, so a stubbed
/// transport is enough to prove the store.
/// </summary>
public sealed class HttpTemplateLibrary(HttpClient client) : ITemplateLibrary
{
    /// <summary>The route prefix the server serves the library under.</summary>
    public const string RoutePrefix = "/api/tinytools/templates";

    public async Task<List<Template>> ListAsync(CancellationToken ct = default) =>
        await client.GetFromJsonAsync<List<Template>>(RoutePrefix, ct) ?? [];

    public async Task<Template?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var response = await client.GetAsync($"{RoutePrefix}/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Template>(ct);
    }

    public async Task<Template> CreateAsync(Template template, CancellationToken ct = default)
    {
        var response = await client.PostAsJsonAsync(RoutePrefix, template, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Template>(ct)
               ?? throw new InvalidOperationException("The server accepted the template but returned no body.");
    }

    public async Task<Template?> UpdateAsync(Template template, CancellationToken ct = default)
    {
        var response = await client.PutAsJsonAsync($"{RoutePrefix}/{template.Id}", template, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Template>(ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var response = await client.DeleteAsync($"{RoutePrefix}/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return false;
        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<TemplateValidation?> ValidateVersionAsync(Guid versionId, CancellationToken ct = default)
    {
        var response = await client.PostAsync($"{RoutePrefix}/versions/{versionId}/validate", null, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        if (response.StatusCode == HttpStatusCode.BadRequest)
            throw new InvalidOperationException(await ReadErrorAsync(response, ct));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TemplateValidation>(ct);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ErrorBody>(ct);
            if (!string.IsNullOrWhiteSpace(problem?.Error)) return problem!.Error;
        }
        catch (NotSupportedException)
        {
            // A body that is not JSON falls through to the status text.
        }

        return response.ReasonPhrase ?? "The server refused the request.";
    }

    private sealed record ErrorBody(string? Error);
}
