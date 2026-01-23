namespace LowlandTech.TinyTools.UnitTests;

public class WhenRenderingConfigurationFileTest : WhenTestingFor<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    protected override void Given()
    {
        _context = new ExecutionContext();
        _context.Set("Environment", "production");
        _context.Set("AppName", "MyWebApp");
        _context.Set("Port", 8080);
        _context.Set("EnableHttps", true);
        _context.Set("DatabaseHost", "db.example.com");
        _context.Set("DatabaseName", "myapp_prod");
        _context.Set("CacheEnabled", true);
        _context.Set("CacheHost", "redis.example.com");
        _context.Set("LogLevel", "Warning");
        _context.Set("Features", new List<object>
        {
            new { Name = "FeatureA", Enabled = true },
            new { Name = "FeatureB", Enabled = false },
            new { Name = "FeatureC", Enabled = true }
        });

        _template = """
            @* Configuration for ${Context.Environment} environment *@
            {
              "AppSettings": {
                "Name": "${Context.AppName}",
                "Environment": "${Context.Environment}",
                "Port": ${Context.Port},
            @if (Context.EnableHttps) {
                "UseHttps": true,
            } else {
                "UseHttps": false,
            }
                "Logging": {
                  "LogLevel": "${Context.LogLevel ?? "Information"}"
                }
              },
              "Database": {
                "Host": "${Context.DatabaseHost}",
                "Name": "${Context.DatabaseName}",
                "ConnectionString": "Server=${Context.DatabaseHost};Database=${Context.DatabaseName}"
              },
            @if (Context.CacheEnabled) {
              "Cache": {
                "Enabled": true,
                "Host": "${Context.CacheHost ?? "localhost"}"
              },
            }
              "FeatureFlags": {
            @foreach (var feature in Context.Features) {
                "${feature.Name}": ${feature.Enabled}
            }
              }
            }
            """;
    }

    protected override void When()
    {
        _result = Sut.Render(_template, _context);
    }

    [Fact]
    public void ItShouldRenderAppSettings()
    {
        _result.Should().Contain("\"Name\": \"MyWebApp\"");
        _result.Should().Contain("\"Environment\": \"production\"");
        _result.Should().Contain("\"Port\": 8080");
    }

    [Fact]
    public void ItShouldRenderHttpsAsTrue()
    {
        _result.Should().Contain("\"UseHttps\": true");
    }

    [Fact]
    public void ItShouldRenderDatabaseConfig()
    {
        _result.Should().Contain("\"Host\": \"db.example.com\"");
        _result.Should().Contain("Server=db.example.com;Database=myapp_prod");
    }

    [Fact]
    public void ItShouldIncludeCacheSection()
    {
        _result.Should().Contain("\"Cache\":");
        _result.Should().Contain("\"Host\": \"redis.example.com\"");
    }

    [Fact]
    public void ItShouldRenderFeatureFlags()
    {
        _result.Should().Contain("\"FeatureA\": True");
        _result.Should().Contain("\"FeatureB\": False");
        _result.Should().Contain("\"FeatureC\": True");
    }

    [Fact]
    public void ItShouldRemoveConfigComments()
    {
        _result.Should().NotContain("Configuration for");
    }
}
