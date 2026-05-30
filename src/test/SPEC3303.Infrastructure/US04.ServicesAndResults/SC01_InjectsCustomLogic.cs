using Humanizer;

namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US04.ServicesAndResults;

/// <summary>
/// Demonstrates how to use Template Services to extend TinyTemplateEngine.
/// Services are simple functions registered with string keys.
/// Usage: ${Context.Services("key")(input)}
/// </summary>
[Trait(Spec.SPEC, "3303")]
[Trait(Spec.SC, "01")]
[UserStory("04", "Template services inject custom logic")]
public class WhenUsingTemplateServicesTest : TinyToolsScenario<TinyTemplateEngine>
{
    private ToolContext _context = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _context = new ToolContext();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        // When is handled in each test
    }

    #region Pluralization/Singularization Tests

    [Trait(Spec.UAC, "01")]
    [Then("it Should Pluralize Using Humanizer Service")]
    [Fact]
    public void ItShouldPluralizeUsingHumanizerService()
    {
        ArrangeAndAct();
        // Arrange
        _context.RegisterService("pluralize", input => input?.ToString()?.Pluralize() ?? "");
        _context.Set("EntityName", "customer");
        var template = "Entities: ${Context.Services('pluralize')(Context.EntityName)}";

        // Act
        _result = Sut.Render(template, _context);

        // Assert
        _result.Should().Contain("Entities: customers");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Singularize Using Humanizer Service")]
    [Fact]
    public void ItShouldSingularizeUsingHumanizerService()
    {
        ArrangeAndAct();
        // Arrange
        _context.RegisterService("singularize", input => input?.ToString()?.Singularize() ?? "");
        _context.Set("CollectionName", "categories");
        var template = "Entity: ${Context.Services('singularize')(Context.CollectionName)}";

        // Act
        _result = Sut.Render(template, _context);

        // Assert
        _result.Should().Contain("Entity: category");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Handle Irregular Plurals")]
    [Fact]
    public void ItShouldHandleIrregularPlurals()
    {
        ArrangeAndAct();
        // Arrange
        _context.RegisterService("pluralize", input => input?.ToString()?.Pluralize() ?? "");
        var template = "${Context.Services('pluralize')('person')}, ${Context.Services('pluralize')('child')}, ${Context.Services('pluralize')('goose')}";

        // Act
        _result = Sut.Render(template, _context);

        // Assert
        _result.Should().Contain("people");
        _result.Should().Contain("children");
        _result.Should().Contain("geese");
    }

    #endregion

    #region Service Not Found Tests

    [Trait(Spec.UAC, "04")]
    [Then("it Should Return Error Message When Service Not Found")]
    [Fact]
    public void ItShouldReturnErrorMessageWhenServiceNotFound()
    {
        ArrangeAndAct();
        // Arrange
        var template = "${Context.Services('nonexistent')('test')}";

        // Act
        _result = Sut.Render(template, _context);

        // Assert
        _result.Should().Contain("{nonexistent not registered}");
    }

    #endregion

    #region Calculator Service Tests (using NCalc)

    [Trait(Spec.UAC, "05")]
    [Then("it Should Calculate Simple Expression")]
    [Fact]
    public void ItShouldCalculateSimpleExpression()
    {
        ArrangeAndAct();
        // Arrange
        _context.RegisterService("calc", input =>
        {
            var expr = new NCalc.Expression(input?.ToString() ?? "0");
            return expr.Evaluate();
        });
        var template = "Result: ${Context.Services('calc')('2 + 2')}";

        // Act
        _result = Sut.Render(template, _context);

        // Assert
        _result.Should().Contain("Result: 4");
    }

    [Trait(Spec.UAC, "06")]
    [Then("it Should Calculate Tax Example")]
    [Fact]
    public void ItShouldCalculateTaxExample()
    {
        ArrangeAndAct();
        // Arrange
        _context.RegisterService("calc", input =>
        {
            var expr = new NCalc.Expression(input?.ToString() ?? "0");
            return expr.Evaluate();
        });
        var template = "Tax: $${Context.Services('calc')('100 * 0.15')}";

        // Act
        _result = Sut.Render(template, _context);

        // Assert
        _result.Should().Contain("Tax: $15");
    }

    #endregion

    #region Multiple Services

    [Trait(Spec.UAC, "07")]
    [Then("it Should Use Multiple Services In Same Template")]
    [Fact]
    public void ItShouldUseMultipleServicesInSameTemplate()
    {
        ArrangeAndAct();
        // Arrange
        _context.RegisterService("pluralize", input => input?.ToString()?.Pluralize() ?? "");
        _context.RegisterService("calc", input =>
        {
            var expr = new NCalc.Expression(input?.ToString() ?? "0");
            return expr.Evaluate();
        });
        
        var template = "We have ${Context.Services('calc')('5 + 3')} ${Context.Services('pluralize')('order')}.";

        // Act
        _result = Sut.Render(template, _context);

        // Assert
        _result.Should().Contain("We have 8 orders.");
    }

    #endregion

    #region Real-World Example

    [Trait(Spec.UAC, "08")]
    [Then("it Should Generate Invoice With Services")]
    [Fact]
    public void ItShouldGenerateInvoiceWithServices()
    {
        ArrangeAndAct();
        // Arrange
        _context.RegisterService("pluralize", input => input?.ToString()?.Pluralize() ?? "");
        _context.RegisterService("calc", input =>
        {
            var expr = new NCalc.Expression(input?.ToString() ?? "0");
            var result = expr.Evaluate();
            if (result is double d)
                return d.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            return result;
        });
        
        var template = """
            Invoice Summary
            ---------------
            Items: ${Context.Services('calc')('5')} ${Context.Services('pluralize')('widget')}
            Subtotal: $${Context.Services('calc')('19.99 * 5')}
            Tax: $${Context.Services('calc')('19.99 * 5 * 0.08')}
            Total: $${Context.Services('calc')('19.99 * 5 * 1.08')}
            """;

        // Act
        _result = Sut.Render(template, _context);

        // Assert
        _result.Should().Contain("Items: 5 widgets");
        _result.Should().Contain("Subtotal: $99.95");
        _result.Should().Contain("Tax: $8.00");
        _result.Should().Contain("Total: $107.95");
    }

    #endregion
}
