namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Comprehensive tests for ExecutionContext features.
/// Covers all public API surface area.
/// </summary>
public class WhenUsingExecutionContextTest : WhenTestingFor<ExecutionContext>
{
    protected override ExecutionContext For()
    {
        return new ExecutionContext();
    }

    protected override void Given()
    {
        // Base setup
    }

    protected override void When()
    {
        // When is handled in each test
    }

    #region Key and Parent Tests

    [Fact]
    public void ItShouldAllowSettingKey()
    {
        // Arrange
        var context = new ExecutionContext();

        // Act
        context.Key = "test-context";

        // Assert
        context.Key.Should().Be("test-context");
    }

    [Fact]
    public void ItShouldHaveNullKeyByDefault()
    {
        // Arrange & Act
        var context = new ExecutionContext();

        // Assert
        context.Key.Should().BeNull();
    }

    [Fact]
    public void ItShouldHaveNullParentByDefault()
    {
        // Arrange & Act
        var context = new ExecutionContext();

        // Assert
        context.Parent.Should().BeNull();
    }

    [Fact]
    public void ItShouldSetParentWhenCreatingChild()
    {
        // Arrange
        var parent = new ExecutionContext { Key = "parent" };

        // Act
        var child = parent.CreateChild("child");

        // Assert
        child.Parent.Should().BeSameAs(parent);
        child.Key.Should().Be("child");
    }

    #endregion

    #region Model Property Tests

    [Fact]
    public void ItShouldSetAndGetModel()
    {
        // Arrange
        var context = new ExecutionContext();
        var model = new { Name = "Test", Value = 42 };

        // Act
        context.Model = model;

        // Assert
        context.Model.Should().BeSameAs(model);
        context.Get("Model").Should().BeSameAs(model);
    }

    [Fact]
    public void ItShouldReturnNullForModelByDefault()
    {
        // Arrange
        var context = new ExecutionContext();

        // Act & Assert
        context.Model.Should().BeNull();
    }

    #endregion

    #region OutputPath Property Tests

    [Fact]
    public void ItShouldSetAndGetOutputPath()
    {
        // Arrange
        var context = new ExecutionContext();

        // Act
        context.OutputPath = @"C:\output\path";

        // Assert
        context.OutputPath.Should().Be(@"C:\output\path");
        context.Get("OutputPath").Should().Be(@"C:\output\path");
    }

    [Fact]
    public void ItShouldReturnNullForOutputPathByDefault()
    {
        // Arrange
        var context = new ExecutionContext();

        // Act & Assert
        context.OutputPath.Should().BeNull();
    }

    #endregion

    #region Cursor Stack Tests

    [Fact]
    public void ItShouldPushAndPopCursor()
    {
        // Arrange
        var context = new ExecutionContext();
        var item = new { Name = "Test" };

        // Act
        context.PushCursor(item, 5, "key1");

        // Assert
        context.Current.Should().BeSameAs(item);
        context.CurrentIndex.Should().Be(5);
        context.CurrentKey.Should().Be("key1");
        context.CursorDepth.Should().Be(1);

        // Act - Pop
        context.PopCursor();

        // Assert
        context.Current.Should().BeNull();
        context.CurrentIndex.Should().Be(-1);
        context.CurrentKey.Should().BeNull();
        context.CursorDepth.Should().Be(0);
    }

    [Fact]
    public void ItShouldHandleMultipleCursorPushes()
    {
        // Arrange
        var context = new ExecutionContext();
        var item1 = new { Name = "First" };
        var item2 = new { Name = "Second" };
        var item3 = new { Name = "Third" };

        // Act
        context.PushCursor(item1, 0);
        context.PushCursor(item2, 1);
        context.PushCursor(item3, 2);

        // Assert
        context.Current.Should().BeSameAs(item3);
        context.CurrentIndex.Should().Be(2);
        context.CursorDepth.Should().Be(3);

        // Pop and verify stack behavior
        context.PopCursor();
        context.Current.Should().BeSameAs(item2);
        context.CurrentIndex.Should().Be(1);

        context.PopCursor();
        context.Current.Should().BeSameAs(item1);
        context.CurrentIndex.Should().Be(0);

        context.PopCursor();
        context.Current.Should().BeNull();
    }

    [Fact]
    public void ItShouldHandlePopCursorOnEmptyStack()
    {
        // Arrange
        var context = new ExecutionContext();

        // Act - Pop on empty stack should not throw
        context.PopCursor();

        // Assert
        context.CursorDepth.Should().Be(0);
        context.Current.Should().BeNull();
    }

