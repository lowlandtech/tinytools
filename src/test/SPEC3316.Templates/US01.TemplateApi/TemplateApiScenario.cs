#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Api.Domain;
using LowlandTech.TinyTools.Core.Entities;
using LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LowlandTech.TinyTools.Tests.SPEC3316.Templates.US01.TemplateApi;

/// <summary>
/// Shared plumbing for the imported template API specifications: it hosts the API
/// and exposes the seeded library, while each scenario keeps its own Given and
/// When in its own source.
///
/// Foundry's base derived ids from <c>GuidV5</c>; here the seed owns the ids and
/// exposes them, which keeps the same assertions without porting a hashing helper.
/// </summary>
public abstract class TemplateApiScenario : IAsyncLifetime
{
    private TemplateApiFactory _factory = null!;

    protected HttpClient Client { get; private set; } = null!;
    protected Guid EntityClassId { get; private set; }
    protected Guid EntityClassV1Id { get; private set; }
    protected Guid DbContextClassId { get; private set; }
    protected Guid DbContextClassV1Id { get; private set; }

    /// <summary>Hosts the API. The library itself is seeded by the scenario's Given.</summary>
    public virtual ValueTask InitializeAsync()
    {
        _factory = new TemplateApiFactory();
        Client = _factory.CreateClient();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Seeds one template whose only version carries test data and one whose
    /// version does not, so the validation refusal has a subject.
    /// </summary>
    protected async Task SeedLibraryAsync()
    {
        EntityClassId = Guid.NewGuid();
        EntityClassV1Id = Guid.NewGuid();
        DbContextClassId = Guid.NewGuid();
        DbContextClassV1Id = Guid.NewGuid();

        await using var database = CreateDbContext();
        database.Templates.Add(new Template
        {
            Id = EntityClassId,
            Name = "EntityClass",
            Versions =
            [
                new TemplateVersion
                {
                    Id = EntityClassV1Id,
                    Name = "v1",
                    Content = "Hello ${Context.Name}",
                    Json = """{"Name":"World"}""",
                    Text = "Hello World",
                    TemplateId = EntityClassId,
                },
            ],
        });
        database.Templates.Add(new Template
        {
            Id = DbContextClassId,
            Name = "DbContextClass",
            Versions =
            [
                new TemplateVersion
                {
                    Id = DbContextClassV1Id,
                    Name = "v1",
                    Content = "class ${Context.Name} { }",
                    TemplateId = DbContextClassId,
                },
            ],
        });

        await database.SaveChangesAsync();
    }

    protected TinyToolsDbContext CreateDbContext() => _factory.CreateDbContext();

    public ValueTask DisposeAsync()
    {
        Client?.Dispose();
        _factory?.Dispose();
        return ValueTask.CompletedTask;
    }
}
#endif
