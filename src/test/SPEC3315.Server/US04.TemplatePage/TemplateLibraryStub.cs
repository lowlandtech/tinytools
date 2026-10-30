#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Core.Entities;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US04.TemplatePage;

/// <summary>
/// The library the page reads through in the component scenario: it returns a
/// fixed template set, so the scenario proves rendering rather than transport.
/// </summary>
internal sealed class TemplateLibraryStub(params Template[] templates) : ITemplateLibrary
{
    public List<Template> ListAsync_calls { get; } = [];

    public Task<List<Template>> ListAsync(CancellationToken ct = default)
    {
        ListAsync_calls.AddRange(templates);
        return Task.FromResult(templates.ToList());
    }

    public Task<Template?> GetAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(templates.FirstOrDefault(template => template.Id == id));

    public Task<Template> CreateAsync(Template template, CancellationToken ct = default)
        => Task.FromResult(template);

    public Task<Template?> UpdateAsync(Template template, CancellationToken ct = default)
        => Task.FromResult<Template?>(template);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(templates.Any(template => template.Id == id));

    public Task<TemplateValidation> ValidateVersionAsync(Guid versionId, CancellationToken ct = default)
        => Task.FromResult(new TemplateValidation(true, "output", "output"));
}
#endif