    [Fact]
    public void ItShouldUpdateVariablesWhenCursorChanges()
    {
        // Arrange
        var context = new ExecutionContext();
        var item = new { Value = 100 };

        // Act
        context.PushCursor(item, 42, "mykey");

        // Assert - Verify variables are set
        context.Get("Current").Should().BeSameAs(item);
        context.Get("CurrentIndex").Should().Be(42);
        context.Get("CurrentKey").Should().Be("mykey");
    }

    [Fact]
    public void ItShouldReturnNegativeOneForCurrentIndexWhenNoCursor()
    {
        // Arrange
        var context = new ExecutionContext();

        // Act & Assert
        context.CurrentIndex.Should().Be(-1);
    }

    #endregion

    #region Variable Tests

    [Fact]
    public void ItShouldCheckIfVariableExists()
    {
        // Arrange
        var context = new ExecutionContext();
        context.Set("Existing", "value");

        // Act & Assert
        context.Has("Existing").Should().BeTrue();
        context.Has("NonExisting").Should().BeFalse();
    }

    [Fact]
    public void ItShouldReturnAllVariableKeys()
    {
        // Arrange
        var context = new ExecutionContext();
        context.Set("Key1", "Value1");
        context.Set("Key2", "Value2");
        context.Set("Key3", "Value3");

        // Act
        var keys = context.Keys.ToList();

        // Assert
        keys.Should().HaveCount(3);
        keys.Should().Contain("Key1");
        keys.Should().Contain("Key2");
        keys.Should().Contain("Key3");
    }

    [Fact]
    public void ItShouldBeCaseInsensitiveForVariableKeys()
    {
        // Arrange
        var context = new ExecutionContext();
        context.Set("TestKey", "value");

        // Act & Assert
        context.Get("testkey").Should().Be("value");
        context.Get("TESTKEY").Should().Be("value");
        context.Get("TestKey").Should().Be("value");
        context.Has("testkey").Should().BeTrue();
    }

    #endregion

    #region CreateChild Tests

    [Fact]
    public void ItShouldInheritVariablesInChildContext()
    {
        // Arrange
        var parent = new ExecutionContext();
        parent.Set("Name", "Parent");
        parent.Set("Value", 42);

        // Act
        var child = parent.CreateChild();

        // Assert
        child.Get("Name").Should().Be("Parent");
        child.Get("Value").Should().Be(42);
    }

    [Fact]
    public void ItShouldInheritServicesInChildContext()
    {
        // Arrange
        var parent = new ExecutionContext();
        parent.RegisterService("upper", input => input?.ToString()?.ToUpper());

        // Act
        var child = parent.CreateChild();
        var result = child.Services("upper")("hello");

        // Assert
        result.Should().Be("HELLO");
    }

    [Fact]
    public void ItShouldNotAffectParentWhenModifyingChildVariables()
    {
        // Arrange
        var parent = new ExecutionContext();
        parent.Set("Name", "Parent");

        var child = parent.CreateChild();

        // Act
        child.Set("Name", "Child");
        child.Set("NewVar", "ChildOnly");

        // Assert
        parent.Get("Name").Should().Be("Parent");
        parent.Has("NewVar").Should().BeFalse();
        child.Get("Name").Should().Be("Child");
        child.Get("NewVar").Should().Be("ChildOnly");
    }

    #endregion

    #region Merge Tests

    [Fact]
    public void ItShouldMergeSpecifiedKeysFromOtherContext()
    {
        // Arrange
        var context1 = new ExecutionContext();
        context1.Set("Key1", "Value1");
        context1.Set("Key2", "Value2");

        var context2 = new ExecutionContext();
        context2.Set("Key2", "UpdatedValue2");
        context2.Set("Key3", "Value3");

        // Act
        context1.Merge(context2, "Key2", "Key3");

        // Assert
        context1.Get("Key1").Should().Be("Value1"); // Unchanged
        context1.Get("Key2").Should().Be("UpdatedValue2"); // Merged
        context1.Get("Key3").Should().Be("Value3"); // Merged
    }

    [Fact]
    public void ItShouldIgnoreNonExistentKeysWhenMerging()
    {
        // Arrange
        var context1 = new ExecutionContext();
        var context2 = new ExecutionContext();
        context2.Set("ExistingKey", "value");

        // Act - Should not throw
        context1.Merge(context2, "ExistingKey", "NonExistentKey");

        // Assert
        context1.Get("ExistingKey").Should().Be("value");
        context1.Has("NonExistentKey").Should().BeFalse();
    }

