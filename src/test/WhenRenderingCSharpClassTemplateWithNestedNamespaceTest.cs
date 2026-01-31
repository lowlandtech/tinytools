using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests template with complex nested namespace.
/// </summary>
public class WhenRenderingCSharpClassTemplateWithNestedNamespace : WhenTestingFor<CSharpClassTemplate>
{
    private TemplateResult? _result;

    protected override CSharpClassTemplate For()
    {
        return new CSharpClassTemplate();
    }

    protected override void When()
    {
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

    [Fact]
    public void ItShouldGenerateNestedPath()
    {
        _result!.Path.Should().Be("src/App/Domain/Models/Entities/Product.cs");
    }

    [Fact]
    public void ItShouldUseFullNamespace()
    {
        _result!.Namespace.Should().Be("App.Domain.Models.Entities");
    }

    [Fact]
    public void ItShouldContainNamespaceInContent()
    {
        _result!.Content.Should().Contain("namespace App.Domain.Models.Entities;");
    }

    [Fact]
    public void ItShouldNotContainConstructor()
    {
        _result!.Content.Should().NotContain("public Product()");
    }

    [Fact]
    public void ItShouldContainPropertyWithDefaultValue()
    {
        _result!.Content.Should().Contain("public Guid ProductId { get; set; } = Guid.NewGuid();");
    }
}
