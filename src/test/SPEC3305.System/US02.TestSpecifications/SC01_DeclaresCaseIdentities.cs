using System.Reflection;

namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US02.TestSpecifications;

[Trait(Spec.SPEC, "3305")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Test specifications expose a story goal and named case identities")]
[Story(typeof(TestSpecificationStory))]
public class WhenCheckingTestSpecificationLayout
{
    private TestSpecificationResult _result = null!;

    [Given("the test assembly is inspected for story, case and acceptance identities")]
    private void Given()
    {
        var assembly = typeof(WhenCheckingTestSpecificationLayout).Assembly;
        var scenarios = SpecLayout.Scenarios(assembly);

        _result = new TestSpecificationResult(
            SpecLayout.Violations(assembly),
            SpecLayout.Migrated(assembly),
            SpecLayout.Unmigrated(assembly),
            scenarios);
    }

    [When("the layout check runs once over the assembly")]
    private void When()
    {
        Given();
    }

    [Case<TestSpecificationCase>(TestSpecificationCase.MigratedScenariosDeclareCases)]
    [Trait(Spec.UAC, "02")]
    [Then("it Should Declare One Known Case Per Migrated Fact")]
    [Fact]
    public void ItShouldDeclareOneKnownCasePerMigratedFact()
    {
        When();
        _result.Violations.Should().BeEmpty();
    }

    [Case<TestSpecificationCase>(TestSpecificationCase.InventoryCountsReconcile)]
    [Trait(Spec.UAC, "01")]
    [Then("it Should Reconcile Migrated And Unmigrated Counts")]
    [Fact]
    public void ItShouldReconcileMigratedAndUnmigratedCounts()
    {
        When();
        _result.Migrated.Should().NotBeEmpty();
        _result.InventoryReconciles.Should().BeTrue();
    }
}