    [Fact]
    public void ItShouldHandleEmptyMergeKeysList()
    {
        // Arrange
        var context1 = new ExecutionContext();
        context1.Set("Key1", "Value1");

        var context2 = new ExecutionContext();
        context2.Set("Key2", "Value2");

        // Act
        context1.Merge(context2); // No keys specified

        // Assert
        context1.Get("Key1").Should().Be("Value1");
        context1.Has("Key2").Should().BeFalse();
    }

    #endregion

    #region Service Registration Tests

    [Fact]
    public void ItShouldRegisterServiceByKey()
    {
        // Arrange
        var context = new ExecutionContext();

        // Act
        context.RegisterService("test", input => $"Processed: {input}");
        var result = context.Services("test")("data");

        // Assert
        result.Should().Be("Processed: data");
    }

    [Fact]
    public void ItShouldRegisterITemplateService()
    {
        // Arrange
        var context = new ExecutionContext();
        var service = new TestService();

        // Act
        context.RegisterService(service);
        var result = context.Services("testservice")("input");

        // Assert
        result.Should().Be("TEST:input");
    }

    [Fact]
    public void ItShouldRegisterMultipleServices()
    {
        // Arrange
        var context = new ExecutionContext();
        var services = new List<ITemplateService>
        {
            new TestService(),
            new AnotherTestService()
        };

        // Act
        context.RegisterServices(services);

        // Assert
        context.Services("testservice")("a").Should().Be("TEST:a");
        context.Services("another")("b").Should().Be("ANOTHER:b");
    }

    [Fact]
    public void ItShouldReturnErrorFunctionForNonExistentService()
    {
        // Arrange
        var context = new ExecutionContext();

        // Act
        var result = context.Services("nonexistent")("test");

        // Assert
        result.Should().Be("{nonexistent not registered}");
    }

    [Fact]
    public void ItShouldBeCaseInsensitiveForServiceKeys()
    {
        // Arrange
        var context = new ExecutionContext();
        context.RegisterService("MyService", input => "result");

        // Act & Assert
        context.Services("myservice")("test").Should().Be("result");
        context.Services("MYSERVICE")("test").Should().Be("result");
        context.Services("MyService")("test").Should().Be("result");
    }

    [Fact]
    public void ItShouldOverwriteServiceWithSameKey()
    {
        // Arrange
        var context = new ExecutionContext();
        context.RegisterService("test", input => "first");

        // Act
        context.RegisterService("test", input => "second");
        var result = context.Services("test")("data");

        // Assert
        result.Should().Be("second");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ItShouldWorkWithComplexScenario()
    {
        // Arrange - Create parent context with services and variables
        var parent = new ExecutionContext();
        parent.Key = "parent-context";
        parent.Model = new { Name = "TestModel", Version = "1.0" };
        parent.OutputPath = @"C:\output";
        parent.Set("GlobalVar", "global-value");
        parent.RegisterService("transform", input => input?.ToString()?.ToUpper());

        // Act - Create child, modify, and merge back
        var child = parent.CreateChild("child-context");
        child.Set("LocalVar", "local-value");
        child.Set("Result", child.Services("transform")("hello"));

        // Push some cursor items
        child.PushCursor(new { Index = 0 }, 0);
        child.PushCursor(new { Index = 1 }, 1);

        // Assert
        child.Parent.Should().BeSameAs(parent);
        child.Get("GlobalVar").Should().Be("global-value");
        child.Get("LocalVar").Should().Be("local-value");
        child.Get("Result").Should().Be("HELLO");
        child.CurrentIndex.Should().Be(1);
        child.CursorDepth.Should().Be(2);

        // Merge result back to parent
        parent.Merge(child, "Result", "LocalVar");
        parent.Get("Result").Should().Be("HELLO");
        parent.Get("LocalVar").Should().Be("local-value");
    }

    #endregion

    #region Helper Classes

    private class TestService : ITemplateService
    {
        public string Name => "testservice";
        public object? Transform(object? input) => $"TEST:{input}";
    }

    private class AnotherTestService : ITemplateService
    {
        public string Name => "another";
        public object? Transform(object? input) => $"ANOTHER:{input}";
    }

    #endregion
}
