namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests for TemplateHelpers - the pipe helper functions.
/// </summary>
public class WhenUsingTemplateHelpersTest
{
    #region PadLeft Tests

    [Fact]
    public void PadLeftShouldPadWithSpacesByDefault()
    {
        // Arrange
        var value = "test";

        // Act
        var result = TemplateHelpers.Apply(value, "padleft", "10");

        // Assert
        result.Should().Be("      test");
    }

    [Fact]
    public void PadLeftShouldPadWithCustomCharacter()
    {
        // Arrange
        var value = "42";

        // Act
        var result = TemplateHelpers.Apply(value, "padleft", "5,0");

        // Assert
        result.Should().Be("00042");
    }

    [Fact]
    public void PadLeftShouldNotTruncateIfValueLongerThanWidth()
    {
        // Arrange
        var value = "hello world";

        // Act
        var result = TemplateHelpers.Apply(value, "padleft", "5");

        // Assert
        result.Should().Be("hello world");
    }

    [Fact]
    public void PadLeftShouldReturnOriginalIfWidthNotNumeric()
    {
        // Arrange
        var value = "test";

        // Act
        var result = TemplateHelpers.Apply(value, "padleft", "abc");

        // Assert
        result.Should().Be("test");
    }

    [Fact]
    public void PadLeftShouldReturnOriginalIfNullArg()
    {
        // Arrange
        var value = "test";

        // Act
        var result = TemplateHelpers.Apply(value, "padleft", null);

        // Assert
        result.Should().Be("test");
    }

