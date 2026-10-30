using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "02")]
[UserStory("03", "Real-world scenarios generate practical output")]
public sealed class SC02_GeneratesConfigurationFile : UnitScenario
{
    private readonly TinyTemplateEngine _engine = new();
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string _result = null!;

    [Given("a production configuration template with its data")]
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

    [When("the engine renders the configuration template")]
    protected override void When() => _result = _engine.Render(_template, _context);

    public SC02_GeneratesConfigurationFile() => ArrangeAndAct();

    [Then("the content should contain the app settings")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersTheAppSettings() => _result.Should().Contain("\"Name\": \"MyWebApp\"").And.Contain("\"Environment\": \"production\"").And.Contain("\"Port\": 8080");

    [Then("the content should contain the https flag")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersHttpsAsTrue() => _result.Should().Contain("\"UseHttps\": true");

    [Then("the content should contain the database settings")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RendersTheDatabaseConfig() => _result.Should().Contain("\"Host\": \"db.example.com\"").And.Contain("Server=db.example.com;Database=myapp_prod");

    [Then("the content should contain the cache section")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_IncludesTheCacheSection() => _result.Should().Contain("\"Cache\":").And.Contain("\"Host\": \"redis.example.com\"");

    [Then("the content should contain the feature flags")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RendersTheFeatureFlags() => _result.Should().Contain("\"FeatureA\": True").And.Contain("\"FeatureB\": False").And.Contain("\"FeatureC\": True");

    [Then("the content should not contain the configuration comment")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_RemovesTheConfigComments() => _result.Should().NotContain("Configuration for");
}
