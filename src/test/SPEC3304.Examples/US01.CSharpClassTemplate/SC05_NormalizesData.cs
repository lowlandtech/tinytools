namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US01.CSharpClassTemplate;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "05")]
[UserStory("01", "CSharp class template generates class code")]
public sealed class SC05_NormalizesData : UnitScenario, IClassFixture<UseCases.CSharpClassTemplateUseCase>
{
    private readonly UseCases.CSharpClassTemplateUseCase useCase;
    private readonly ComponentTemplate _component = new();
    private object _anonymousData = null!;

    public SC05_NormalizesData(UseCases.CSharpClassTemplateUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("anonymous data shaped like the component data")]
    protected override void Given() => _anonymousData = new
    {
        ComponentName = "TestComponent",
        Props = new object[]
        {
            new { Name = "prop1", Type = "string" }
        },
        PropsDestructured = "{ prop1 }"
    };

    [When("the component template renders the anonymous data")]
    protected override void When() => useCase.Cases["Result"] = _component.Render(_anonymousData);

    [Then("the result should not be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_NormalizesTheAnonymousData() => useCase.Cases["Result"].Should().NotBeNull();

    [Then("the content should contain the normalized component and prop")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersWithTheNormalizedData() => ((TemplateResult)useCase.Cases["Result"]!).Content.Should().Contain("TestComponent").And.Contain("prop1");

    [Then("the generated path should be src/components/TestComponent.tsx")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_GeneratesTheComponentPath() => ((TemplateResult)useCase.Cases["Result"]!).Path.Should().Be("src/components/TestComponent.tsx");
}