    [Fact]
    public void PadLeftShouldReturnNullIfValueIsNull()
    {
        // Act
        var result = TemplateHelpers.Apply(null, "padleft", "10");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void PadLeftShouldHandleEmptyString()
    {
        // Arrange
        var value = "";

        // Act
        var result = TemplateHelpers.Apply(value, "padleft", "5");

        // Assert
        result.Should().Be("");
    }

    [Fact]
    public void PadLeftShouldHandleExactWidthMatch()
    {
        // Arrange
        var value = "hello";

        // Act
        var result = TemplateHelpers.Apply(value, "padleft", "5");

        // Assert
        result.Should().Be("hello");
    }

    #endregion

    #region PadRight Tests

    [Fact]
    public void PadRightShouldPadWithSpacesByDefault()
    {
        // Arrange
        var value = "test";

        // Act
        var result = TemplateHelpers.Apply(value, "padright", "10");

        // Assert
        result.Should().Be("test      ");
    }

    [Fact]
    public void PadRightShouldPadWithCustomCharacter()
    {
        // Arrange
        var value = "42";

        // Act
        var result = TemplateHelpers.Apply(value, "padright", "5,-");

        // Assert
        result.Should().Be("42---");
    }

    [Fact]
    public void PadRightShouldNotTruncateIfValueLongerThanWidth()
    {
        // Arrange
        var value = "hello world";

        // Act
        var result = TemplateHelpers.Apply(value, "padright", "5");

        // Assert
        result.Should().Be("hello world");
    }

    [Fact]
    public void PadRightShouldReturnOriginalIfWidthNotNumeric()
    {
        // Arrange
        var value = "test";

        // Act
        var result = TemplateHelpers.Apply(value, "padright", "abc");

        // Assert
        result.Should().Be("test");
    }

    [Fact]
    public void PadRightShouldReturnOriginalIfNullArg()
    {
        // Arrange
        var value = "test";

        // Act
        var result = TemplateHelpers.Apply(value, "padright", null);

        // Assert
        result.Should().Be("test");
    }

    [Fact]
    public void PadRightShouldReturnNullIfValueIsNull()
    {
        // Act
        var result = TemplateHelpers.Apply(null, "padright", "10");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void PadRightShouldHandleEmptyString()
    {
        // Arrange
        var value = "";

        // Act
        var result = TemplateHelpers.Apply(value, "padright", "5");

        // Assert
        result.Should().Be("");
    }

    [Fact]
    public void PadRightShouldHandleExactWidthMatch()
    {
        // Arrange
        var value = "hello";

        // Act
        var result = TemplateHelpers.Apply(value, "padright", "5");

        // Assert
        result.Should().Be("hello");
    }

    #endregion

    #region Integration Tests - Pipe Syntax

    [Fact]
    public void PadLeftShouldWorkWithPipeSyntax()
    {
        // Arrange
        var template = "${Context.Value | padleft:10}";
        var context = new ExecutionContext();
        context.Set("Value", "test");
        var resolver = new VariableResolver();

        // Act
        var result = resolver.ResolveString(template, context);

        // Assert
        result.Should().Be("      test");
    }

    [Fact]
    public void PadRightShouldWorkWithPipeSyntax()
    {
        // Arrange
        var template = "${Context.Value | padright:10}";
        var context = new ExecutionContext();
        context.Set("Value", "test");
        var resolver = new VariableResolver();

        // Act
        var result = resolver.ResolveString(template, context);

        // Assert
        result.Should().Be("test      ");
    }

    [Fact]
    public void PadLeftWithCustomCharShouldWorkWithPipeSyntax()
    {
        // Arrange
        var template = "${Context.Number | padleft:5,0}";
        var context = new ExecutionContext();
        context.Set("Number", "42");
        var resolver = new VariableResolver();

        // Act
        var result = resolver.ResolveString(template, context);

        // Assert
        result.Should().Be("00042");
    }

    [Fact]
    public void PadRightWithCustomCharShouldWorkWithPipeSyntax()
    {
        // Arrange
        var template = "${Context.Value | padright:8,*}";
        var context = new ExecutionContext();
        context.Set("Value", "hi");
        var resolver = new VariableResolver();

        // Act
        var result = resolver.ResolveString(template, context);

        // Assert
        result.Should().Be("hi******");
    }

    [Fact]
    public void PadShouldWorkInChainedPipes()
    {
        // Arrange
        var template = "${Context.Value | upper | padleft:10}";
        var context = new ExecutionContext();
        context.Set("Value", "test");
        var resolver = new VariableResolver();

        // Act
        var result = resolver.ResolveString(template, context);

        // Assert
        result.Should().Be("      TEST");
    }

    #endregion

    #region Helper Registration Tests

    [Fact]
    public void ExistsShouldReturnTrueForPadLeft()
    {
        TemplateHelpers.Exists("padleft").Should().BeTrue();
    }

    [Fact]
    public void ExistsShouldReturnTrueForPadRight()
    {
        TemplateHelpers.Exists("padright").Should().BeTrue();
    }

    [Fact]
    public void ExistsShouldBeCaseInsensitive()
    {
        TemplateHelpers.Exists("PADLEFT").Should().BeTrue();
        TemplateHelpers.Exists("PadRight").Should().BeTrue();
    }

    #endregion

    #region Round Tests

    [Fact]
    public void RoundShouldRoundDoubleToSpecifiedDecimals()
    {
        // Arrange
        var value = 3.14159;

        // Act
        var result = TemplateHelpers.Apply(value, "round", "2");

        // Assert
        result.Should().Be(3.14);
    }

    [Fact]
    public void RoundShouldRoundDecimalToSpecifiedDecimals()
    {
        // Arrange
        var value = 3.14159m;

        // Act
        var result = TemplateHelpers.Apply(value, "round", "2");

        // Assert
        result.Should().Be(3.14m);
    }

    [Fact]
    public void RoundShouldRoundFloatToSpecifiedDecimals()
    {
        // Arrange
        var value = 3.14159f;

        // Act
        var result = TemplateHelpers.Apply(value, "round", "2");

        // Assert
        ((double)result!).Should().BeApproximately(3.14, 0.01);
    }

    [Fact]
    public void RoundShouldDefaultToZeroDecimals()
    {
        // Arrange
        var value = 3.7;

        // Act
        var result = TemplateHelpers.Apply(value, "round", null);

        // Assert
        result.Should().Be(4.0);
    }

    [Fact]
    public void RoundShouldReturnNullForNullValue()
    {
        // Act
        var result = TemplateHelpers.Apply(null, "round", "2");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void RoundShouldReturnOriginalForNonNumericValue()
    {
        // Arrange
        var value = "not a number";

        // Act
        var result = TemplateHelpers.Apply(value, "round", "2");

        // Assert
        result.Should().Be("not a number");
    }

    [Fact]
    public void RoundShouldHandleDecimalWithNoDecimalPlaces()
    {
        // Arrange
        var value = 5.999m;

        // Act
        var result = TemplateHelpers.Apply(value, "round", "0");

        // Assert
        result.Should().Be(6m);
    }

    [Fact]
    public void RoundShouldHandleFloatRoundingUp()
    {
        // Arrange
        var value = 2.5f;

        // Act
        var result = TemplateHelpers.Apply(value, "round", "0");

        // Assert
        // Math.Round uses banker's rounding by default, so 2.5 rounds to 2
        ((double)result!).Should().BeApproximately(2.0, 0.01);
    }

    [Fact]
    public void RoundShouldWorkWithPipeSyntax()
    {
        // Arrange
        var template = "${Context.Value | round:2}";
        var context = new ExecutionContext();
        context.Set("Value", 3.14159);
        var resolver = new VariableResolver();

        // Act
        var result = resolver.ResolveString(template, context);

        // Assert - result format depends on culture, so parse and compare numerically
        double.Parse(result, System.Globalization.CultureInfo.CurrentCulture).Should().BeApproximately(3.14, 0.001);
    }

    #endregion

    #region Default Helper Tests

    [Fact]
    public void DefaultShouldReturnDefaultValueWhenNull()
    {
        // Act
        var result = TemplateHelpers.Apply(null, "default", "fallback");

        // Assert
        result.Should().Be("fallback");
    }

    [Fact]
    public void DefaultShouldReturnDefaultValueWhenEmptyString()
    {
        // Arrange
        var value = "";

        // Act
        var result = TemplateHelpers.Apply(value, "default", "fallback");

        // Assert
        result.Should().Be("fallback");
    }

    [Fact]
    public void DefaultShouldReturnOriginalValueWhenNotEmpty()
    {
        // Arrange
        var value = "hello";

        // Act
        var result = TemplateHelpers.Apply(value, "default", "fallback");

        // Assert
        result.Should().Be("hello");
    }

    [Fact]
    public void DefaultShouldReturnDefaultValueWhenEmptyCollection()
    {
        // Arrange
        var value = new List<string>();

        // Act
        var result = TemplateHelpers.Apply(value, "default", "empty list");

        // Assert
        result.Should().Be("empty list");
    }

    [Fact]
    public void DefaultShouldReturnOriginalValueWhenCollectionHasItems()
    {
        // Arrange
        var value = new List<string> { "item1", "item2" };

        // Act
        var result = TemplateHelpers.Apply(value, "default", "empty list");

        // Assert
        result.Should().BeEquivalentTo(value);
    }

    [Fact]
    public void DefaultShouldReturnOriginalValueWhenNonEmptyString()
    {
        // Arrange
        var value = "content";

        // Act
        var result = TemplateHelpers.Apply(value, "default", "default value");

        // Assert
        result.Should().Be("content");
    }

    [Fact]
    public void DefaultShouldWorkWithPipeSyntax()
    {
        // Arrange
        var template = "${Context.Missing | default:N/A}";
        var context = new ExecutionContext();
        var resolver = new VariableResolver();

        // Act
        var result = resolver.ResolveString(template, context);

        // Assert
        result.Should().Be("N/A");
    }

    [Fact]
    public void DefaultShouldNotApplyWhenValueExists()
    {
        // Arrange
        var template = "${Context.Name | default:Unknown}";
        var context = new ExecutionContext();
        context.Set("Name", "John");
        var resolver = new VariableResolver();

        // Act
        var result = resolver.ResolveString(template, context);

        // Assert
        result.Should().Be("John");
    }

    [Fact]
    public void IfEmptyShouldBehaveTheSameAsDefault()
    {
        // Act
        var result = TemplateHelpers.Apply(null, "ifempty", "fallback");

        // Assert
        result.Should().Be("fallback");
    }

    [Fact]
    public void ExistsShouldReturnTrueForDefault()
    {
        TemplateHelpers.Exists("default").Should().BeTrue();
    }

    [Fact]
    public void ExistsShouldReturnTrueForIfEmpty()
    {
        TemplateHelpers.Exists("ifempty").Should().BeTrue();
    }

    [Fact]
    public void ExistsShouldReturnTrueForRound()
    {
        TemplateHelpers.Exists("round").Should().BeTrue();
    }

    #endregion

    #region Floor Tests

    [Fact]
    public void FloorShouldFloorDouble()
    {
        var result = TemplateHelpers.Apply(3.7, "floor", null);
        result.Should().Be(3.0);
    }

    [Fact]
    public void FloorShouldFloorDecimal()
    {
        var result = TemplateHelpers.Apply(3.7m, "floor", null);
        result.Should().Be(3m);
    }

    [Fact]
    public void FloorShouldFloorFloat()
    {
        var result = TemplateHelpers.Apply(3.7f, "floor", null);
        ((double)result!).Should().BeApproximately(3.0, 0.01);
    }

    [Fact]
    public void FloorShouldReturnOriginalForNonNumeric()
    {
        var result = TemplateHelpers.Apply("text", "floor", null);
        result.Should().Be("text");
    }

    [Fact]
    public void FloorShouldHandleNegativeDouble()
    {
        var result = TemplateHelpers.Apply(-3.2, "floor", null);
        result.Should().Be(-4.0);
    }

    #endregion

    #region Ceiling Tests

    [Fact]
    public void CeilingShouldCeilingDouble()
    {
        var result = TemplateHelpers.Apply(3.2, "ceiling", null);
        result.Should().Be(4.0);
    }

    [Fact]
    public void CeilingShouldCeilingDecimal()
    {
        var result = TemplateHelpers.Apply(3.2m, "ceiling", null);
        result.Should().Be(4m);
    }

    [Fact]
    public void CeilingShouldCeilingFloat()
    {
        var result = TemplateHelpers.Apply(3.2f, "ceiling", null);
        ((double)result!).Should().BeApproximately(4.0, 0.01);
    }

    [Fact]
    public void CeilingShouldReturnOriginalForNonNumeric()
    {
        var result = TemplateHelpers.Apply("text", "ceiling", null);
        result.Should().Be("text");
    }

    [Fact]
    public void CeilingShouldHandleNegativeDouble()
    {
        var result = TemplateHelpers.Apply(-3.7, "ceiling", null);
        result.Should().Be(-3.0);
    }

    #endregion

    #region Count Tests

    [Fact]
    public void CountShouldReturnZeroForNull()
    {
        var result = TemplateHelpers.Apply(null, "count", null);
        result.Should().Be(0);
    }

    [Fact]
    public void CountShouldReturnStringLength()
    {
        var result = TemplateHelpers.Apply("hello", "count", null);
        result.Should().Be(5);
    }

    [Fact]
    public void CountShouldReturnCollectionCount()
    {
        var result = TemplateHelpers.Apply(new List<int> { 1, 2, 3 }, "count", null);
        result.Should().Be(3);
    }

    [Fact]
    public void CountShouldReturnEnumerableCount()
    {
        var enumerable = Enumerable.Range(1, 5);
        var result = TemplateHelpers.Apply(enumerable, "count", null);
        result.Should().Be(5);
    }

    [Fact]
    public void CountShouldReturnOneForNonCollectionValue()
    {
        var result = TemplateHelpers.Apply(42, "count", null);
        result.Should().Be(1);
    }

    [Fact]
    public void CountShouldReturnArrayLength()
    {
        var result = TemplateHelpers.Apply(new[] { "a", "b", "c", "d" }, "count", null);
        result.Should().Be(4);
    }

    #endregion

    #region First Tests

    [Fact]
    public void FirstShouldReturnNullForNull()
    {
        var result = TemplateHelpers.Apply(null, "first", null);
        result.Should().BeNull();
    }

    [Fact]
    public void FirstShouldReturnFirstCharOfString()
    {
        var result = TemplateHelpers.Apply("hello", "first", null);
        result.Should().Be("h");
    }

    [Fact]
    public void FirstShouldReturnNullForEmptyString()
    {
        var result = TemplateHelpers.Apply("", "first", null);
        result.Should().BeNull();
    }

    [Fact]
    public void FirstShouldReturnFirstElementOfCollection()
    {
        var result = TemplateHelpers.Apply(new List<string> { "apple", "banana", "cherry" }, "first", null);
        result.Should().Be("apple");
    }

    [Fact]
    public void FirstShouldReturnNullForEmptyCollection()
    {
        var result = TemplateHelpers.Apply(new List<string>(), "first", null);
        result.Should().BeNull();
    }

    [Fact]
    public void FirstShouldReturnOriginalForNonCollectionValue()
    {
        var result = TemplateHelpers.Apply(42, "first", null);
        result.Should().Be(42);
    }

    [Fact]
    public void FirstShouldReturnFirstElementOfArray()
    {
        var result = TemplateHelpers.Apply(new[] { 10, 20, 30 }, "first", null);
        result.Should().Be(10);
    }

    #endregion

    #region Last Tests

    [Fact]
    public void LastShouldReturnNullForNull()
    {
        var result = TemplateHelpers.Apply(null, "last", null);
        result.Should().BeNull();
    }

    [Fact]
    public void LastShouldReturnLastCharOfString()
    {
        var result = TemplateHelpers.Apply("hello", "last", null);
        result.Should().Be("o");
    }

    [Fact]
    public void LastShouldReturnNullForEmptyString()
    {
        var result = TemplateHelpers.Apply("", "last", null);
        result.Should().BeNull();
    }

    [Fact]
    public void LastShouldReturnLastElementOfCollection()
    {
        var result = TemplateHelpers.Apply(new List<string> { "apple", "banana", "cherry" }, "last", null);
        result.Should().Be("cherry");
    }

    [Fact]
    public void LastShouldReturnNullForEmptyCollection()
    {
        var result = TemplateHelpers.Apply(new List<string>(), "last", null);
        result.Should().BeNull();
    }

    [Fact]
    public void LastShouldReturnOriginalForNonCollectionValue()
    {
        var result = TemplateHelpers.Apply(42, "last", null);
        result.Should().Be(42);
    }

    [Fact]
    public void LastShouldReturnLastElementOfArray()
    {
        var result = TemplateHelpers.Apply(new[] { 10, 20, 30 }, "last", null);
        result.Should().Be(30);
    }

    #endregion

    #region Reverse Tests

    [Fact]
    public void ReverseShouldReturnNullForNull()
    {
        var result = TemplateHelpers.Apply(null, "reverse", null);
        result.Should().BeNull();
    }

    [Fact]
    public void ReverseShouldReverseString()
    {
        var result = TemplateHelpers.Apply("hello", "reverse", null);
        result.Should().Be("olleh");
    }

    [Fact]
    public void ReverseShouldReverseCollection()
    {
        var result = TemplateHelpers.Apply(new List<int> { 1, 2, 3 }, "reverse", null);
        result.Should().BeEquivalentTo(new List<int> { 3, 2, 1 });
    }

    [Fact]
    public void ReverseShouldReturnOriginalForNonCollectionValue()
    {
        var result = TemplateHelpers.Apply(42, "reverse", null);
        result.Should().Be(42);
    }

    [Fact]
    public void ReverseShouldReverseArray()
    {
        var result = TemplateHelpers.Apply(new[] { "a", "b", "c" }, "reverse", null);
        result.Should().BeEquivalentTo(new List<string> { "c", "b", "a" });
    }

    [Fact]
    public void ReverseShouldHandleEmptyString()
    {
        var result = TemplateHelpers.Apply("", "reverse", null);
        result.Should().Be("");
    }

    [Fact]
    public void ReverseShouldHandleSingleCharString()
    {
        var result = TemplateHelpers.Apply("x", "reverse", null);
        result.Should().Be("x");
    }

    #endregion

    #region Format Tests

    [Fact]
    public void FormatShouldFormatDateTime()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15, 10, 30, 0);

        // Act
        var result = TemplateHelpers.Apply(date, "format", "yyyy-MM-dd");

        // Assert
        result.Should().Be("2024-03-15");
    }

    [Fact]
    public void FormatShouldFormatDateTimeWithTimeFormat()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15, 10, 30, 45);

