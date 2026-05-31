namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US01.CSharpClassTemplate;

/// <summary>
/// Tests template with complex nested namespace.
/// </summary>
[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "03")]
[UserStory("01", "CSharp class template generates class code")]
public class WhenRenderingCSharpClassTemplateWithNestedNamespace : TinyToolsScenario<Shared.Examples.CSharpClassTemplate>
{
    private TemplateResult? _result;

    protected override Shared.Examples.CSharpClassTemplate For()
    {
        return new Shared.Examples.CSharpClassTemplate();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        var data = new ClassData
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
        _result = Sut.Render(data);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Generate Nested Path")]
    [Fact]
    public void ItShouldGenerateNestedPath()
    {
        ArrangeAndAct();
        _result!.Path.Should().Be("src/App/Domain/Models/Entities/Product.cs");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Use Full Namespace")]
    [Fact]
    public void ItShouldUseFullNamespace()
    {
        ArrangeAndAct();
        _result!.Namespace.Should().Be("App.Domain.Models.Entities");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Contain Namespace In Content")]
    [Fact]
    public void ItShouldContainNamespaceInContent()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("namespace App.Domain.Models.Entities;");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Not Contain Constructor")]
    [Fact]
    public void ItShouldNotContainConstructor()
    {
        ArrangeAndAct();
        _result!.Content.Should().NotContain("public Product()");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Contain Property With Default Value")]
    [Fact]
    public void ItShouldContainPropertyWithDefaultValue()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("public Guid ProductId { get; set; } = Guid.NewGuid();");
    }
}
