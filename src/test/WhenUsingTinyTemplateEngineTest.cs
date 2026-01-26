namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Comprehensive tests for TinyTemplateEngine features.
/// Covers all public and internal functionality.
/// </summary>
public class WhenUsingTinyTemplateEngineTest : WhenTestingFor<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;

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

    #region Basic Rendering Tests

    [Fact]
    public void ItShouldRenderEmptyTemplate()
    {
        // Arrange
        var template = "";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("");
    }

    [Fact]
    public void ItShouldRenderNullTemplate()
    {
        // Arrange
        string? template = null;

        // Act
        var result = Sut.Render(template!, _context);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ItShouldRenderTemplateWithoutVariables()
    {
        // Arrange
        var template = "Hello, World!";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("Hello, World!");
    }

    [Fact]
    public void ItShouldRenderTemplateWithSimpleVariable()
    {
        // Arrange
        _context.Set("Name", "John");
        var template = "Hello, ${Context.Name}!";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("Hello, John!");
    }

    [Fact]
    public void ItShouldRenderTemplateWithMultipleVariables()
    {
        // Arrange
        _context.Set("FirstName", "John");
        _context.Set("LastName", "Doe");
        _context.Set("Age", 30);
        var template = "${Context.FirstName} ${Context.LastName} is ${Context.Age} years old.";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("John Doe is 30 years old.");
    }

    #endregion

    #region Comment Removal Tests

    [Fact]
    public void ItShouldRemoveSingleLineComment()
    {
        // Arrange
        var template = "Before @* comment *@ After";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("Before  After");
    }

    [Fact]
    public void ItShouldRemoveMultiLineComment()
    {
        // Arrange
        var template = @"Before
@* 
This is a
multi-line comment
*@
After";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Before");
        result.Should().Contain("After");
        result.Should().NotContain("multi-line comment");
    }

    [Fact]
    public void ItShouldRemoveMultipleComments()
    {
        // Arrange
        var template = "A @* comment1 *@ B @* comment2 *@ C";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("A  B  C");
    }

    #endregion

    #region If Statement Tests

    [Fact]
    public void ItShouldRenderIfBlockWhenConditionIsTrue()
    {
        // Arrange
        _context.Set("IsActive", true);
        var template = @"@if(Context.IsActive) {
Active
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Active");
    }

    [Fact]
    public void ItShouldNotRenderIfBlockWhenConditionIsFalse()
    {
        // Arrange
        _context.Set("IsActive", false);
        var template = @"@if(Context.IsActive) {
Active
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().NotContain("Active");
    }

    [Fact]
    public void ItShouldRenderElseBlockWhenConditionIsFalse()
    {
        // Arrange
        _context.Set("IsActive", false);
        var template = @"@if(Context.IsActive) {
Active
} else {
Inactive
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Inactive");
        result.Should().NotContain("Active");
    }

    [Fact]
    public void ItShouldRenderElseIfBlockWhenConditionMatches()
    {
        // Arrange
        _context.Set("Status", "pending");
        var template = @"@if(Context.Status == ""active"") {
Active
} else if (Context.Status == ""pending"") {
Pending
} else {
Unknown
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Pending");
        result.Should().NotContain("Active");
        result.Should().NotContain("Unknown");
    }

    [Fact]
    public void ItShouldHandleNegationOperator()
    {
        // Arrange
        _context.Set("IsActive", false);
        var template = @"@if(!Context.IsActive) {
Not Active
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Not Active");
    }

    [Fact]
    public void ItShouldHandleGreaterThanComparison()
    {
        // Arrange
        _context.Set("Count", 10);
        var template = @"@if(Context.Count > 5) {
Greater
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Greater");
    }

    [Fact]
    public void ItShouldHandleGreaterThanOrEqualComparison()
    {
        // Arrange
        _context.Set("Count", 5);
        var template = @"@if(Context.Count >= 5) {
Greater or Equal
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Greater or Equal");
    }

    [Fact]
    public void ItShouldHandleLessThanComparison()
    {
        // Arrange
        _context.Set("Count", 3);
        var template = @"@if(Context.Count < 5) {
Less
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Less");
    }

    [Fact]
    public void ItShouldHandleLessThanOrEqualComparison()
    {
        // Arrange
        _context.Set("Count", 5);
        var template = @"@if(Context.Count <= 5) {
Less or Equal
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Less or Equal");
    }

    [Fact]
    public void ItShouldHandleEqualityComparison()
    {
        // Arrange
        _context.Set("Status", "active");
        var template = @"@if(Context.Status == ""active"") {
Equal
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Equal");
    }

    [Fact]
    public void ItShouldHandleInequalityComparison()
    {
        // Arrange
        _context.Set("Status", "inactive");
        var template = @"@if(Context.Status != ""active"") {
Not Equal
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Not Equal");
    }

    [Fact]
    public void ItShouldHandleTruthyCheckForString()
    {
        // Arrange
        _context.Set("Name", "John");
        var template = @"@if(Context.Name) {
Has Name
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Has Name");
    }

    [Fact]
    public void ItShouldHandleTruthyCheckForEmptyString()
    {
        // Arrange
        _context.Set("Name", "");
        var template = @"@if(Context.Name) {
Has Name
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().NotContain("Has Name");
    }

    [Fact]
    public void ItShouldHandleTruthyCheckForNull()
    {
        // Arrange
        _context.Set("Name", null);
        var template = @"@if(Context.Name) {
Has Name
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().NotContain("Has Name");
    }

    [Fact]
    public void ItShouldHandleTruthyCheckForNumber()
    {
        // Arrange
        _context.Set("Count", 0);
        var template = @"@if(Context.Count) {
Has Count
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().NotContain("Has Count");
    }

    [Fact]
    public void ItShouldHandleTruthyCheckForCollection()
    {
        // Arrange
        _context.Set("Items", new List<string> { "a", "b" });
        var template = @"@if(Context.Items) {
Has Items
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Has Items");
    }

    #endregion

    #region Foreach Loop Tests

    [Fact]
    public void ItShouldIterateOverCollection()
    {
        // Arrange
        _context.Set("Items", new[] { "A", "B", "C" });
        var template = @"@foreach(var item in Context.Items) {
${item}
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("A");
        result.Should().Contain("B");
        result.Should().Contain("C");
    }

    [Fact]
    public void ItShouldHandleEmptyCollection()
    {
        // Arrange
        _context.Set("Items", new string[] { });
        var template = @"@foreach(var item in Context.Items) {
${item}
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ItShouldHandleNullCollection()
    {
        // Arrange
        _context.Set("Items", null);
        var template = @"@foreach(var item in Context.Items) {
${item}
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ItShouldAccessItemPropertiesInForeach()
    {
        // Arrange
        var items = new[]
        {
            new { Name = "John", Age = 30 },
            new { Name = "Jane", Age = 25 }
        };
        _context.Set("People", items);
        var template = @"@foreach(var person in Context.People) {
${person.Name} is ${person.Age}
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("John is 30");
        result.Should().Contain("Jane is 25");
    }

    #endregion

    #region Nested Control Flow Tests

    [Fact]
    public void ItShouldHandleNestedIfStatements()
    {
        // Arrange
        _context.Set("IsActive", true);
        _context.Set("IsPremium", true);
        var template = @"@if(Context.IsActive) {
@if(Context.IsPremium) {
Premium Active User
}
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Premium Active User");
    }

    [Fact]
    public void ItShouldHandleIfInsideForeach()
    {
        // Arrange
        _context.Set("Numbers", new[] { 1, 2, 3, 4, 5 });
        var template = @"@foreach(var num in Context.Numbers) {
@if(num > 3) {
${num} is greater than 3
}
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("4 is greater than 3");
        result.Should().Contain("5 is greater than 3");
        result.Should().NotContain("1 is greater");
    }

    [Fact]
    public void ItShouldHandleForeachInsideIf()
    {
        // Arrange
        _context.Set("HasItems", true);
        _context.Set("Items", new[] { "A", "B", "C" });
        var template = @"@if(Context.HasItems) {
@foreach(var item in Context.Items) {
Item: ${item}
}
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Item: A");
        result.Should().Contain("Item: B");
        result.Should().Contain("Item: C");
    }

    #endregion

    #region ResolveVariables Public Method Tests

    [Fact]
    public void ItShouldResolveVariablesDirectly()
    {
        // Arrange
        _context.Set("Name", "John");
        var input = "Hello, ${Context.Name}!";

        // Act
        var result = Sut.ResolveVariables(input, _context);

        // Assert
        result.Should().Be("Hello, John!");
    }

    [Fact]
    public void ItShouldResolveMultipleVariables()
    {
        // Arrange
        _context.Set("First", "John");
        _context.Set("Last", "Doe");
        var input = "${Context.First} ${Context.Last}";

        // Act
        var result = Sut.ResolveVariables(input, _context);

        // Assert
        result.Should().Be("John Doe");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ItShouldHandleTemplateWithOnlyWhitespace()
    {
        // Arrange
        var template = "   \n\t  ";

        // Act
        var result = Sut.Render(template, _context);

        // Assert - Should preserve whitespace (platform-specific line endings may differ)
        result.Should().NotBeNull();
        result.Should().Contain("   ");
        result.Should().Contain("\t");
    }

    [Fact]
    public void ItShouldHandleUnmatchedBraces()
    {
        // Arrange
        var template = "@if(Context.Test) {\nNo closing brace";

        // Act
        var result = Sut.Render(template, _context);

        // Assert - Should not throw, might have unpredictable output
        result.Should().NotBeNull();
    }

    [Fact]
    public void ItShouldHandleIntegerComparison()
    {
        // Arrange
        _context.Set("IntValue", 10);
        var template = @"@if(Context.IntValue > 5) {
Int Greater
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Int Greater");
    }

    [Fact]
    public void ItShouldHandleDoubleComparison()
    {
        // Arrange
        _context.Set("DoubleValue", 10.5);
        var template = @"@if(Context.DoubleValue >= 10.5) {
Double Greater or Equal
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Double Greater or Equal");
    }

    [Fact]
    public void ItShouldHandleStringComparisons()
    {
        // Arrange
        _context.Set("Name", "Alice");
        var template = @"@if(Context.Name == ""Alice"") {
Name is Alice
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Name is Alice");
    }

    [Fact]
    public void ItShouldBeCaseInsensitiveForStringComparisons()
    {
        // Arrange
        _context.Set("Name", "alice");
        var template = @"@if(Context.Name == ""ALICE"") {
Match
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Match");
    }

    [Fact]
    public void ItShouldHandleNullComparisons()
    {
        // Arrange
        _context.Set("Value", null);
        var template = @"@if(Context.Value == null) {
Is Null
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Is Null");
    }

    [Fact]
    public void ItShouldHandleBooleanComparisons()
    {
        // Arrange
        _context.Set("IsActive", true);
        var template = @"@if(Context.IsActive == true) {
Is True
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Is True");
    }

    #endregion

    #region Complex Integration Tests

    [Fact]
    public void ItShouldHandleComplexTemplateWithAllFeatures()
    {
        // Arrange
        var customers = new[]
        {
            new { Name = "John", Age = 30, IsActive = true },
            new { Name = "Jane", Age = 25, IsActive = false },
            new { Name = "Bob", Age = 35, IsActive = true }
        };
        _context.Set("Customers", customers);
        _context.Set("Title", "Customer Report");

        var template = @"${Context.Title}
=================

@* Loop through all customers *@
@foreach(var customer in Context.Customers) {
Customer: ${customer.Name}
Age: ${customer.Age}
@if(customer.IsActive) {
Status: ACTIVE
} else {
Status: INACTIVE
}
@if(customer.Age >= 30) {
Category: Senior
} else {
Category: Junior
}
---
}

End of Report";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Customer Report");
        result.Should().Contain("Customer: John");
        result.Should().Contain("Customer: Jane");
        result.Should().Contain("Customer: Bob");
        result.Should().Contain("Status: ACTIVE");
        result.Should().Contain("Status: INACTIVE");
        result.Should().Contain("Category: Senior");
        result.Should().Contain("Category: Junior");
        result.Should().Contain("End of Report");
    }

    [Fact]
    public void ItShouldHandleTemplateWithVariablesAndHelpers()
    {
        // Arrange
        _context.Set("Name", "john doe");
        var template = "Hello, ${Context.Name | upper}!";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("Hello, JOHN DOE!");
    }

    [Fact]
    public void ItShouldHandleForeachWithConditionalRendering()
    {
        // Arrange
        var products = new[]
        {
            new { Name = "Product A", Price = 10, InStock = true },
            new { Name = "Product B", Price = 20, InStock = false },
            new { Name = "Product C", Price = 15, InStock = true }
        };
        _context.Set("Products", products);

        var template = @"Available Products:
@foreach(var product in Context.Products) {
@if(product.InStock) {
- ${product.Name}: $${product.Price}
}
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Product A: $10");
        result.Should().Contain("Product C: $15");
        result.Should().NotContain("Product B");
    }

    #endregion

    #region Negation Operator Tests

    [Fact]
    public void ItShouldHandleSimpleNegation()
    {
        // Arrange
        _context.Set("IsActive", false);
        var template = @"@if(!Context.IsActive) {
Inactive
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Inactive");
    }

    [Fact]
    public void ItShouldHandleNegationWithoutParentheses()
    {
        // Arrange - Test that !Context.IsActive works (without extra parentheses)
        _context.Set("IsActive", false);
        var template = @"@if (!Context.IsActive) {
Not Active
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert - IsActive is false, so !IsActive is true
        result.Should().Contain("Not Active");
    }

    [Fact]
    public void ItShouldHandleNegationOfTruthyValue()
    {
        // Arrange
        _context.Set("Count", 5);
        var template = @"@if(!Context.Count) {
No Count
} else {
Has Count
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Has Count");
        result.Should().NotContain("No Count");
    }

    [Fact]
    public void ItShouldHandleNegationOfZero()
    {
        // Arrange
        _context.Set("Count", 0);
        var template = @"@if(!Context.Count) {
Zero is falsy
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Zero is falsy");
    }

    [Fact]
    public void ItShouldHandleNegationOfEmptyString()
    {
        // Arrange
        _context.Set("Name", "");
        var template = @"@if(!Context.Name) {
Empty Name
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Empty Name");
    }

    [Fact]
    public void ItShouldHandleNegationOfNull()
    {
        // Arrange - NotSet means null
        var template = @"@if(!Context.Missing) {
Missing is falsy
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Missing is falsy");
    }

    #endregion

    #region Comparison Operator Edge Cases

    [Fact]
    public void ItShouldHandleNumericComparisonWithDoubles()
    {
        // Arrange
        _context.Set("Value", 3.14);
        var template = @"@if (Context.Value > 3) {
Greater
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Greater");
    }

    [Fact]
    public void ItShouldHandleNumericComparisonWithDecimals()
    {
        // Arrange
        _context.Set("Value", 99.99m);
        var template = @"@if (Context.Value >= 100) {
Expensive
} else {
Affordable
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Affordable");
    }

    [Fact]
    public void ItShouldHandleComparisonWithBooleanLiteral()
    {
        // Arrange
        _context.Set("Flag", true);
        var template = @"@if (Context.Flag == true) {
Is True
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Is True");
    }

    [Fact]
    public void ItShouldHandleComparisonWithFalseBooleanLiteral()
    {
        // Arrange
        _context.Set("Flag", false);
        var template = @"@if (Context.Flag == false) {
Is False
}";

        // Act
        var result = Sut.Render(template, _context);


        // Assert
        result.Should().Contain("Is False");
    }

    [Fact]
    public void ItShouldHandleLessThanComparisonForAge()
    {
        // Arrange
        _context.Set("Age", 17);
        var template = @"@if (Context.Age < 18) {
Minor
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Minor");
    }



    [Fact]
    public void ItShouldHandleNotNullComparison()
    {
        // Arrange
        _context.Set("Value", "something");
        var template = @"@if (Context.Value != null) {
Has Value
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Has Value");
    }

    #endregion

    #region IsTruthy Edge Cases

    [Fact]
    public void ItShouldTreatNonZeroIntAsTruthy()
    {
        // Arrange
        _context.Set("Value", -1);
        var template = @"@if (Context.Value) {
Truthy
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Truthy");
    }

    [Fact]
    public void ItShouldTreatEmptyCollectionAsFalsy()
    {
        // Arrange
        _context.Set("Items", new List<string>());
        var template = @"@if (Context.Items) {
Has Items
} else {
No Items
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("No Items");
    }

    [Fact]
    public void ItShouldTreatTrueBoolAsTruthy()
    {
        // Arrange
        _context.Set("Flag", true);
        var template = @"@if (Context.Flag) {
True
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("True");
    }

    [Fact]
    public void ItShouldTreatFalseBoolAsFalsy()
    {
        // Arrange
        _context.Set("Flag", false);
        var template = @"@if (Context.Flag) {
True
} else {
False
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("False");
        result.Should().NotContain("True\n");
    }

    [Fact]
    public void ItShouldTreatObjectAsTruthy()
    {
        // Arrange
        _context.Set("Obj", new { Name = "Test" });
        var template = @"@if (Context.Obj) {
Has Object
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Has Object");
    }

    #endregion

    #region Compare Method Edge Cases

    [Fact]
    public void ItShouldCompareValueWithNullLiteral()
    {
        // Arrange
        _context.Set("Value", "something");
        var template = @"@if (Context.Value != null) {
Not Null
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Not Null");
    }

    [Fact]
    public void ItShouldCompareMissingValueWithNull()
    {
        // Arrange - Context.Missing is not set, so it's null
        var template = @"@if (Context.Missing == null) {
Is Null
}";

    // Act
    var result = Sut.Render(template, _context);

    // Assert
    result.Should().Contain("Is Null");
}

[Fact]
public void ItShouldCompareNullWithValue()
{
        // Arrange
        _context.Set("A", null);
        _context.Set("B", 5);
        var template = @"@if (Context.A < Context.B) {
Null Less Than Value
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Null Less Than Value");
    }

    [Fact]
    public void ItShouldCompareValueWithNull()
    {
        // Arrange
        _context.Set("A", 5);
        var template = @"@if (Context.A > null) {
Value Greater Than Null
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Value Greater Than Null");
    }

    [Fact]
    public void ItShouldCompareStringsAlphabetically()
    {
        // Arrange
        _context.Set("A", "apple");
        _context.Set("B", "banana");
        var template = @"@if (Context.A < Context.B) {
Apple Before Banana
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Apple Before Banana");
    }

    #endregion

    #region Bug Fix Tests (GitHub Issues)

    [Fact]
    public void ItShouldThrowWhenForeachIteratesOverString_Issue11()
    {
        // Arrange - Issue #11: @foreach should not iterate over string characters
        _context.Set("Name", "Hello");
        var template = @"@foreach(var item in Context.Name) {
Item: ${item}
}";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => Sut.Render(template, _context));
        exception.Message.Should().Contain("Cannot iterate over string value in @foreach");
        exception.Message.Should().Contain("Context.Name");
    }

    [Fact]
    public void ItShouldHandleFloatingPointEqualityWithTolerance_Issue12()
    {
        // Arrange - Issue #12: Floating-point equality should use tolerance
        // The classic 0.1 + 0.2 != 0.3 problem
        _context.Set("Value1", 0.1 + 0.2);  // Results in 0.30000000000000004
        _context.Set("Value2", 0.3);
        
        var template = @"@if (Context.Value1 == Context.Value2) {
Values Are Equal
} else {
Values Are Not Equal
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert - Should be Equal with tolerance-based comparison
        result.Should().Contain("Values Are Equal");
        result.Should().NotContain("Values Are Not Equal");
    }

    [Fact]
    public void ItShouldHandleFloatingPointInequalityWithTolerance_Issue12()
    {
        // Arrange - Issue #12: Values outside tolerance should not be equal
        _context.Set("Value1", 1.0);
        _context.Set("Value2", 1.0001);  // Outside 1e-10 tolerance
        
        var template = @"@if (Context.Value1 == Context.Value2) {
Values Are Equal
} else {
Values Are Not Equal
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert - Should be Not Equal (outside tolerance)
        result.Should().Contain("Values Are Not Equal");
        result.Should().NotContain("Values Are Equal");
    }

    #endregion
}