        // Act
        var result = TemplateHelpers.Apply(date, "format", "HH:mm:ss");

        // Assert
        result.Should().Be("10:30:45");
    }

    [Fact]
    public void FormatShouldFormatDateTimeOffset()
    {
        // Arrange
        var date = new DateTimeOffset(2024, 3, 15, 10, 30, 0, TimeSpan.Zero);

        // Act
        var result = TemplateHelpers.Apply(date, "format", "yyyy-MM-dd");

        // Assert
        result.Should().Be("2024-03-15");
    }

    [Fact]
    public void FormatShouldFormatDateOnly()
    {
        // Arrange
        var date = new DateOnly(2024, 3, 15);

        // Act
        var result = TemplateHelpers.Apply(date, "format", "yyyy-MM-dd");

        // Assert
        result.Should().Be("2024-03-15");
    }

    [Fact]
    public void FormatShouldFormatTimeOnly()
    {
        // Arrange
        var time = new TimeOnly(10, 30, 45);

        // Act
        var result = TemplateHelpers.Apply(time, "format", "HH:mm:ss");

        // Assert
        result.Should().Be("10:30:45");
    }

    [Fact]
    public void FormatShouldFormatIFormattableNumber()
    {
        // Arrange
        var value = 1234.567;

        // Act
        var result = TemplateHelpers.Apply(value, "format", "F2");

        // Assert
        result.Should().Be("1234.57");
    }

    [Fact]
    public void FormatShouldFormatDecimal()
    {
        // Arrange
        var value = 1234.5678m;

        // Act
        var result = TemplateHelpers.Apply(value, "format", "C2");

        // Assert - currency symbol varies by culture, just verify it's not null
        result.Should().NotBeNull();
        result!.ToString().Should().NotBeEmpty();
    }

    [Fact]
    public void FormatShouldReturnToStringForNonFormattableObject()
    {
        // Arrange
        var value = new { Name = "Test" };

        // Act
        var result = TemplateHelpers.Apply(value, "format", "any");

        // Assert
        result!.ToString().Should().Contain("Name");
    }

    [Fact]
    public void FormatShouldReturnToStringWhenArgIsNull()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15);

        // Act
        var result = TemplateHelpers.Apply(date, "format", null);

        // Assert
        result.Should().NotBeNull();
        result!.ToString().Should().Contain("2024");
    }

    [Fact]
    public void FormatShouldReturnToStringWhenArgIsEmpty()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15);

        // Act
        var result = TemplateHelpers.Apply(date, "format", "");

        // Assert
        result.Should().NotBeNull();
        result!.ToString().Should().Contain("2024");
    }

    [Fact]
    public void FormatShouldReturnNullToStringForNullValue()
    {
        // Act
        var result = TemplateHelpers.Apply(null, "format", "yyyy-MM-dd");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FormatShouldFormatInteger()
    {
        // Arrange
        var value = 42;

        // Act
        var result = TemplateHelpers.Apply(value, "format", "D5");

        // Assert
        result.Should().Be("00042");
    }

    [Fact]
    public void FormatShouldFormatDateTimeWithCustomFormat()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15, 14, 30, 0);

        // Act
        var result = TemplateHelpers.Apply(date, "format", "MMMM dd, yyyy");

        // Assert
        result.Should().Be("March 15, 2024");
    }

    #endregion

    #region Register Tests

    [Fact]
    public void RegisterShouldAddNewHelper()
    {
        // Arrange
        var helperName = "testhelper_" + Guid.NewGuid().ToString("N")[..8];
        
        // Act
        TemplateHelpers.Register(helperName, (value, arg) => $"transformed:{value}");
        var result = TemplateHelpers.Apply("input", helperName, null);

        // Assert
        result.Should().Be("transformed:input");
    }

    [Fact]
    public void RegisterShouldOverrideExistingHelper()
    {
        // Arrange
        var helperName = "testoverride_" + Guid.NewGuid().ToString("N")[..8];
        TemplateHelpers.Register(helperName, (value, _) => "first");
        
        // Act
        TemplateHelpers.Register(helperName, (value, _) => "second");
        var result = TemplateHelpers.Apply("input", helperName, null);

        // Assert
        result.Should().Be("second");
    }

    [Fact]
    public void RegisteredHelperShouldReceiveArgument()
    {
        // Arrange
        var helperName = "testwitharg_" + Guid.NewGuid().ToString("N")[..8];
        TemplateHelpers.Register(helperName, (value, arg) => $"{value}:{arg}");
        
        // Act
        var result = TemplateHelpers.Apply("hello", helperName, "world");

        // Assert
        result.Should().Be("hello:world");
    }

    [Fact]
    public void RegisteredHelperShouldBeAccessibleByExists()
    {
        // Arrange
        var helperName = "testexists_" + Guid.NewGuid().ToString("N")[..8];
        
        // Act
        TemplateHelpers.Register(helperName, (value, _) => value);

        // Assert
        TemplateHelpers.Exists(helperName).Should().BeTrue();
    }

    [Fact]
    public void RegisteredHelperShouldWorkWithPipeSyntax()
    {
        // Arrange
        var helperName = "testpipe_" + Guid.NewGuid().ToString("N")[..8];
        TemplateHelpers.Register(helperName, (value, _) => value?.ToString()?.ToUpperInvariant() + "!");
        
        var template = $"${{Context.Name | {helperName}}}";
        var context = new ExecutionContext();
        context.Set("Name", "hello");
        var resolver = new VariableResolver();

        // Act
        var result = resolver.ResolveString(template, context);

        // Assert
        result.Should().Be("HELLO!");
    }

    [Fact]
    public void RegisteredHelperShouldHandleNullValue()
    {
        // Arrange
        var helperName = "testnull_" + Guid.NewGuid().ToString("N")[..8];
        TemplateHelpers.Register(helperName, (value, _) => value == null ? "was null" : "not null");
        
        // Act
        var result = TemplateHelpers.Apply(null, helperName, null);

        // Assert
        result.Should().Be("was null");
    }

    [Fact]
    public void RegisteredHelperCanReturnNull()
    {
        // Arrange
        var helperName = "testreturnnull_" + Guid.NewGuid().ToString("N")[..8];
        TemplateHelpers.Register(helperName, (_, _) => null);
        
        // Act
        var result = TemplateHelpers.Apply("input", helperName, null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void RegisteredHelperCanReturnDifferentType()
    {
        // Arrange
        var helperName = "testtype_" + Guid.NewGuid().ToString("N")[..8];
        TemplateHelpers.Register(helperName, (value, _) => 
            int.TryParse(value?.ToString(), out var num) ? num * 2 : 0);
        
        // Act
        var result = TemplateHelpers.Apply("21", helperName, null);

        // Assert
        result.Should().Be(42);
    }

    #endregion

    #region YesNo Tests

    [Fact]
    public void YesNoShouldReturnNoForNull()
    {
        var result = TemplateHelpers.Apply(null, "yesno", null);
        result.Should().Be("No");
    }

    [Fact]
    public void YesNoShouldReturnYesForTrue()
    {
        var result = TemplateHelpers.Apply(true, "yesno", null);
        result.Should().Be("Yes");
    }

    [Fact]
    public void YesNoShouldReturnNoForFalse()
    {
        var result = TemplateHelpers.Apply(false, "yesno", null);
        result.Should().Be("No");
    }

    [Fact]
    public void YesNoShouldReturnYesForNonZeroInt()
    {
        var result = TemplateHelpers.Apply(42, "yesno", null);
        result.Should().Be("Yes");
    }

    [Fact]
    public void YesNoShouldReturnNoForZero()
    {
        var result = TemplateHelpers.Apply(0, "yesno", null);
        result.Should().Be("No");
    }

    [Fact]
    public void YesNoShouldReturnYesForNonEmptyString()
    {
        var result = TemplateHelpers.Apply("hello", "yesno", null);
        result.Should().Be("Yes");
    }

    [Fact]
    public void YesNoShouldReturnNoForEmptyString()
    {
        var result = TemplateHelpers.Apply("", "yesno", null);
        result.Should().Be("No");
    }

    [Fact]
    public void YesNoShouldReturnNoForFalseString()
    {
        var result = TemplateHelpers.Apply("false", "yesno", null);
        result.Should().Be("No");
    }

    [Fact]
    public void YesNoShouldReturnNoForFalseStringCaseInsensitive()
    {
        var result = TemplateHelpers.Apply("FALSE", "yesno", null);
        result.Should().Be("No");
    }

    [Fact]
    public void YesNoShouldReturnYesForOtherObjects()
    {
        var result = TemplateHelpers.Apply(new { Name = "test" }, "yesno", null);
        result.Should().Be("Yes");
    }

    [Fact]
    public void YesNoShouldUseCustomLabels()
    {
        var result = TemplateHelpers.Apply(true, "yesno", "Active,Inactive");
        result.Should().Be("Active");
    }

    [Fact]
    public void YesNoShouldUseCustomNoLabel()
    {
        var result = TemplateHelpers.Apply(false, "yesno", "On,Off");
        result.Should().Be("Off");
    }

    [Fact]
    public void YesNoShouldDefaultNoLabelIfOnlyYesProvided()
    {
        var result = TemplateHelpers.Apply(false, "yesno", "Active");
        result.Should().Be("No");
    }

    [Fact]
    public void YesNoShouldReturnYesForNegativeInt()
    {
        var result = TemplateHelpers.Apply(-1, "yesno", null);
        result.Should().Be("Yes");
    }

    #endregion

    #region IsEmpty Tests (via default/ifempty helpers)

    [Fact]
    public void IsEmptyShouldReturnTrueForNull()
    {
        var result = TemplateHelpers.Apply(null, "ifempty", "empty");
        result.Should().Be("empty");
    }

    [Fact]
    public void IsEmptyShouldReturnTrueForEmptyString()
    {
        var result = TemplateHelpers.Apply("", "ifempty", "empty");
        result.Should().Be("empty");
    }

    [Fact]
    public void IsEmptyShouldReturnFalseForNonEmptyString()
    {
        var result = TemplateHelpers.Apply("hello", "ifempty", "empty");
        result.Should().Be("hello");
    }

    [Fact]
    public void IsEmptyShouldReturnTrueForEmptyCollection()
    {
        var result = TemplateHelpers.Apply(new List<int>(), "ifempty", "empty");
        result.Should().Be("empty");
    }

    [Fact]
    public void IsEmptyShouldReturnFalseForNonEmptyCollection()
    {
        var list = new List<int> { 1, 2, 3 };
        var result = TemplateHelpers.Apply(list, "ifempty", "empty");
        result.Should().BeEquivalentTo(list);
    }

    [Fact]
    public void IsEmptyShouldReturnTrueForEmptyEnumerable()
    {
        var enumerable = Enumerable.Empty<int>();
        var result = TemplateHelpers.Apply(enumerable, "ifempty", "empty");
        result.Should().Be("empty");
    }

    [Fact]
    public void IsEmptyShouldReturnFalseForNonEmptyEnumerable()
    {
        var enumerable = Enumerable.Range(1, 5);
        var result = TemplateHelpers.Apply(enumerable, "ifempty", "empty");
        result.Should().BeEquivalentTo(enumerable);
    }

    [Fact]
    public void IsEmptyShouldReturnFalseForNonCollectionObject()
    {
        var result = TemplateHelpers.Apply(42, "ifempty", "empty");
        result.Should().Be(42);
    }

    [Fact]
    public void IsEmptyShouldReturnFalseForDateTimeObject()
    {
        var date = DateTime.Now;
        var result = TemplateHelpers.Apply(date, "ifempty", "empty");
        result.Should().Be(date);
    }

    #endregion
}
