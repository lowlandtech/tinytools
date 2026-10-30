namespace LowlandTech.TinyTools.Core.Abstractions;

using LowlandTech.TinyTools.Core.Entities;

/// <summary>
/// The template library the server exposes: the aggregate, its versions and the
/// validation of one version against its own recorded test data.
/// </summary>
public interface ITemplateLibrary
{
    Task<List<Template>> ListAsync(CancellationToken ct = default);

    Task<Template?> GetAsync(Guid id, CancellationToken ct = default);

    Task<Template> CreateAsync(Template template, CancellationToken ct = default);

    Task<Template?> UpdateAsync(Template template, CancellationToken ct = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Renders the version's content with its test JSON and compares the result
    /// with its expected text. Returns null when the version does not exist and
    /// fails with a named error when the version carries no test data.
    /// </summary>
    Task<TemplateValidation> ValidateVersionAsync(Guid versionId, CancellationToken ct = default);
}

/// <summary>The outcome of rendering a version and comparing it with its expected text.</summary>
/// <param name="Valid">Whether the rendered output matches the expected text.</param>
/// <param name="Output">The normalized rendered output.</param>
/// <param name="Expected">The normalized expected text.</param>
public sealed record TemplateValidation(bool Valid, string Output, string Expected);
