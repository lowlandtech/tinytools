using LowlandTech.Plugins;
using LowlandTech.TinyTools.Api.Domain;
using LowlandTech.TinyTools.Api.Plugins;
using LowlandTech.TinyTools.Core.Abstractions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LowlandTech.TinyTools.Tests.SPEC3317.PluginPackaging.US01.Composition;

[Trait(Spec.SPEC, "3317")]
[Trait(Spec.SC, "04")]
[UserStory("01", "Tinytools ships as composable Harness plugin packages")]
public sealed class SC04_InstallsAgainstTheHostConnection : IClassFixture<UseCases.PluginPackagingUseCase>
{
    private readonly UseCases.PluginPackagingUseCase useCase;

    public SC04_InstallsAgainstTheHostConnection(UseCases.PluginPackagingUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("the mounted Tinytools plugin has a host database with an existing Packages table")]
    private void Given()
    {
        var databaseName = "tinytools-" + Guid.NewGuid().ToString("N");
        var connectionString = $"Data Source={databaseName};Mode=Memory;Cache=Shared";
        var anchor = new SqliteConnection(connectionString);
        anchor.Open();
        using var command = anchor.CreateCommand();
        command.CommandText = "CREATE TABLE host_marker (Id INTEGER PRIMARY KEY); CREATE TABLE Packages (Id TEXT PRIMARY KEY); INSERT INTO Packages VALUES ('host-package');";
        command.ExecuteNonQuery();
        useCase.Cases["ConnectionString"] = connectionString;
        useCase.Cases["DatabaseName"] = databaseName;
        useCase.Cases["HostConnection"] = anchor;
    }

    [When("the plugin registers and installs its schema against the host connection")]
    private void When()
    {
        var connectionString = (string)useCase.Cases["ConnectionString"]!;
        var anchor = (SqliteConnection)useCase.Cases["HostConnection"]!;
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString
            })
            .Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        var registration = new PluginServiceRegistrationAdapter(services);
        var plugin = new TinyToolsMountedPlugin();
        plugin.RegisterAsync(new PluginRegistrationContext
        {
            PluginId = plugin.Descriptor.PluginId,
            PluginVersion = plugin.Descriptor.Version,
            Targets = plugin.Descriptor.Targets,
            Services = registration
        }, CancellationToken.None).GetAwaiter().GetResult();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TinyToolsDbContext>>();
        useCase.Cases["LibraryRegistered"] = scope.ServiceProvider.GetRequiredService<ITemplateLibrary>() is not null;
        var install = new PluginInstallContext
        {
            PluginId = plugin.Descriptor.PluginId,
            PluginVersion = plugin.Descriptor.Version,
            HostDeployment = "test-deployment",
            Connection = new PluginHostConnection { Name = "DefaultConnection", Provider = "SQLite", Database = (string)useCase.Cases["DatabaseName"]! },
            Receipts = new InMemoryInstallReceiptStore(),
            Services = new Dictionary<Type, object> { [typeof(IDbContextFactory<TinyToolsDbContext>)] = factory }
        };
        var result = plugin.InstallAsync(install, CancellationToken.None).GetAwaiter().GetResult();
        useCase.Cases["Migration"] = result.Migration;
        useCase.Cases["MissingConnection"] = plugin.InstallAsync(install with { Connection = null }, CancellationToken.None)
            .GetAwaiter().GetResult().Refusal?.Code;
        useCase.Cases["HostDataPreserved"] = SqliteTableCatalog.Contains(anchor, "host_marker")
            && SqliteTableCatalog.Contains(anchor, "Packages") && SqliteTableCatalog.HasRows(anchor, "Packages")
            && SqliteTableCatalog.Contains(anchor, "TinyTools_Packages") && SqliteTableCatalog.Contains(anchor, "TinyTools_Templates");
    }

    [Then("the template library registration should be true")]
    [Trait("UAC", "23"), Fact]
    public void UAC23_RegistersTemplateServices() => ((bool)useCase.Cases["LibraryRegistered"]!).Should().BeTrue();

    [Then("the migration outcome should be Applied")]
    [Trait("UAC", "24"), Fact]
    public void UAC24_MigratesHostSchema() => ((PluginMigrationOutcome?)useCase.Cases["Migration"]).Should().Be(PluginMigrationOutcome.Applied);

    [Then("the host data preservation check should be true")]
    [Trait("UAC", "25"), Fact]
    public void UAC25_PreservesHostAndCreatesTemplateSchema() => ((bool)useCase.Cases["HostDataPreserved"]!).Should().BeTrue();
    [Then("the missing connection refusal should be MissingHostConnection")]
    [Trait("UAC", "26"), Fact]
    public void UAC26_RefusesMissingHostConnection() => ((PluginLifecycleRefusalCode?)useCase.Cases["MissingConnection"]).Should().Be(PluginLifecycleRefusalCode.MissingHostConnection);
}
