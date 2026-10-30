using LowlandTech.TinyTools.Api.Domain;
using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace LowlandTech.TinyTools.Api.Services;

/// <summary>The template library over the server's own database.</summary>
public sealed class TemplateService(TinyToolsDbContext database, ITemplateExecutor executor) : ITemplateLibrary
{
    public Task<List<Template>> ListAsync(CancellationToken ct = default) =>
        database.Templates
            .AsNoTracking()
            .Include(template => template.Versions)
            .Include(template => template.Packages)
            .OrderBy(template => template.Name)
            .ToListAsync(ct);

    public Task<Template?> GetAsync(Guid id, CancellationToken ct = default) =>
        database.Templates
            .Include(template => template.Versions)
            .Include(template => template.Packages).ThenInclude(package => package.Imports)
            .FirstOrDefaultAsync(template => template.Id == id, ct);

    public async Task<Template> CreateAsync(Template template, CancellationToken ct = default)
    {
        database.Templates.Add(template);
        await database.SaveChangesAsync(ct);
        return template;
    }

    public async Task<Template?> UpdateAsync(Template template, CancellationToken ct = default)
    {
        database.Entry(template).State = EntityState.Modified;
        await database.SaveChangesAsync(ct);
        return template;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var template = await database.Templates.FindAsync([id], ct);
        if (template is null) return false;
        database.Templates.Remove(template);
        await database.SaveChangesAsync(ct);
        return true;
    }

    public async Task<TemplateValidation?> ValidateVersionAsync(Guid versionId, CancellationToken ct = default)
    {
        var version = await database.TemplateVersions.FindAsync([versionId], ct);
        if (version is null) return null;
        if (version.Json is null || version.Text is null)
            throw new InvalidOperationException("The version carries no test JSON or expected text.");

        var validation = executor.Validate(version.Content, version.Json, version.Text);
        version.IsValid = validation.Valid;
        await database.SaveChangesAsync(ct);
        return validation;
    }
}
