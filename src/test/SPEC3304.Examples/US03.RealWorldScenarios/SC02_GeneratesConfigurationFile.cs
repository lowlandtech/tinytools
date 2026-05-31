using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "02")]
[UserStory("03", "Real-world scenarios generate practical output")]
public class WhenRenderingConfigurationFileTest : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    [Given("Setup test context")]
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Render App Settings")]
    [Fact]
    public void ItShouldRenderAppSettings()
    {
        ArrangeAndAct();
        _result.Should().Contain("\"Name\": \"MyWebApp\"");
        _result.Should().Contain("\"Environment\": \"production\"");
        _result.Should().Contain("\"Port\": 8080");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Render Https As True")]
    [Fact]
    public void ItShouldRenderHttpsAsTrue()
    {
        ArrangeAndAct();
        _result.Should().Contain("\"UseHttps\": true");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Render Database Config")]
    [Fact]
    public void ItShouldRenderDatabaseConfig()
    {
        ArrangeAndAct();
        _result.Should().Contain("\"Host\": \"db.example.com\"");
        _result.Should().Contain("Server=db.example.com;Database=myapp_prod");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Include Cache Section")]
    [Fact]
    public void ItShouldIncludeCacheSection()
    {
        ArrangeAndAct();
        _result.Should().Contain("\"Cache\":");
        _result.Should().Contain("\"Host\": \"redis.example.com\"");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Render Feature Flags")]
    [Fact]
    public void ItShouldRenderFeatureFlags()
    {
        ArrangeAndAct();
        _result.Should().Contain("\"FeatureA\": True");
        _result.Should().Contain("\"FeatureB\": False");
        _result.Should().Contain("\"FeatureC\": True");
    }

    [Trait(Spec.UAC, "06")]
    [Then("it Should Remove Config Comments")]
    [Fact]
    public void ItShouldRemoveConfigComments()
    {
        ArrangeAndAct();
        _result.Should().NotContain("Configuration for");
    }
}
