using LowlandTech.TinyTools.Core.Entities;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US01.EntityModel;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "04")]
[UserStory("01", "The Tinytools server shares one template entity model")]
public sealed class SC04_AvoidsHostFrameworkReferences
{
    private List<string> _hostReferences = [];

    [Given("the Tinytools core assembly")]
    private void Given()
    {
    }

    [When("its referenced assemblies are read")]
    private void When() => _hostReferences = typeof(Template).Assembly.GetReferencedAssemblies()
        .Select(name => name.Name ?? string.Empty)
        .Where(name => name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal)
            || name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal))
        .ToList();

    public SC04_AvoidsHostFrameworkReferences()
    {
        Given();
        When();
    }

    [Then("the host references should be empty")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_NoHostFramework() => _hostReferences.Should().BeEmpty();
}
