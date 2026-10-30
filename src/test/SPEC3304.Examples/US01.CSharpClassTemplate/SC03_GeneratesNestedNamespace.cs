namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US01.CSharpClassTemplate;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "03")]
[UserStory("01", "CSharp class template generates class code")]
public sealed class SC03_GeneratesNestedNamespace : UnitScenario, IClassFixture<UseCases.CSharpClassTemplateUseCase>
{
    private readonly UseCases.CSharpClassTemplateUseCase useCase;
    private readonly Shared.Examples.CSharpClassTemplate _template = new();
    private ClassData _data = null!;

    public SC03_GeneratesNestedNamespace(UseCases.CSharpClassTemplateUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a product entity definition in a nested namespace")]
    protected override void Given() => _data = new ClassData
    {
        Namespace = "App.Domain.Models.Entities",
        ClassName = "Product",
        Description = "Represents a product entity",
        IncludeConstructor = false,
        Properties = new()
        {
            new PropertyData
            {
                Name = "ProductId",
                Type = "Guid",
                Description = "Product unique identifier",
                DefaultValue = "Guid.NewGuid()"
            }
        },
        Methods = new()
    };

    [When("the class template renders the definition")]
    protected override void When() => useCase.Cases["Result"] = _template.Render(_data);

    [Then("the generated path should be src/App/Domain/Models/Entities/Product.cs")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_GeneratesTheNestedPath() => ((TemplateResult)useCase.Cases["Result"]!).Path.Should().Be("src/App/Domain/Models/Entities/Product.cs");

    [Then("the namespace should be App.Domain.Models.Entities")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_UsesTheFullNamespace() => ((TemplateResult)useCase.Cases["Result"]!).Namespace.Should().Be("App.Domain.Models.Entities");

    [Then("the content should contain the namespace declaration")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ContainsTheNamespaceInContent() => ((TemplateResult)useCase.Cases["Result"]!).Content.Should().Contain("namespace App.Domain.Models.Entities;");

    [Then("the content should not contain a constructor")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ContainsNoConstructor() => ((TemplateResult)useCase.Cases["Result"]!).Content.Should().NotContain("public Product()");

    [Then("the content should contain the defaulted product id property")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ContainsThePropertyWithDefaultValue() => ((TemplateResult)useCase.Cases["Result"]!).Content.Should().Contain("public Guid ProductId { get; set; } = Guid.NewGuid();");
}
