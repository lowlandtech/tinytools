using Humanizer;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Demonstrates how to use Template Services to extend TinyTemplateEngine.
/// Services are simple functions registered with string keys.
/// Usage: ${Context.Services("key")(input)}
/// </summary>
public class WhenUsingTemplateServicesTest : WhenTestingFor<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    protected override void Given()
    {
        _context = new ExecutionContext();
    }

    protected override void When()
    {
        // When is handled in each test
    }

    #region Pluralization/Singularization Tests

    [Fact]
    public void ItShouldPluralizeUsingHumanizerService()
    {
        // Arrange
        _context.RegisterService("pluralize", input => input?.ToString()?.Pluralize() ?? "");
        _context.Set("EntityName", "customer");
        var template = "Entities: ${Context.Services('pluralize')(Context.EntityName)}";

        // Act
        _result = Sut.Render(template, _context);

        // Assert
        _result.Should().Contain("Entities: customers");
    }

    [Fact]
    public void ItShouldSingularizeUsingHumanizerService()
    {
        // Arrange
        _context.RegisterService("singularize", input => input?.ToString()?.Singularize() ?? "");
        _context.Set("CollectionName", "categories");
        var template = "Entity: ${Context.Services('singularize')(Context.CollectionName)}";

        // Act
        _result = Sut.Render(template, _context);

        // Assert
        _result.Should().Contain("Entity: category");
    }

    [Fact]
    public void ItShouldHandleIrregularPlurals()
    {
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

    [Fact]
    public void ItShouldReturnErrorMessageWhenServiceNotFound()
    {
        // Arrange
        var template = "${Context.Services('nonexistent')('test')}";

        // Act
        _result = Sut.Render(template, _context);

        // Assert
        _result.Should().Contain("{nonexistent not registered}");
    }

    #endregion

    #region Calculator Service Tests (using NCalc)

    [Fact]
    public void ItShouldCalculateSimpleExpression()
    {
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

    [Fact]
    public void ItShouldCalculateTaxExample()
    {
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

    [Fact]
    public void ItShouldUseMultipleServicesInSameTemplate()
    {
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

    [Fact]
    public void ItShouldGenerateInvoiceWithServices()
    {
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
