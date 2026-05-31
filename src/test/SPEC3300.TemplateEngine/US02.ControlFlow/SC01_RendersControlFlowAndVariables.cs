using ExecutionContext = LowlandTech.TinyTools.Core.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

/// <summary>
/// Comprehensive tests for TinyTemplateEngine features.
/// Covers all public and internal functionality.
/// </summary>
[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Template engine renders control flow and variables")]
public class WhenUsingTinyTemplateEngineTest : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _context = new ExecutionContext();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        // When is handled in each test
    }

    #region Basic Rendering Tests

    [Trait(Spec.UAC, "01")]
    [Then("it Should Render Empty Template")]
    [Fact]
    public void ItShouldRenderEmptyTemplate()
    {
        ArrangeAndAct();
        // Arrange
        var template = "";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Render Null Template")]
    [Fact]
    public void ItShouldRenderNullTemplate()
    {
        ArrangeAndAct();
        // Arrange
        string? template = null;

        // Act
        var result = Sut.Render(template!, _context);

        // Assert
        result.Should().BeNull();
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Render Template Without Variables")]
    [Fact]
    public void ItShouldRenderTemplateWithoutVariables()
    {
        ArrangeAndAct();
        // Arrange
        var template = "Hello, World!";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("Hello, World!");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Render Template With Simple Variable")]
    [Fact]
    public void ItShouldRenderTemplateWithSimpleVariable()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Name", "John");
        var template = "Hello, ${Context.Name}!";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("Hello, John!");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Render Template With Multiple Variables")]
    [Fact]
    public void ItShouldRenderTemplateWithMultipleVariables()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "06")]
    [Then("it Should Remove Single Line Comment")]
    [Fact]
    public void ItShouldRemoveSingleLineComment()
    {
        ArrangeAndAct();
        // Arrange
        var template = "Before @* comment *@ After";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("Before  After");
    }

    [Trait(Spec.UAC, "07")]
    [Then("it Should Remove Multi Line Comment")]
    [Fact]
    public void ItShouldRemoveMultiLineComment()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "08")]
    [Then("it Should Remove Multiple Comments")]
    [Fact]
    public void ItShouldRemoveMultipleComments()
    {
        ArrangeAndAct();
        // Arrange
        var template = "A @* comment1 *@ B @* comment2 *@ C";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("A  B  C");
    }

    #endregion

    #region If Statement Tests

    [Trait(Spec.UAC, "09")]
    [Then("it Should Render If Block When Condition Is True")]
    [Fact]
    public void ItShouldRenderIfBlockWhenConditionIsTrue()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "10")]
    [Then("it Should Not Render If Block When Condition Is False")]
    [Fact]
    public void ItShouldNotRenderIfBlockWhenConditionIsFalse()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "11")]
    [Then("it Should Render Else Block When Condition Is False")]
    [Fact]
    public void ItShouldRenderElseBlockWhenConditionIsFalse()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "12")]
    [Then("it Should Render Else If Block When Condition Matches")]
    [Fact]
    public void ItShouldRenderElseIfBlockWhenConditionMatches()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "13")]
    [Then("it Should Handle Negation Operator")]
    [Fact]
    public void ItShouldHandleNegationOperator()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "14")]
    [Then("it Should Handle Greater Than Comparison")]
    [Fact]
    public void ItShouldHandleGreaterThanComparison()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "15")]
    [Then("it Should Handle Greater Than Or Equal Comparison")]
    [Fact]
    public void ItShouldHandleGreaterThanOrEqualComparison()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "16")]
    [Then("it Should Handle Less Than Comparison")]
    [Fact]
    public void ItShouldHandleLessThanComparison()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "17")]
    [Then("it Should Handle Less Than Or Equal Comparison")]
    [Fact]
    public void ItShouldHandleLessThanOrEqualComparison()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "18")]
    [Then("it Should Handle Equality Comparison")]
    [Fact]
    public void ItShouldHandleEqualityComparison()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "19")]
    [Then("it Should Handle Inequality Comparison")]
    [Fact]
    public void ItShouldHandleInequalityComparison()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "20")]
    [Then("it Should Handle Truthy Check For String")]
    [Fact]
    public void ItShouldHandleTruthyCheckForString()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "21")]
    [Then("it Should Handle Truthy Check For Empty String")]
    [Fact]
    public void ItShouldHandleTruthyCheckForEmptyString()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "22")]
    [Then("it Should Handle Truthy Check For Null")]
    [Fact]
    public void ItShouldHandleTruthyCheckForNull()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "23")]
    [Then("it Should Handle Truthy Check For Number")]
    [Fact]
    public void ItShouldHandleTruthyCheckForNumber()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "24")]
    [Then("it Should Handle Truthy Check For Collection")]
    [Fact]
    public void ItShouldHandleTruthyCheckForCollection()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "25")]
    [Then("it Should Iterate Over Collection")]
    [Fact]
    public void ItShouldIterateOverCollection()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "26")]
    [Then("it Should Handle Empty Collection")]
    [Fact]
    public void ItShouldHandleEmptyCollection()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "27")]
    [Then("it Should Handle Null Collection")]
    [Fact]
    public void ItShouldHandleNullCollection()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "28")]
    [Then("it Should Access Item Properties In Foreach")]
    [Fact]
    public void ItShouldAccessItemPropertiesInForeach()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "29")]
    [Then("it Should Expose Index Metadata In Foreach")]
    [Fact]
    public void ItShouldExposeIndexMetadataInForeach()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Items", new[] { "A", "B", "C" });
        var template = @"@foreach(var item in Context.Items) {
