using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LowlandTech.TinyTools.Api.Domain;

/// <summary>Builds the provider-neutral model snapshot without connecting to a production database.</summary>
public sealed class TinyToolsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TinyToolsDbContext>
{
    public TinyToolsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TinyToolsDbContext>()
            .UseNpgsql("Host=localhost;Database=tinytools;Username=design;Password=design")
            .Options;
        return new TinyToolsDbContext(options);
    }
}
