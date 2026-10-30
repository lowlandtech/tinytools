namespace LowlandTech.TinyTools.Tests.SPEC3310.Packaging.US02.HelperRegistry;

[Trait(Spec.SPEC, "3310")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Helper behavior is the same for every caller in the process")]
public sealed class SC01_ExposesImmutableHelpers : IClassFixture<UseCases.HelperRegistryUseCase>
{
    private const string Template =
        "S: ${Context.Name | upper}; C: ${Context.Items | count}; J: ${Context.Items | join:-}; R: ${Context.Ratio | round:1}";

    private readonly UseCases.HelperRegistryUseCase useCase;

    public SC01_ExposesImmutableHelpers(UseCases.HelperRegistryUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("the declared helper surface, a registered service and concurrent renders")]
    private void Given()
    {
        useCase.Cases["PropertyType"] = typeof(TemplateHelpers)
            .GetProperty(nameof(TemplateHelpers.Helpers))!.PropertyType.Name;

        var service = new ExecutionContext();
        service.RegisterService("shout", input => input?.ToString()?.ToUpperInvariant() + "!");
        useCase.Cases["ServiceOutput"] = new TinyTemplateEngine().Render("${Context.Services('shout')('hi')}", service);

        var failures = 0;
        var outputs = new System.Collections.Concurrent.ConcurrentBag<string>();
        Parallel.For(0, 64, _ =>
        {
            var context = new ExecutionContext();
            context.Set("Name", "ada");
            context.Set("Items", new[] { "a", "b", "c" });
            context.Set("Ratio", 2.345);
            try
            {
                outputs.Add(new TinyTemplateEngine().Render(Template, context));
            }
            catch (Exception)
            {
                Interlocked.Increment(ref failures);
            }
        });

        // The round helper formats with the current culture, so the expectation follows it.
        useCase.Cases["Expected"] = $"S: ADA; C: 3; J: a-b-c; R: {2.345.ToString("F1")}";
        useCase.Cases["Failures"] = failures;
        useCase.Cases["DistinctOutputs"] = outputs.Distinct().Count();
        useCase.Cases["Sample"] = outputs.FirstOrDefault() ?? string.Empty;
    }

    [When("the helper registry and concurrent renders are observed once")]
    private void When()
    {
    }

    [Then("the helpers property type should be IReadOnlyDictionary`2")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ExposesOnlyAReadOnlySurface() => ((string)useCase.Cases["PropertyType"]!).Should().Be("IReadOnlyDictionary`2");

    [Then("the concurrent failures should be 0")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_HasNoConcurrentFailures() => ((int)useCase.Cases["Failures"]!).Should().Be(0);

    [Then("the concurrent outputs should have count 1")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ProducesOneStableOutput() => ((int)useCase.Cases["DistinctOutputs"]!).Should().Be(1);

    [Then("the sample render should be exact")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_KeepsEveryDocumentedHelperOutput() => useCase.Cases["Sample"].Should().Be(useCase.Cases["Expected"]);

    [Then("the service render should be exact")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ReachesTheRegisteredService() => ((string)useCase.Cases["ServiceOutput"]!).Should().Be("HI!");
}
