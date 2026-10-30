namespace LowlandTech.TinyTools.Tests.SPEC3327.MsBuild.US01.Targets;

[Trait(Spec.SPEC, "3327")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Templates render during the build, incrementally")]
public sealed class SC01_RendersDuringBuild : IClassFixture<UseCases.MsBuildUseCase>
{
    private readonly UseCases.MsBuildUseCase useCase;

    public SC01_RendersDuringBuild(UseCases.MsBuildUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    private string workspace = string.Empty;
    private string consumer = string.Empty;
    private string packageFeed = string.Empty;

    [Given("a packaged TinyTools build integration and a consumer project declaring a template")]
    private void Given()
    {
        workspace = Path.Combine(Path.GetTempPath(), "tinytools-msbuild-" + Guid.NewGuid().ToString("N"));
        consumer = Path.Combine(workspace, "Consumer");
        packageFeed = Path.Combine(workspace, "packages");
        Directory.CreateDirectory(consumer);
        Directory.CreateDirectory(packageFeed);

        var root = UseCases.MsBuildUseCase.RepositoryRoot();
        var pack = UseCases.MsBuildUseCase.Run(
            "dotnet",
            ["pack", Path.Combine(root, "src", "lib", "LowlandTech.TinyTools.csproj"), "-c", "Release", "-o", packageFeed],
            root);
        useCase.Cases["PackExit"] = pack.Exit;
        File.WriteAllText(Path.Combine(consumer, "greeting.tmpl"), "Hello ${Context.Model.Name}");
        File.WriteAllText(Path.Combine(consumer, "model.json"), "{\"Name\":\"Ada\"}");
        File.WriteAllText(Path.Combine(consumer, "Consumer.csproj"), """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <RestorePackagesPath>$(MSBuildThisFileDirectory)obj/packages</RestorePackagesPath>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="LowlandTech.TinyTools" Version="2026.10.4" />
              </ItemGroup>
              <ItemGroup>
                <TinyTemplate Include="greeting.tmpl" Model="model.json" TinyToolsOutput="$(IntermediateOutputPath)generated/greeting.txt" />
              </ItemGroup>
            </Project>
            """);
        File.WriteAllText(Path.Combine(consumer, "nuget.config"), $"""
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
                <add key="local" value="{packageFeed}" />
                <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
              </packageSources>
            </configuration>
            """);
    }
    [When("the consumer builds twice and then builds a broken template")]
    private void When()
    {
        useCase.Cases["First"] = Build();
        var output = Path.Combine(consumer, "obj", "Debug", "net10.0", "generated", "greeting.txt");
        useCase.Cases["OutputExists"] = File.Exists(output);
        useCase.Cases["OutputText"] = File.Exists(output) ? File.ReadAllText(output) : "missing";
        var firstWrite = File.Exists(output) ? File.GetLastWriteTimeUtc(output) : DateTime.MinValue;
        Thread.Sleep(1100);
        useCase.Cases["Second"] = Build();
        useCase.Cases["Skipped"] = File.Exists(output) && File.GetLastWriteTimeUtc(output) == firstWrite;
        File.WriteAllText(Path.Combine(consumer, "greeting.tmpl"), "@else");
        useCase.Cases["Broken"] = Build();
    }
    private (int Exit, string Output) Build() =>
        UseCases.MsBuildUseCase.Run("dotnet", ["build", Path.Combine(consumer, "Consumer.csproj"), "-c", "Debug"], consumer);
    [Then("the first exit should be 0")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_FirstBuildSucceeds() => ((int)((ValueTuple<int, string>)useCase.Cases["First"]!).Item1).Should().Be(0);
    [Then("the output existence should be true")]
    [Trait("UAC", "02"), Fact]
    public void UAC01b_WritesTheOutput() => ((bool)useCase.Cases["OutputExists"]!).Should().BeTrue();
    [Then("the output text should be exact")]
    [Trait("UAC", "03"), Fact]
    public void UAC01c_RendersTheTemplate() => ((string)useCase.Cases["OutputText"]!).Should().Be("Hello Ada");
    [Then("the rebuild exit should be 0")]
    [Trait("UAC", "04"), Fact]
    public void UAC02_RebuildSucceeds() => ((int)((ValueTuple<int, string>)useCase.Cases["Second"]!).Item1).Should().Be(0);
    [Then("the skip flag should be true")]
    [Trait("UAC", "05"), Fact]
    public void UAC02b_SkipsUnchangedInputs() => ((bool)useCase.Cases["Skipped"]!).Should().BeTrue();
    [Then("the broken exit should be refused")]
    [Trait("UAC", "06"), Fact]
    public void UAC03_FailsOnABrokenTemplate() => ((int)((ValueTuple<int, string>)useCase.Cases["Broken"]!).Item1).Should().NotBe(0);
    [Then("the broken output should contain TS007")]
    [Trait("UAC", "07"), Fact]
    public void UAC03b_ReportsTheDiagnostic() => ((string)((ValueTuple<int, string>)useCase.Cases["Broken"]!).Item2).Should().Contain("TS007");
}