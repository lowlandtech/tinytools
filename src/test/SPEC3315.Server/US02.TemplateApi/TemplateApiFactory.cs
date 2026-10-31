#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Api.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;

/// <summary>Hosts the API in memory over its own SQLite file.</summary>
public sealed class TemplateApiFactory(string environment = "Development", string? apiKey = null, IDictionary<string, string?>? mutableSettings = null)
    : WebApplicationFactory<Program>
{
    private readonly string _database = Path.Combine(Path.GetTempPath(), $"tinytools-api-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environment);
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_database}");
        if (apiKey is not null) builder.UseSetting("Tinytools:ApiKey", apiKey);
        if (mutableSettings is not null)
            builder.ConfigureAppConfiguration((_, configuration) => configuration.Add(new MutableSettingsSource(mutableSettings)));
    }

    /// <summary>Opens the scenario database for facts that assert on persisted state.</summary>
    public TinyToolsDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<TinyToolsDbContext>().UseSqlite($"Data Source={_database}").Options);

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try { File.Delete(_database); } catch (IOException) { }
    }
}
#endif