namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US01.ExecutionContext;

/// <summary>
/// Comprehensive tests for ExecutionContext features.
/// Covers all public API surface area.
/// </summary>
[Trait(Spec.SPEC, "3303")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Tool context manages variables and state")]
public class WhenUsingToolContextTest : TinyToolsScenario<Core.ExecutionContext>
{
    protected override Core.ExecutionContext For()
    {
        return new Core.ExecutionContext();
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        // Base setup
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        // When is handled in each test
    }

    #region Key and Parent Tests

    [Trait(Spec.UAC, "01")]
    [Then("it Should Allow Setting Key")]
    [Fact]
    public void ItShouldAllowSettingKey()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();

        // Act
        context.Key = "test-context";

        // Assert
        context.Key.Should().Be("test-context");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Have Null Key By Default")]
    [Fact]
    public void ItShouldHaveNullKeyByDefault()
    {
        ArrangeAndAct();
        // Arrange & Act
        var context = new Core.ExecutionContext();

        // Assert
        context.Key.Should().BeNull();
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Have Null Parent By Default")]
    [Fact]
    public void ItShouldHaveNullParentByDefault()
    {
        ArrangeAndAct();
        // Arrange & Act
        var context = new Core.ExecutionContext();

        // Assert
        context.Parent.Should().BeNull();
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Set Parent When Creating Child")]
    [Fact]
    public void ItShouldSetParentWhenCreatingChild()
    {
        ArrangeAndAct();
        // Arrange
        var parent = new Core.ExecutionContext { Key = "parent" };

        // Act
        var child = parent.CreateChild("child");

        // Assert
        child.Parent.Should().BeSameAs(parent);
        child.Key.Should().Be("child");
    }

    #endregion

    #region Model Property Tests

    [Trait(Spec.UAC, "05")]
    [Then("it Should Set And Get Model")]
    [Fact]
    public void ItShouldSetAndGetModel()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();
        var model = new { Name = "Test", Value = 42 };

        // Act
        context.Model = model;

        // Assert
        context.Model.Should().BeSameAs(model);
        context.Get("Model").Should().BeSameAs(model);
    }

    [Trait(Spec.UAC, "06")]
    [Then("it Should Return Null For Model By Default")]
    [Fact]
    public void ItShouldReturnNullForModelByDefault()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();

        // Act & Assert
        context.Model.Should().BeNull();
    }

    #endregion

    #region OutputPath Property Tests

    [Trait(Spec.UAC, "07")]
    [Then("it Should Set And Get Output Path")]
    [Fact]
    public void ItShouldSetAndGetOutputPath()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();

        // Act
        context.OutputPath = @"C:\output\path";

        // Assert
        context.OutputPath.Should().Be(@"C:\output\path");
        context.Get("OutputPath").Should().Be(@"C:\output\path");
    }

    [Trait(Spec.UAC, "08")]
    [Then("it Should Return Null For Output Path By Default")]
    [Fact]
    public void ItShouldReturnNullForOutputPathByDefault()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();

        // Act & Assert
        context.OutputPath.Should().BeNull();
    }

    #endregion

    #region Cursor Stack Tests

    [Trait(Spec.UAC, "09")]
    [Then("it Should Push And Pop Cursor")]
    [Fact]
    public void ItShouldPushAndPopCursor()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();
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

    [Trait(Spec.UAC, "10")]
    [Then("it Should Handle Multiple Cursor Pushes")]
    [Fact]
    public void ItShouldHandleMultipleCursorPushes()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();
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

    [Trait(Spec.UAC, "11")]
    [Then("it Should Handle Pop Cursor On Empty Stack")]
    [Fact]
    public void ItShouldHandlePopCursorOnEmptyStack()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();

        // Act - Pop on empty stack should not throw
        context.PopCursor();

        // Assert
        context.CursorDepth.Should().Be(0);
        context.Current.Should().BeNull();
    }

    [Trait(Spec.UAC, "12")]
    [Then("it Should Update Variables When Cursor Changes")]
    [Fact]
    public void ItShouldUpdateVariablesWhenCursorChanges()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();
        var item = new { Value = 100 };

        // Act
        context.PushCursor(item, 42, "mykey");

        // Assert - Verify variables are set
        context.Get("Current").Should().BeSameAs(item);
        context.Get("CurrentIndex").Should().Be(42);
        context.Get("CurrentKey").Should().Be("mykey");
    }

    [Trait(Spec.UAC, "13")]
    [Then("it Should Return Negative One For Current Index When No Cursor")]
    [Fact]
    public void ItShouldReturnNegativeOneForCurrentIndexWhenNoCursor()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();

        // Act & Assert
        context.CurrentIndex.Should().Be(-1);
    }

    #endregion

    #region Variable Tests

    [Trait(Spec.UAC, "14")]
    [Then("it Should Check If Variable Exists")]
    [Fact]
    public void ItShouldCheckIfVariableExists()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();
        context.Set("Existing", "value");

        // Act & Assert
        context.Has("Existing").Should().BeTrue();
        context.Has("NonExisting").Should().BeFalse();
    }

    [Trait(Spec.UAC, "15")]
    [Then("it Should Return All Variable Keys")]
    [Fact]
    public void ItShouldReturnAllVariableKeys()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();
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

    [Trait(Spec.UAC, "16")]
    [Then("it Should Be Case Insensitive For Variable Keys")]
    [Fact]
    public void ItShouldBeCaseInsensitiveForVariableKeys()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();
        context.Set("TestKey", "value");

        // Act & Assert
        context.Get("testkey").Should().Be("value");
        context.Get("TESTKEY").Should().Be("value");
        context.Get("TestKey").Should().Be("value");
        context.Has("testkey").Should().BeTrue();
    }

    #endregion

    #region CreateChild Tests

    [Trait(Spec.UAC, "17")]
    [Then("it Should Inherit Variables In Child Context")]
    [Fact]
    public void ItShouldInheritVariablesInChildContext()
    {
        ArrangeAndAct();
        // Arrange
        var parent = new Core.ExecutionContext();
        parent.Set("Name", "Parent");
        parent.Set("Value", 42);

        // Act
        var child = parent.CreateChild();

        // Assert
        child.Get("Name").Should().Be("Parent");
        child.Get("Value").Should().Be(42);
    }

    [Trait(Spec.UAC, "18")]
    [Then("it Should Inherit Services In Child Context")]
    [Fact]
    public void ItShouldInheritServicesInChildContext()
    {
        ArrangeAndAct();
        // Arrange
        var parent = new Core.ExecutionContext();
        parent.RegisterService("upper", input => input?.ToString()?.ToUpper());

        // Act
        var child = parent.CreateChild();
        var result = child.Services("upper")("hello");

        // Assert
        result.Should().Be("HELLO");
    }

    [Trait(Spec.UAC, "19")]
    [Then("it Should Not Affect Parent When Modifying Child Variables")]
    [Fact]
    public void ItShouldNotAffectParentWhenModifyingChildVariables()
    {
        ArrangeAndAct();
        // Arrange
        var parent = new Core.ExecutionContext();
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

    [Trait(Spec.UAC, "20")]
    [Then("it Should Merge Specified Keys From Other Context")]
    [Fact]
    public void ItShouldMergeSpecifiedKeysFromOtherContext()
    {
        ArrangeAndAct();
        // Arrange
        var context1 = new Core.ExecutionContext();
        context1.Set("Key1", "Value1");
        context1.Set("Key2", "Value2");

        var context2 = new Core.ExecutionContext();
        context2.Set("Key2", "UpdatedValue2");
        context2.Set("Key3", "Value3");

        // Act
        context1.Merge(context2, "Key2", "Key3");

        // Assert
        context1.Get("Key1").Should().Be("Value1"); // Unchanged
        context1.Get("Key2").Should().Be("UpdatedValue2"); // Merged
        context1.Get("Key3").Should().Be("Value3"); // Merged
    }

    [Trait(Spec.UAC, "21")]
    [Then("it Should Ignore Non Existent Keys When Merging")]
    [Fact]
    public void ItShouldIgnoreNonExistentKeysWhenMerging()
    {
        ArrangeAndAct();
        // Arrange
        var context1 = new Core.ExecutionContext();
        var context2 = new Core.ExecutionContext();
        context2.Set("ExistingKey", "value");

        // Act - Should not throw
        context1.Merge(context2, "ExistingKey", "NonExistentKey");

        // Assert
        context1.Get("ExistingKey").Should().Be("value");
        context1.Has("NonExistentKey").Should().BeFalse();
    }

    [Trait(Spec.UAC, "22")]
    [Then("it Should Handle Empty Merge Keys List")]
    [Fact]
    public void ItShouldHandleEmptyMergeKeysList()
    {
        ArrangeAndAct();
        // Arrange
        var context1 = new Core.ExecutionContext();
        context1.Set("Key1", "Value1");

        var context2 = new Core.ExecutionContext();
        context2.Set("Key2", "Value2");

        // Act
        context1.Merge(context2); // No keys specified

        // Assert
        context1.Get("Key1").Should().Be("Value1");
        context1.Has("Key2").Should().BeFalse();
    }

    #endregion

    #region Service Registration Tests

    [Trait(Spec.UAC, "23")]
    [Then("it Should Register Service By Key")]
    [Fact]
    public void ItShouldRegisterServiceByKey()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();

        // Act
        context.RegisterService("test", input => $"Processed: {input}");
        var result = context.Services("test")("data");

        // Assert
        result.Should().Be("Processed: data");
    }

    [Trait(Spec.UAC, "24")]
    [Then("it Should Register I Template Service")]
    [Fact]
    public void ItShouldRegisterITemplateService()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();
        var service = new TestService();

        // Act
        context.RegisterService(service);
        var result = context.Services("testservice")("input");

        // Assert
        result.Should().Be("TEST:input");
    }

    [Trait(Spec.UAC, "25")]
    [Then("it Should Register Multiple Services")]
    [Fact]
    public void ItShouldRegisterMultipleServices()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();
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

    [Trait(Spec.UAC, "26")]
    [Then("it Should Return Error Function For Non Existent Service")]
    [Fact]
    public void ItShouldReturnErrorFunctionForNonExistentService()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();

        // Act
        var result = context.Services("nonexistent")("test");

        // Assert
        result.Should().Be("{nonexistent not registered}");
    }

    [Trait(Spec.UAC, "27")]
    [Then("it Should Be Case Insensitive For Service Keys")]
    [Fact]
    public void ItShouldBeCaseInsensitiveForServiceKeys()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();
        context.RegisterService("MyService", input => "result");

        // Act & Assert
        context.Services("myservice")("test").Should().Be("result");
        context.Services("MYSERVICE")("test").Should().Be("result");
        context.Services("MyService")("test").Should().Be("result");
    }

    [Trait(Spec.UAC, "28")]
    [Then("it Should Overwrite Service With Same Key")]
    [Fact]
    public void ItShouldOverwriteServiceWithSameKey()
    {
        ArrangeAndAct();
        // Arrange
        var context = new Core.ExecutionContext();
        context.RegisterService("test", input => "first");

        // Act
        context.RegisterService("test", input => "second");
        var result = context.Services("test")("data");

        // Assert
        result.Should().Be("second");
    }

    #endregion

    #region Integration Tests

    [Trait(Spec.UAC, "29")]
    [Then("it Should Work With Complex Scenario")]
    [Fact]
    public void ItShouldWorkWithComplexScenario()
    {
        ArrangeAndAct();
        // Arrange - Create parent context with services and variables
        var parent = new Core.ExecutionContext();
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

    [Trait(Spec.SPEC, "3303")]
    [Trait(Spec.SC, "01")]
    [UserStory("01", "Execution context manages variables and state")]
    private class TestService : ITemplateService
    {
        public string Name => "testservice";
        public object? Transform(object? input) => $"TEST:{input}";
    }

    [Trait(Spec.SPEC, "3303")]
    [Trait(Spec.SC, "01")]
    [UserStory("01", "Execution context manages variables and state")]
    private class AnotherTestService : ITemplateService
    {
        public string Name => "another";
        public object? Transform(object? input) => $"ANOTHER:{input}";
    }

    #endregion
}
