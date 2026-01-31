using Xunit;
using LowlandTech.TinyTools;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.Tests;

public class TemplateSystemTests
{
    [Fact]
    public void ComponentTemplate_Validates_Successfully()
    {
        // Arrange
        var template = new ComponentTemplate();

        // Act
        var isValid = template.Validate();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ComponentTemplate_Renders_CorrectContent()
    {
        // Arrange
        var template = new ComponentTemplate();
        var data = new ComponentData
        {
            ComponentName = "Button",
            Props = new()
            {
                new PropDefinition { Name = "label", Type = "string" },
                new PropDefinition { Name = "onClick", Type = "() => void" }
            },
            PropsDestructured = "{ label, onClick }"
        };

        // Act
        var result = template.Render(data);

        // Assert
        Assert.Contains("export interface ButtonProps", result.Content);
        Assert.Contains("label: string", result.Content);
        Assert.Contains("onClick: () => void", result.Content);
        Assert.Contains("export const Button", result.Content);
    }

    [Fact]
    public void ComponentTemplate_Renders_CorrectPath()
    {
        // Arrange
        var template = new ComponentTemplate();
        var data = new ComponentData
        {
            ComponentName = "UserProfile",
            Props = new(),
            PropsDestructured = "{}"
        };

        // Act
        var result = template.Render(data);

        // Assert
        Assert.Equal("src/components/UserProfile.tsx", result.Path);
    }

    [Fact]
    public void CSharpClassTemplate_Validates_Successfully()
    {
        // Arrange
        var template = new CSharpClassTemplate();

        // Act
        var result = template.ValidateDetailed();

        // Assert
        Assert.True(result.IsValid, result.ErrorMessage);
    }

    [Fact]
    public void CSharpClassTemplate_Renders_WithProperties()
    {
        // Arrange
        var template = new CSharpClassTemplate();
        var data = new ClassData
        {
            Namespace = "MyApp.Models",
            ClassName = "Product",
            Description = "Represents a product",
            IncludeConstructor = true,
            Properties = new()
            {
                new PropertyData
                {
                    Name = "Id",
                    Type = "int",
                    Description = "Product identifier"
                },
                new PropertyData
                {
                    Name = "Name",
                    Type = "string",
                    Description = "Product name",
                    DefaultValue = "\"\""
                }
            },
            Methods = new()
            {
                new MethodData
                {
                    Name = "Validate",
                    ReturnType = "bool",
                    Parameters = "",
                    Description = "Validates the product"
                }
            }
        };

        // Act
        var result = template.Render(data);

        // Assert
        Assert.Contains("namespace MyApp.Models;", result.Content);
        Assert.Contains("public class Product", result.Content);
        Assert.Contains("public int Id { get; set; }", result.Content);
        Assert.Contains("public string Name { get; set; } = \"\";", result.Content);
        Assert.Contains("public Product()", result.Content);
        Assert.Contains("public bool Validate()", result.Content);
    }

    [Fact]
    public void TemplateRegistry_RegistersAndRetrievesTemplates()
    {
        // Arrange
        var registry = new TemplateRegistry();
        var template = new ComponentTemplate();

        // Act
        registry.Register("component", template);
        var retrieved = registry.Get("component");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Same(template, retrieved);
    }

    [Fact]
    public void TemplateRegistry_ValidatesAllTemplates()
    {
        // Arrange
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        registry.Register("class", new CSharpClassTemplate());

        // Act
        var results = registry.ValidateAll();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results.Values, r => Assert.True(r.IsValid));
    }

    [Fact]
    public void TemplateRegistry_RenderByName()
    {
        // Arrange
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        
        var data = new ComponentData
        {
            ComponentName = "TestComponent",
            Props = new(),
            PropsDestructured = "{}"
        };

        // Act
        var result = registry.Render("component", data);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("TestComponent", result.Content);
    }

    [Fact]
    public void TemplateRegistry_RenderBatch()
    {
        // Arrange
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        
        var batch = new Dictionary<string, object>
        {
            ["component1"] = new ComponentData 
            { 
                ComponentName = "Button",
                Props = new(),
                PropsDestructured = "{}"
            },
            ["component2"] = new ComponentData 
            { 
                ComponentName = "Input",
                Props = new(),
                PropsDestructured = "{}"
            }
        };

        // Act - note the key is template name, not unique id
        // So we need different approach for batch
        var results = new Dictionary<string, TemplateResult>();
        foreach (var (key, data) in batch)
        {
            var result = registry.Render("component", data);
            if (result != null)
                results[key] = result;
        }

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains("Button", results["component1"].Content);
        Assert.Contains("Input", results["component2"].Content);
    }

    [Fact]
    public void TemplateBase_NormalizesDataCorrectly()
    {
        // Arrange
        var template = new ComponentTemplate();
        var anonymousData = new 
        {
            ComponentName = "Test",
            Props = new object[] { },
            PropsDestructured = "{}"
        };

        // Act
        var result = template.Render(anonymousData);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Test", result.Content);
    }

    [Fact]
    public void TemplateValidation_DetectsMismatch()
    {
        // Arrange - create a template that will fail validation
        var template = new TestFailingTemplate();

        // Act
        var result = template.ValidateDetailed();

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Differences);
        Assert.Contains("Content", result.Differences.Keys);
    }

    [Fact]
    public void Template_HandlesComplexNestedData()
    {
        // Arrange
        var template = new CSharpClassTemplate();
        var data = new ClassData
        {
            Namespace = "App.Models.Domain",
            ClassName = "ComplexEntity",
            Description = "A complex entity",
            Properties = new()
            {
                new PropertyData { Name = "Prop1", Type = "string", Description = "First" },
                new PropertyData { Name = "Prop2", Type = "int", Description = "Second" },
                new PropertyData { Name = "Prop3", Type = "bool", Description = "Third" }
            }
        };

        // Act
        var result = template.Render(data);

        // Assert
        Assert.Equal("src/App/Models/Domain/ComplexEntity.cs", result.Path);
        Assert.Equal("App.Models.Domain", result.Namespace);
        Assert.Contains("namespace App.Models.Domain;", result.Content);
    }
}

/// <summary>
/// Test template that deliberately fails validation for testing purposes.
/// </summary>
internal class TestFailingTemplate : TemplateBase
{
    public override string TemplatePath => "test.txt";
    public override string TemplateNamespace => "Test";
    public override string TemplateContent => "Wrong content: ${Context.Value}";
    public override Type DataType => typeof(TestData);
    
    public override string TestDataJson => @"{ ""Value"": ""Test"" }";
    public override string ExpectedContent => "This will not match!";
}

internal record TestData
{
    public string Value { get; init; } = "";
}
