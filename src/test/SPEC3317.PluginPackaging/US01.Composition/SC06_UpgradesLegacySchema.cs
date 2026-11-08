using LowlandTech.Plugins;
using LowlandTech.TinyTools.Api.Domain;
using LowlandTech.TinyTools.Api.Plugins;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LowlandTech.TinyTools.Tests.SPEC3317.PluginPackaging.US01.Composition;

[Trait(Spec.SPEC, "3317")]
[Trait(Spec.SC, "06")]
[UserStory("01", "Tinytools ships as composable Harness plugin packages")]
public sealed class SC06_UpgradesLegacySchema : IClassFixture<UseCases.PluginPackagingUseCase>
{
    private readonly UseCases.PluginPackagingUseCase useCase;

    public SC06_UpgradesLegacySchema(UseCases.PluginPackagingUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("the host database has the previously applied TinyTools migration and package data")]
    private void Given()
    {
        var databaseName = "tinytools-legacy-" + Guid.NewGuid().ToString("N");
        var connectionString = $"Data Source={databaseName};Mode=Memory;Cache=Shared";
        var anchor = new SqliteConnection(connectionString);
        anchor.Open();
        using var command = anchor.CreateCommand();
        command.CommandText = "CREATE TABLE __EFMigrationsHistory (MigrationId TEXT PRIMARY KEY, ProductVersion TEXT NOT NULL);"
            + "INSERT INTO __EFMigrationsHistory VALUES ('20261108012126_InitialTinyToolsSchema', '10.0.9');"
            + "CREATE TABLE Packages (Id TEXT PRIMARY KEY); INSERT INTO Packages VALUES ('legacy-package');"
            + "CREATE TABLE StepIntents (Id TEXT); CREATE TABLE TemplateFlows (Id TEXT); CREATE TABLE Templates (Id TEXT);"
            + "CREATE TABLE Imports (Id TEXT); CREATE TABLE TemplateSteps (Id TEXT);"
            + "CREATE TABLE TemplateTemplatePackage (Id TEXT); CREATE TABLE TemplateVersions (Id TEXT);";
        command.ExecuteNonQuery();
        useCase.Cases["ConnectionString"] = connectionString;
        useCase.Cases["DatabaseName"] = databaseName;
        useCase.Cases["HostConnection"] = anchor;
    }

    [When("the mounted plugin installs against the existing host connection")]
    private void When()
    {
        var connectionString = (string)useCase.Cases["ConnectionString"]!;
        var anchor = (SqliteConnection)useCase.Cases["HostConnection"]!;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            { ["ConnectionStrings:DefaultConnection"] = connectionString }).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        var registration = new PluginServiceRegistrationAdapter(services);
        var plugin = new TinyToolsMountedPlugin();
        plugin.RegisterAsync(new PluginRegistrationContext
            { PluginId = plugin.Descriptor.PluginId, PluginVersion = plugin.Descriptor.Version, Targets = plugin.Descriptor.Targets, Services = registration }, CancellationToken.None).GetAwaiter().GetResult();
        registration.Close();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TinyToolsDbContext>>();
        var install = new PluginInstallContext
        {
            PluginId = plugin.Descriptor.PluginId, PluginVersion = plugin.Descriptor.Version, HostDeployment = "legacy-deployment",
            Connection = new PluginHostConnection { Name = "DefaultConnection", Provider = "SQLite", Database = (string)useCase.Cases["DatabaseName"]! },
            Receipts = new InMemoryInstallReceiptStore(),
            Services = new Dictionary<Type, object> { [typeof(IDbContextFactory<TinyToolsDbContext>)] = factory }
        };
        var result = plugin.InstallAsync(install, CancellationToken.None).GetAwaiter().GetResult();
        useCase.Cases["LegacyDataPreserved"] = result.Migration == PluginMigrationOutcome.AlreadyApplied
            && SqliteTableCatalog.Contains(anchor, "TinyTools_Packages") && SqliteTableCatalog.HasRows(anchor, "TinyTools_Packages")
            && SqliteTableCatalog.Contains(anchor, "TinyTools_Templates");
    }

    [Then("the existing TinyTools package data should be preserved")]
    [Trait("UAC", "33"), Fact]
    public void UAC27_PreservesLegacyPackageData() => ((bool)useCase.Cases["LegacyDataPreserved"]!).Should().BeTrue();
}
