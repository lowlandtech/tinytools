namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Comprehensive tests for VariableResolver features.
/// Covers all public methods and untested use cases.
/// </summary>
public class WhenUsingVariableResolverTest : WhenTestingFor<VariableResolver>
{
    private ExecutionContext _context = null!;

    protected override VariableResolver For()
    {
        return new VariableResolver();
    }

    protected override void Given()
    {
        _context = new ExecutionContext();
    }

    protected override void When()
    {
        // When is handled in each test
    }

    #region ResolveString Tests

    [Fact]
    public void ItShouldResolveSimpleVariable()
    {
        // Arrange
        _context.Set("Name", "John");
        var input = "Hello, ${Context.Name}!";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("Hello, John!");
    }

    [Fact]
    public void ItShouldHandleNullInput()
    {
        // Arrange
        string? input = null;

        // Act
        var result = Sut.ResolveString(input!, _context);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ItShouldHandleEmptyInput()
    {
        // Arrange
        var input = "";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("");
    }

    [Fact]
    public void ItShouldResolveMultipleVariables()
    {
        // Arrange
        _context.Set("FirstName", "John");
        _context.Set("LastName", "Doe");
        var input = "${Context.FirstName} ${Context.LastName}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("John Doe");
    }

    [Fact]
    public void ItShouldResolveNestedProperties()
    {
        // Arrange
        var person = new { Name = "John", Address = new { City = "Seattle" } };
        _context.Set("Person", person);
        var input = "${Context.Person.Address.City}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("Seattle");
    }

    #endregion

    #region Null Coalescing Tests

    [Fact]
    public void ItShouldHandleNullCoalescingWithNullValue()
    {
        // Arrange
        _context.Set("Name", null);
        var input = "${Context.Name ?? \"Default\"}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("Default");
    }

    [Fact]
    public void ItShouldHandleNullCoalescingWithEmptyString()
    {
        // Arrange
        _context.Set("Name", "");
        var input = "${Context.Name ?? \"Default\"}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("Default");
    }

    [Fact]
    public void ItShouldHandleNullCoalescingWithValue()
    {
        // Arrange
        _context.Set("Name", "John");
        var input = "${Context.Name ?? \"Default\"}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("John");
    }

    [Fact]
    public void ItShouldHandleNullCoalescingWithSingleQuotes()
    {
        // Arrange
        _context.Set("Name", null);
        var input = "${Context.Name ?? 'Default'}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("Default");
    }

    [Fact]
    public void ItShouldHandleNullCoalescingWithoutQuotes()
    {
        // Arrange
        _context.Set("Name", null);
        var input = "${Context.Name ?? Default}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("Default");
    }

    #endregion

    #region Pipe Helper Tests

    [Fact]
    public void ItShouldApplySinglePipeHelper()
    {
        // Arrange
        _context.Set("Name", "john");
        var input = "${Context.Name | upper}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("JOHN");
    }

    [Fact]
    public void ItShouldApplyMultiplePipeHelpers()
    {
        // Arrange
        _context.Set("Text", "  hello world  ");
        var input = "${Context.Text | trim | upper}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("HELLO WORLD");
    }

    [Fact]
    public void ItShouldApplyPipeHelperWithArgument()
    {
        // Arrange
        _context.Set("Text", "Hello World");
        var input = "${Context.Text | truncate:5}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert - Truncate adds "..." when truncating
        result.Should().Be("He...");
    }

    #endregion

    #region Context.Get() Syntax Tests

    [Fact]
    public void ItShouldResolveContextGetWithDoubleQuotes()
    {
        // Arrange
        _context.Set("UserName", "John");
        var input = "${Context.Get(\"UserName\")}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("John");
    }

    [Fact]
    public void ItShouldResolveContextGetWithSingleQuotes()
    {
        // Arrange
        _context.Set("UserName", "Jane");
        var input = "${Context.Get('UserName')}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("Jane");
    }

    #endregion

    #region Dictionary Property Access Tests

    [Fact]
    public void ItShouldAccessStringDictionaryProperty()
    {
        // Arrange
        var dict = new Dictionary<string, object?> { ["Name"] = "John", ["Age"] = 30 };
        _context.Set("Data", dict);
        var input = "${Context.Data.Name}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("John");
    }

    [Fact]
    public void ItShouldAccessObjectDictionaryProperty()
    {
        // Arrange
        var dict = new Dictionary<object, object?> { ["City"] = "Seattle" };
        _context.Set("Data", dict);
        var input = "${Context.Data.City}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("Seattle");
    }

    #endregion

    #region ResolveExpression Tests

    [Fact]
    public void ItShouldResolveExpressionWithContextPrefix()
    {
        // Arrange
        _context.Set("Name", "John");

        // Act
        var result = Sut.ResolveExpression("Context.Name", _context);

        // Assert
        result.Should().Be("John");
    }

    [Fact]
    public void ItShouldResolveExpressionWithoutContextPrefix()
    {
        // Arrange
        _context.Set("Name", "Jane");

        // Act
        var result = Sut.ResolveExpression("Name", _context);

        // Assert
        result.Should().Be("Jane");
    }

    [Fact]
    public void ItShouldResolveNestedPropertyExpression()
    {
        // Arrange
        var person = new { Name = "John", Details = new { Age = 30 } };
        _context.Set("Person", person);

        // Act
        var result = Sut.ResolveExpression("Context.Person.Details.Age", _context);

        // Assert
        result.Should().Be(30);
    }

    [Fact]
    public void ItShouldReturnNullForNonExistentExpression()
    {
        // Arrange
        // No variable set

        // Act
        var result = Sut.ResolveExpression("Context.NonExistent", _context);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ItShouldResolveComplexObject()
    {
        // Arrange
        var customer = new 
        { 
            Name = "ACME Corp",
            Contact = new { Email = "info@acme.com", Phone = "555-1234" }
        };
        _context.Set("Customer", customer);

        // Act
        var result = Sut.ResolveExpression("Customer.Contact.Email", _context);

        // Assert
        result.Should().Be("info@acme.com");
    }

    #endregion

    #region ResolveInputs Tests

    [Fact]
    public void ItShouldResolveInputsDictionary()
    {
        // Arrange
        _context.Set("Name", "John");
        _context.Set("Age", 30);
        
        var inputs = new Dictionary<string, object?>
        {
            ["Greeting"] = "Hello, ${Context.Name}",
            ["Info"] = "Age: ${Context.Age}"
        };

        // Act
        var result = Sut.ResolveInputs(inputs, _context);

        // Assert
        result["Greeting"].Should().Be("Hello, John");
        result["Info"].Should().Be("Age: 30");
    }

    [Fact]
    public void ItShouldResolveEmptyInputsDictionary()
    {
        // Arrange
        var inputs = new Dictionary<string, object?>();

        // Act
        var result = Sut.ResolveInputs(inputs, _context);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ItShouldHandleMixedInputTypes()
    {
        // Arrange
        _context.Set("Name", "John");
        
        var inputs = new Dictionary<string, object?>
        {
            ["StringValue"] = "Hello, ${Context.Name}",
            ["IntValue"] = 42,
            ["NullValue"] = null
        };

        // Act
        var result = Sut.ResolveInputs(inputs, _context);

        // Assert
        result["StringValue"].Should().Be("Hello, John");
        result["IntValue"].Should().Be(42);
        result["NullValue"].Should().BeNull();
    }

    #endregion

    #region ResolveValue Tests

    [Fact]
    public void ItShouldResolveNullValue()
    {
        // Arrange
        object? value = null;

        // Act
        var result = Sut.ResolveValue(value, _context);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ItShouldResolveStringWithVariable()
    {
        // Arrange
        _context.Set("Name", "John");
        var value = "Hello, ${Context.Name}!";

        // Act
        var result = Sut.ResolveValue(value, _context);

        // Assert
        result.Should().Be("Hello, John!");
    }

    [Fact]
    public void ItShouldResolveStringWithSingleExpression()
    {
        // Arrange
        _context.Set("Name", "John");
        var value = "${Context.Name}";

        // Act
        var result = Sut.ResolveValue(value, _context);

        // Assert
        result.Should().Be("John");
    }

    [Fact]
    public void ItShouldResolvePlainString()
    {
        // Arrange
        var value = "Plain text";

        // Act
        var result = Sut.ResolveValue(value, _context);

        // Assert
        result.Should().Be("Plain text");
    }

    [Fact]
    public void ItShouldResolveListOfValues()
    {
        // Arrange
        _context.Set("Prefix", "Mr.");
        var list = new List<object> 
        { 
            "${Context.Prefix} John",
            "${Context.Prefix} Jane",
            42
        };

        // Act
        var result = Sut.ResolveValue(list, _context) as List<object>;

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result![0].Should().Be("Mr. John");
        result[1].Should().Be("Mr. Jane");
        result[2].Should().Be(42);
    }

    [Fact]
    public void ItShouldResolveNestedDictionary()
    {
        // Arrange
        _context.Set("Name", "John");
        var dict = new Dictionary<string, object?>
        {
            ["Greeting"] = "${Context.Name}"
        };

        // Act
        var result = Sut.ResolveValue(dict, _context) as Dictionary<string, object?>;

        // Assert
        result.Should().NotBeNull();
        result!["Greeting"].Should().Be("John");
    }

    [Fact]
    public void ItShouldResolveObjectDictionary()
    {
        // Arrange
        _context.Set("Value", "Test");
        var dict = new Dictionary<object, object?>
        {
            ["Key"] = "${Context.Value}"
        };

        // Act
        var result = Sut.ResolveValue(dict, _context) as Dictionary<string, object?>;

        // Assert
        result.Should().NotBeNull();
        result!["Key"].Should().Be("Test");
    }

    [Fact]
    public void ItShouldPassThroughOtherTypes()
    {
        // Arrange
        var value = 42;

        // Act
        var result = Sut.ResolveValue(value, _context);

        // Assert
        result.Should().Be(42);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void ItShouldHandleNullPropertyAccess()
    {
        // Arrange
        _context.Set("Object", null);
        var input = "${Context.Object.Property}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("");
    }

    [Fact]
    public void ItShouldHandleMissingProperty()
    {
        // Arrange
        var obj = new { Name = "John" };
        _context.Set("Object", obj);
        var input = "${Context.Object.NonExistent}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("");
    }

    [Fact]
    public void ItShouldNotReplaceEmptyExpression()
    {
        // Arrange
        var input = "${}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert - Empty expressions are not valid and should not be replaced
        result.Should().Be("${}");
    }

    [Fact]
    public void ItShouldHandleCaseInsensitivePropertyAccess()
    {
        // Arrange
        var obj = new { Name = "John" };
        _context.Set("Object", obj);
        var input = "${Context.Object.name}"; // lowercase 'name'

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("John");
    }

    [Fact]
    public void ItShouldHandleFieldAccess()
    {
        // Arrange
        var obj = new TestClassWithField { PublicField = "FieldValue" };
        _context.Set("Object", obj);
        var input = "${Context.Object.PublicField}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("FieldValue");
    }

    [Fact]
    public void ItShouldHandleComplexNestedPath()
    {
        // Arrange
        var root = new 
        { 
            Level1 = new 
            { 
                Level2 = new 
                { 
                    Level3 = new 
                    { 
                        Value = "DeepValue" 
                    } 
                } 
            } 
        };
        _context.Set("Root", root);
        var input = "${Context.Root.Level1.Level2.Level3.Value}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("DeepValue");
    }

    [Fact]
    public void ItShouldHandleNumericValues()
    {
        // Arrange
        _context.Set("Count", 42);
        _context.Set("Price", 19.99);
        var input = "Count: ${Context.Count}, Price: ${Context.Price}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert - Should contain the values (culture-invariant check)
        result.Should().Contain("Count: 42");
        result.Should().Contain("Price:");
        result.Should().Contain("19");
    }

    [Fact]
    public void ItShouldHandleBooleanValues()
    {
        // Arrange
        _context.Set("IsActive", true);
        _context.Set("IsDeleted", false);
        var input = "Active: ${Context.IsActive}, Deleted: ${Context.IsDeleted}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("Active: True, Deleted: False");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ItShouldHandleComplexScenarioWithAllFeatures()
    {
        // Arrange
        var customer = new 
        { 
            FirstName = "john",
            LastName = "doe",
            Email = "john.doe@example.com",
            IsActive = true
        };
        _context.Set("Customer", customer);
        _context.Set("CompanyName", null);
        
        var input = "Welcome ${Context.Customer.FirstName | upper} ${Context.Customer.LastName | upper}! " +
                   "Email: ${Context.Customer.Email} " +
                   "Company: ${Context.CompanyName ?? \"Not Specified\"}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Contain("Welcome JOHN DOE!");
        result.Should().Contain("Email: john.doe@example.com");
        result.Should().Contain("Company: Not Specified");
    }

    #endregion

    #region Additional Edge Cases

    [Fact]
    public void ItShouldHandleNullCoalescingCombinedWithPipeHelpers()
    {
        // Arrange
        _context.Set("Name", null);
        // When ?? and | are both present, pipes take precedence
        var input = "${Context.Name | upper ?? \"Default\"}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert - The pipe will process the null value, then ?? won't apply
        result.Should().Be("");
    }

    [Fact]
    public void ItShouldHandleDelegateInvocationFailureGracefully()
    {
        // Arrange
        // Register a service that will throw when invoked with wrong type
        _context.RegisterService("test", input => 
        {
            // This will throw if input is not a number
            return int.Parse(input?.ToString() ?? "0") * 2;
        });
        
        var input = "${Context.Services('test')('not-a-number')}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert - Should return empty string when delegate throws
        result.Should().Be("");
    }

    [Fact]
    public void ItShouldHandleMultiplePropertiesAfterMethodCall()
    {
        // Arrange
        var service = new TestServiceReturningNestedObject();
        _context.RegisterService("getNested", input => service.GetNestedObject());
        
        // Access nested properties: method().Property1.Property2.Property3
        // This specifically tests the remaining property path navigation code
        var input = "${Context.Services('getNested')('test').Level1.Level2.Value}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert - Should navigate multiple property levels after method invocation
        result.Should().Be("DeepValue");
    }

    [Fact]
    public void ItShouldInvokeDelegatePropertyWhenMethodNotFound()
    {
        // Arrange
        var obj = new TestClassWithDelegateProperty();
        _context.Set("Object", obj);
        
        // Call MyDelegate('test') - there's no MyDelegate method, but there IS a MyDelegate property
        // This tests the fallback path in InvokeMethodOrProperty
        var input = "${Context.Object.MyDelegate('test')}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert - Should invoke the delegate property
        result.Should().Be("Delegate called with: test");
    }

    [Fact]
    public void ItShouldAccessPropertyValueThroughReflection()
    {
        // Arrange
        var obj = new TestClassWithProperties 
        { 
            StringProperty = "PropertyValue",
            IntProperty = 42
        };
        _context.Set("Object", obj);
        
        // This tests GetPropertyOrMethodValue being called to access a property
        var input = "${Context.Object.StringProperty}-${Context.Object.IntProperty}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert - Should get property values via reflection
        result.Should().Be("PropertyValue-42");
    }

    #endregion

    #region Helper Classes

    public class TestClassWithField
    {
        public string PublicField = "";
    }

    public class TestClassWithThrowingMethod
    {
        public string ThrowingMethod(string arg)
        {
            throw new InvalidOperationException("This method always throws");
        }
    }

    public class TestServiceReturningObject
    {
        public TestReturnedObject GetObject()
        {
            return new TestReturnedObject { Name = "TestObject" };
        }
    }

    public class TestServiceReturningNestedObject
    {
        public TestNestedReturnedObject GetNestedObject()
        {
            return new TestNestedReturnedObject 
            { 
                Level1 = new TestLevel1 
                { 
                    Level2 = new TestLevel2 
                    { 
                        Value = "DeepValue" 
                    } 
                } 
            };
        }
    }

    public class TestReturnedObject
    {
        public string Name { get; set; } = "";
    }

    public class TestNestedReturnedObject
    {
        public TestLevel1 Level1 { get; set; } = null!;
    }

    public class TestLevel1
    {
        public TestLevel2 Level2 { get; set; } = null!;
    }

    public class TestLevel2
    {
        public string Value { get; set; } = "";
    }

    public class TestClassWithDelegateProperty
    {
        // This is a property that holds a delegate, not a method
        public Func<string, string> MyDelegate { get; set; } = 
            input => $"Delegate called with: {input}";
    }

    public class TestClassWithProperties
    {
        public string StringProperty { get; set; } = "";
        public int IntProperty { get; set; }
    }

    #endregion
}
