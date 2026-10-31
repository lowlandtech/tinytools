using LowlandTech.TinyTools.Api.Authorization;
using LowlandTech.TinyTools.Api.Domain;
using LowlandTech.TinyTools.Api.Endpoints;
using LowlandTech.TinyTools.Api.Services;
using LowlandTech.TinyTools.Core.Abstractions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<MalformedRequestExceptionHandler>();

// One connection string, two providers: a Npgsql string when it names a host,
// SQLite otherwise, which is what a developer and the tests use.
builder.Services.AddDbContext<TinyToolsDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (connectionString is not null && connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
        options.UseNpgsql(connectionString);
    else
        options.UseSqlite(connectionString ?? "Data Source=tinytools.db");
});

builder.Services.AddSingleton<ITemplateExecutor, TemplateExecutor>();
builder.Services.AddScoped<ITemplateLibrary, TemplateService>();
builder.Services.AddApiKeyPolicy();
builder.Services.AddSingleton<CallerProvenanceLog>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<CallerProvenanceMiddleware>();
app.UseStatusCodePages();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapTemplateEndpoints();

// The database is created rather than migrated: the schema is the imported
// entity model, and the packaging item owns migrations when the server becomes
// a deployed plugin.
using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<TinyToolsDbContext>();
    await database.Database.EnsureCreatedAsync();
}

await app.RunAsync();

/// <summary>The entry point the integration tests host.</summary>
public partial class Program
{
}