${_index}:${item}
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("0:A");
        result.Should().Contain("1:B");
        result.Should().Contain("2:C");
    }

    [Trait(Spec.UAC, "30")]
    [Then("it Should Expose First And Last Metadata In Foreach")]
    [Fact]
    public void ItShouldExposeFirstAndLastMetadataInForeach()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Items", new[] { "A", "B", "C" });
        var template = @"@foreach(var item in Context.Items) {
@if(_first) {
First: ${item}
} else if (_last) {
Last: ${item}
} else {
Middle: ${item}
}
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("First: A");
        result.Should().Contain("Middle: B");
        result.Should().Contain("Last: C");
    }

    [Fact]
    public void ItShouldExposeCountMetadataInForeach()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Items", new[] { "X", "Y" });
        var template = @"@foreach(var item in Context.Items) {
${item} (${_count} total)
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("X (2 total)");
        result.Should().Contain("Y (2 total)");
    }

    [Fact]
    public void ItShouldExposeFirstLastForSingleItemCollection()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Items", new[] { "Only" });
        var template = @"@foreach(var item in Context.Items) {
@if(_first) {
first
}
@if(_last) {
last
}
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert — single item is both first and last
        result.Should().Contain("first");
        result.Should().Contain("last");
    }

    [Fact]
    public void ItShouldExposeLoopMetadataWithEndSyntax()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Items", new[] { "A", "B", "C" });
        var template = "@foreach(var item in Context.Items)\n@if(_first)\nGiven ${item}\n@end\n@if(!_first)\nAnd ${item}\n@end\n@end";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Given A");
        result.Should().Contain("And B");
        result.Should().Contain("And C");
        result.Should().NotContain("Given B");
        result.Should().NotContain("Given C");
    }

    #endregion

    #region Nested Control Flow Tests

    [Fact]
    public void ItShouldHandleNestedIfStatements()
    {
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
    ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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
        ArrangeAndAct();
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

    #region Negation with Parentheses Tests (Code Coverage)

    [Fact]
    public void ItShouldHandleNegationWithParentheses()
    {
        ArrangeAndAct();
        // Arrange - Tests the code path: !(Context.IsActive) with parentheses removal
        _context.Set("IsActive", true);
        var template = @"@if (!(Context.IsActive)) {
Not Active
} else {
Active
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert - IsActive is true, so !(IsActive) is false
        result.Should().Contain("Active");
        result.Should().NotContain("Not Active");
    }

    [Fact]
    public void ItShouldHandleNestedNegationWithParentheses()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Flag", false);
        var template = @"@if (!(Context.Flag)) {
Flag Is False
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert - Flag is false, so !(Flag) is true
        result.Should().Contain("Flag Is False");
    }

    #endregion

    #region Context Expression in Comparisons Tests (Code Coverage)

    [Fact]
    public void ItShouldCompareContextExpressionOnRightSide()
    {
        ArrangeAndAct();
        // Arrange - Tests ResolveValueOrExpression with Context.* on right side
        _context.Set("A", 10);
        _context.Set("B", 5);
        var template = @"@if (Context.A > Context.B) {
A Greater Than B
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("A Greater Than B");
    }

    [Fact]
    public void ItShouldCompareTwoContextExpressionsEquality()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("X", "hello");
        _context.Set("Y", "hello");
        var template = @"@if (Context.X == Context.Y) {
Same Value
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Same Value");
    }

    [Fact]
    public void ItShouldCompareTwoContextExpressionsInequality()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("X", "hello");
        _context.Set("Y", "world");
        var template = @"@if (Context.X != Context.Y) {
Different Values
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Different Values");
    }

    [Fact]
    public void ItShouldHandleLiteralOnRightSideInComparison()
    {
        ArrangeAndAct();
        // Arrange - Tests ResolveValueOrExpression with literal (no Context.) on right
        _context.Set("Score", 95);
        var template = @"@if (Context.Score >= 90) {
Grade A
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Grade A");
    }

    [Fact]
    public void ItShouldCompareTwoContextExpressionsLessThan()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Min", 1);
        _context.Set("Max", 100);
        var template = @"@if (Context.Min < Context.Max) {
Min Less Than Max
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Min Less Than Max");
    }

    [Fact]
    public void ItShouldCompareTwoContextExpressionsLessThanOrEqual()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Value1", 5);
        _context.Set("Value2", 5);
        var template = @"@if (Context.Value1 <= Context.Value2) {
Less Or Equal
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Less Or Equal");
    }

    [Fact]
    public void ItShouldCompareTwoContextExpressionsGreaterThanOrEqual()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Current", 10);
        _context.Set("Previous", 8);
        var template = @"@if (Context.Current >= Context.Previous) {
Current Greater Or Equal
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Current Greater Or Equal");
    }

    #endregion

    #region Malformed Syntax Edge Cases (Code Coverage)

    [Fact]
    public void ItShouldHandleMalformedForeachStatement()
    {
        ArrangeAndAct();
        // Arrange - Tests line 199: @foreach that doesn't match the expected pattern
        var template = @"@foreach {
This is malformed
}
Normal text";

        // Act
        var result = Sut.Render(template, _context);

        // Assert - Should preserve the malformed line and continue
        result.Should().Contain("@foreach {");
        result.Should().Contain("Normal text");
    }

    [Fact]
    public void ItShouldHandleMalformedIfStatement()
    {
        ArrangeAndAct();
        // Arrange - Tests line 93: @if that doesn't match the expected pattern
        var template = @"@if {
This is malformed
}
Normal text";

        // Act
        var result = Sut.Render(template, _context);

        // Assert - Should preserve the malformed line and continue
        result.Should().Contain("@if {");
        result.Should().Contain("Normal text");
    }

    #endregion
}
