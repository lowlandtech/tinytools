using FluentAssertions;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Stress tests for TinyTemplateEngine to verify it handles complex and edge-case patterns.
/// </summary>
public class WhenStressingTinyTemplateEngine : WhenTestingFor<TinyTemplateEngine>
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

    #region Nested Control Flow Tests

    [Fact]
    public void ItShouldHandleNestedForeachInsideIf()
    {
        // Arrange
        _context.Set("ShowList", true);
        _context.Set("Items", new[] { "A", "B", "C" });

        var template = @"@if (Context.ShowList) {
List:
@foreach (var item in Context.Items) {
- ${item}
}
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("List:");
        result.Should().Contain("- A");
        result.Should().Contain("- B");
        result.Should().Contain("- C");
    }

    [Fact]
    public void ItShouldHandleNestedIfInsideForeach()
    {
        // Arrange
        var items = new[]
        {
            new { Name = "Item1", Active = true },
            new { Name = "Item2", Active = false },
            new { Name = "Item3", Active = true }
        };
        _context.Set("Items", items);

        var template = @"@foreach (var item in Context.Items) {
@if (item.Active) {
Active: ${item.Name}
}
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Active: Item1");
        result.Should().NotContain("Item2");
        result.Should().Contain("Active: Item3");
    }

    [Fact]
    public void ItShouldHandleDeepNestedControlFlow()
    {
        // Arrange
        var categories = new[]
        {
            new { 
                Name = "Category1", 
                Items = new[] { 
                    new { Name = "SubA", Visible = true },
                    new { Name = "SubB", Visible = false }
                }
            }
        };
        _context.Set("Categories", categories);
        _context.Set("ShowAll", true);

        var template = @"@if (Context.ShowAll) {
@foreach (var cat in Context.Categories) {
Category: ${cat.Name}
@foreach (var item in cat.Items) {
@if (item.Visible) {
  - ${item.Name}
}
}
}
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Category: Category1");
        result.Should().Contain("- SubA");
        result.Should().NotContain("SubB");
    }

    #endregion

    #region Empty and Null Handling Tests

    [Fact]
    public void ItShouldHandleEmptyCollection()
    {
        // Arrange
        _context.Set("Items", Array.Empty<string>());

        var template = @"Before
@foreach (var item in Context.Items) {
${item}
}
After";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Before");
        result.Should().Contain("After");
    }

    [Fact]
    public void ItShouldHandleNullCollectionGracefully()
    {
        // Arrange
        _context.Set("Items", null);

        var template = @"Before
@foreach (var item in Context.Items) {
${item}
}
After";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Before");
        result.Should().Contain("After");
    }

    [Fact]
    public void ItShouldHandleNullValueInCondition()
    {
        // Arrange
        _context.Set("Value", null);

        var template = @"@if (Context.Value == null) {
Value is null
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Value is null");
    }

    [Fact]
    public void ItShouldHandleMissingContextValue()
    {
        // Arrange - don't set any value

        var template = @"Value: ${Context.MissingValue ?? 'default'}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Value: default");
    }

    #endregion

    #region Ternary Expression Tests

    [Fact]
    public void ItShouldHandleTernaryWithNullCheck()
    {
        // Arrange
        _context.Set("Name", "John");

        var template = @"Hello ${Context.Name != null ? Context.Name : 'Guest'}!";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("Hello John!");
    }

    [Fact]
    public void ItShouldHandleTernaryWithNullValue()
    {
        // Arrange
        _context.Set("Name", null);

        var template = @"Hello ${Context.Name != null ? Context.Name : 'Guest'}!";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("Hello Guest!");
    }

    [Fact]
    public void ItShouldHandleTernaryWithStringConcatenation()
    {
        // Arrange
        _context.Set("FirstName", "John");
        _context.Set("HasLastName", true);
        _context.Set("LastName", "Doe");

        var template = @"Name: ${Context.HasLastName ? Context.FirstName + ' ' + Context.LastName : Context.FirstName}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("Name: John Doe");
    }

    [Fact]
    public void ItShouldHandleNestedTernaryLikePattern()
    {
        // Arrange
        _context.Set("Status", "active");

        var template = @"Status: ${Context.Status == 'active' ? 'Active' : 'Inactive'}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("Status: Active");
    }

    #endregion

    #region Complex Property Access Tests

    [Fact]
    public void ItShouldHandleDeeplyNestedProperties()
    {
        // Arrange
        var data = new
        {
            Level1 = new
            {
                Level2 = new
                {
                    Level3 = new { Value = "DeepValue" }
                }
            }
        };
        _context.Set("Data", data);

        var template = @"Value: ${Context.Data.Level1.Level2.Level3.Value}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("Value: DeepValue");
    }

    [Fact]
    public void ItShouldHandlePropertyAccessOnCollectionItems()
    {
        // Arrange
        var users = new[]
        {
            new { Profile = new { Name = "Alice", Age = 30 } },
            new { Profile = new { Name = "Bob", Age = 25 } }
        };
        _context.Set("Users", users);

        var template = @"@foreach (var user in Context.Users) {
${user.Profile.Name}: ${user.Profile.Age}
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Alice: 30");
        result.Should().Contain("Bob: 25");
    }

    #endregion

    #region Pipe Helper Tests

    [Fact]
    public void ItShouldHandleMultiplePipeHelpers()
    {
        // Arrange
        _context.Set("Name", "  john doe  ");

        var template = @"${Context.Name | trim | upper}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("JOHN DOE");
    }

    [Fact]
    public void ItShouldHandlePipeHelpersInForeach()
    {
        // Arrange
        _context.Set("Names", new[] { "alice", "bob", "carol" });

        var template = @"@foreach (var name in Context.Names) {
${name | capitalize}
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Alice");
        result.Should().Contain("Bob");
        result.Should().Contain("Carol");
    }

    [Fact]
    public void ItShouldHandlePipeWithArguments()
    {
        // Arrange
        _context.Set("Text", "This is a very long text that should be truncated");

        var template = @"${Context.Text | truncate:20}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Length.Should().BeLessThanOrEqualTo(23); // 20 + "..."
    }

    #endregion

    #region Special Characters and Escaping Tests

    [Fact]
    public void ItShouldHandleSpecialCharactersInContent()
    {
        // Arrange
        _context.Set("Code", "if (x > 0 && y < 10) { return true; }");

        var template = @"Code: ${Context.Code}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("if (x > 0 && y < 10)");
    }

    [Fact]
    public void ItShouldHandleBracesInStringValues()
    {
        // Arrange
        _context.Set("Json", "{ \"key\": \"value\" }");

        var template = @"JSON: ${Context.Json}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("{ \"key\": \"value\" }");
    }

    [Fact]
    public void ItShouldHandleDollarSignInValues()
    {
        // Arrange
        _context.Set("Price", "$99.99");

        var template = @"Price: ${Context.Price}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("Price: $99.99");
    }

    #endregion

    #region Comment Handling Tests

    [Fact]
    public void ItShouldRemoveComments()
    {
        // Arrange
        _context.Set("Name", "World");

        var template = @"Hello @* this is a comment *@ ${Context.Name}!";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Hello");
        result.Should().Contain("World!");
        result.Should().NotContain("comment");
    }

    [Fact]
    public void ItShouldRemoveMultiLineComments()
    {
        // Arrange
        _context.Set("Value", "Test");

        var template = @"Before
@* This is a
multi-line
comment *@
After: ${Context.Value}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Before");
        result.Should().Contain("After: Test");
        result.Should().NotContain("multi-line");
    }

    #endregion

    #region Else-If Chain Tests

    [Fact]
    public void ItShouldHandleElseIfChain()
    {
        // Arrange
        _context.Set("Score", 75);

        var template = @"@if (Context.Score >= 90) {
Grade: A
} else if (Context.Score >= 80) {
Grade: B
} else if (Context.Score >= 70) {
Grade: C
} else {
Grade: F
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Grade: C");
        result.Should().NotContain("Grade: A");
        result.Should().NotContain("Grade: B");
        result.Should().NotContain("Grade: F");
    }

    [Fact]
    public void ItShouldHandleElseBlock()
    {
        // Arrange
        _context.Set("IsLoggedIn", false);

        var template = @"@if (Context.IsLoggedIn) {
Welcome back!
} else {
Please log in.
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Please log in.");
        result.Should().NotContain("Welcome back!");
    }

    #endregion

    #region Comparison Operator Tests

    [Fact]
    public void ItShouldHandleGreaterThanComparison()
    {
        // Arrange
        _context.Set("Count", 10);

        var template = @"@if (Context.Count > 5) {
More than 5
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("More than 5");
    }

    [Fact]
    public void ItShouldHandleLessThanOrEqualComparison()
    {
        // Arrange
        _context.Set("Count", 5);

        var template = @"@if (Context.Count <= 5) {
5 or less
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("5 or less");
    }

    [Fact]
    public void ItShouldHandleStringEquality()
    {
        // Arrange
        _context.Set("Status", "active");

        var template = @"@if (Context.Status == ""active"") {
Is Active
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Is Active");
    }

    [Fact]
    public void ItShouldHandleStringInequality()
    {
        // Arrange
        _context.Set("Status", "inactive");

        var template = @"@if (Context.Status != ""active"") {
Not Active
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Not Active");
    }

    #endregion

    #region Negation Tests

    [Fact]
    public void ItShouldHandleNegation()
    {
        // Arrange
        _context.Set("IsHidden", false);

        var template = @"@if (!Context.IsHidden) {
Visible
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Visible");
    }

    [Fact]
    public void ItShouldHandleNegationWithParentheses()
    {
        // Arrange
        _context.Set("IsAdmin", true);

        var template = @"@if (!(Context.IsAdmin)) {
Regular User
} else {
Admin User
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Admin User");
        result.Should().NotContain("Regular User");
    }

    #endregion

    #region Logical Operator Tests

    [Fact]
    public void ItShouldHandleLogicalAndBothTrue()
    {
        // Arrange
        _context.Set("IsLoggedIn", true);
        _context.Set("IsAdmin", true);

        var template = @"@if (Context.IsLoggedIn && Context.IsAdmin) {
Admin Panel
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Admin Panel");
    }

    [Fact]
    public void ItShouldHandleLogicalAndOneFalse()
    {
        // Arrange
        _context.Set("IsLoggedIn", true);
        _context.Set("IsAdmin", false);

        var template = @"@if (Context.IsLoggedIn && Context.IsAdmin) {
Admin Panel
} else {
Access Denied
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Access Denied");
        result.Should().NotContain("Admin Panel");
    }

    [Fact]
    public void ItShouldHandleLogicalOrOneTrue()
    {
        // Arrange
        _context.Set("HasItems", false);
        _context.Set("ShowEmpty", true);

        var template = @"@if (Context.HasItems || Context.ShowEmpty) {
Show Container
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Show Container");
    }

    [Fact]
    public void ItShouldHandleLogicalOrBothFalse()
    {
        // Arrange
        _context.Set("HasItems", false);
        _context.Set("ShowEmpty", false);

        var template = @"@if (Context.HasItems || Context.ShowEmpty) {
Show Container
} else {
Hidden
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Hidden");
        result.Should().NotContain("Show Container");
    }

    [Fact]
    public void ItShouldHandleMixedLogicalOperators()
    {
        // Arrange
        _context.Set("IsAdmin", true);
        _context.Set("IsModerator", false);
        _context.Set("IsOwner", true);

        // Admin OR (Moderator AND Owner)
        var template = @"@if (Context.IsAdmin || Context.IsModerator && Context.IsOwner) {
Has Access
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert - IsAdmin is true, so entire condition is true
        result.Should().Contain("Has Access");
    }

    [Fact]
    public void ItShouldHandleLogicalOperatorsWithComparisons()
    {
        // Arrange
        _context.Set("Age", 25);
        _context.Set("HasLicense", true);

        var template = @"@if (Context.Age >= 18 && Context.HasLicense) {
Can Drive
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Can Drive");
    }

    [Fact]
    public void ItShouldHandleLogicalOperatorsWithNegation()
    {
        // Arrange
        _context.Set("IsBlocked", false);
        _context.Set("IsVerified", true);

        var template = @"@if (!Context.IsBlocked && Context.IsVerified) {
Account Active
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Account Active");
    }

    [Fact]
    public void ItShouldHandleParenthesizedLogicalExpressions()
    {
        // Arrange
        _context.Set("A", true);
        _context.Set("B", false);
        _context.Set("C", true);

        // (A && B) || C = (true && false) || true = false || true = true
        var template = @"@if ((Context.A && Context.B) || Context.C) {
Result True
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Result True");
    }

    [Fact]
    public void ItShouldHandleMultipleOrConditions()
    {
        // Arrange
        _context.Set("Status", "pending");

        var template = @"@if (Context.Status == ""active"" || Context.Status == ""pending"" || Context.Status == ""approved"") {
In Progress
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("In Progress");
    }

    [Fact]
    public void ItShouldShortCircuitLogicalAnd()
    {
        // Arrange - Only set first value, second would cause error if evaluated
        _context.Set("HasValue", false);
        // Don't set "Value" - if && doesn't short-circuit, accessing Value.Length would fail

        var template = @"@if (Context.HasValue && Context.Value) {
Has Value
} else {
No Value
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert - Should short-circuit and not try to access Value
        result.Should().Contain("No Value");
    }

    [Fact]
    public void ItShouldShortCircuitLogicalOr()
    {
        // Arrange - Only set first value, second would cause error if evaluated
        _context.Set("HasDefault", true);
        // Don't set "ComputedValue" - if || doesn't short-circuit, it would try to access it

        var template = @"@if (Context.HasDefault || Context.ComputedValue) {
Has Something
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert - Should short-circuit and not try to access ComputedValue
        result.Should().Contain("Has Something");
    }

    #endregion

    #region Whitespace Handling Tests

    [Fact]
    public void ItShouldPreserveIndentation()
    {
        // Arrange
        _context.Set("ShowCode", true);

        var template = @"@if (Context.ShowCode) {
    function test() {
        return true;
    }
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("    function test()");
        result.Should().Contain("        return true;");
    }

    [Fact]
    public void ItShouldHandleEmptyLines()
    {
        // Arrange
        _context.Set("Name", "Test");

        var template = @"Line 1

Line 3: ${Context.Name}

Line 5";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Line 1");
        result.Should().Contain("Line 3: Test");
        result.Should().Contain("Line 5");
    }

    #endregion

    #region Multiple Variable Interpolation Tests

    [Fact]
    public void ItShouldHandleMultipleVariablesOnSameLine()
    {
        // Arrange
        _context.Set("First", "Hello");
        _context.Set("Second", "World");
        _context.Set("Third", "!");

        var template = @"${Context.First} ${Context.Second}${Context.Third}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("Hello World!");
    }

    [Fact]
    public void ItShouldHandleAdjacentVariables()
    {
        // Arrange
        _context.Set("Prefix", "pre");
        _context.Set("Suffix", "fix");

        var template = @"${Context.Prefix}${Context.Suffix}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Be("prefix");
    }

    #endregion

    #region Boolean Handling Tests

    [Fact]
    public void ItShouldHandleBooleanTrue()
    {
        // Arrange
        _context.Set("Flag", true);

        var template = @"@if (Context.Flag) {
Flag is true
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Flag is true");
    }

    [Fact]
    public void ItShouldHandleBooleanFalse()
    {
        // Arrange
        _context.Set("Flag", false);

        var template = @"@if (Context.Flag) {
Should not appear
}
Always appears";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().NotContain("Should not appear");
        result.Should().Contain("Always appears");
    }

    #endregion

    #region Collection Count Tests

    [Fact]
    public void ItShouldHandleCollectionCountCheck()
    {
        // Arrange
        _context.Set("Items", new[] { "A", "B", "C" });

        var template = @"@if (Context.Items) {
Has items
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("Has items");
    }

    [Fact]
    public void ItShouldHandleEmptyCollectionAsFalsy()
    {
        // Arrange
        _context.Set("Items", Array.Empty<string>());

        var template = @"@if (Context.Items) {
Has items
} else {
No items
}";

        // Act
        var result = Sut.Render(template, _context);

        // Assert
        result.Should().Contain("No items");
        result.Should().NotContain("Has items");
    }

    #endregion
}
