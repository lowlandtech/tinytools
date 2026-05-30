namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US02.VariableResolution;

/// <summary>
/// Comprehensive tests for VariableResolver features.
/// Covers all public methods and untested use cases.
/// </summary>
[Trait(Spec.SPEC, "3303")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Variable resolver interpolates template expressions")]
public class WhenUsingVariableResolverTest : TinyToolsScenario<VariableResolver>
{
    private ToolContext _context = null!;

    protected override VariableResolver For()
    {
        return new VariableResolver();
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

    #region ResolveString Tests

    [Trait(Spec.UAC, "01")]
    [Then("it Should Resolve Simple Variable")]
    [Fact]
    public void ItShouldResolveSimpleVariable()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Name", "John");
        var input = "Hello, ${Context.Name}!";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("Hello, John!");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Handle Null Input")]
    [Fact]
    public void ItShouldHandleNullInput()
    {
        ArrangeAndAct();
        // Arrange
        string? input = null;

        // Act
        var result = Sut.ResolveString(input!, _context);

        // Assert
        result.Should().BeNull();
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Handle Empty Input")]
    [Fact]
    public void ItShouldHandleEmptyInput()
    {
        ArrangeAndAct();
        // Arrange
        var input = "";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Resolve Multiple Variables")]
    [Fact]
    public void ItShouldResolveMultipleVariables()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("FirstName", "John");
        _context.Set("LastName", "Doe");
        var input = "${Context.FirstName} ${Context.LastName}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("John Doe");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Resolve Nested Properties")]
    [Fact]
    public void ItShouldResolveNestedProperties()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "06")]
    [Then("it Should Handle Null Coalescing With Null Value")]
    [Fact]
    public void ItShouldHandleNullCoalescingWithNullValue()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Name", null);
        var input = "${Context.Name ?? \"Default\"}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("Default");
    }

    [Trait(Spec.UAC, "07")]
    [Then("it Should Handle Null Coalescing With Empty String")]
    [Fact]
    public void ItShouldHandleNullCoalescingWithEmptyString()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Name", "");
        var input = "${Context.Name ?? \"Default\"}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("Default");
    }

    [Trait(Spec.UAC, "08")]
    [Then("it Should Handle Null Coalescing With Value")]
    [Fact]
    public void ItShouldHandleNullCoalescingWithValue()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Name", "John");
        var input = "${Context.Name ?? \"Default\"}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("John");
    }

    [Trait(Spec.UAC, "09")]
    [Then("it Should Handle Null Coalescing With Single Quotes")]
    [Fact]
    public void ItShouldHandleNullCoalescingWithSingleQuotes()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Name", null);
        var input = "${Context.Name ?? 'Default'}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("Default");
    }

    [Trait(Spec.UAC, "10")]
    [Then("it Should Handle Null Coalescing Without Quotes")]
    [Fact]
    public void ItShouldHandleNullCoalescingWithoutQuotes()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "11")]
    [Then("it Should Apply Single Pipe Helper")]
    [Fact]
    public void ItShouldApplySinglePipeHelper()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Name", "john");
        var input = "${Context.Name | upper}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("JOHN");
    }

    [Trait(Spec.UAC, "12")]
    [Then("it Should Apply Multiple Pipe Helpers")]
    [Fact]
    public void ItShouldApplyMultiplePipeHelpers()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Text", "  hello world  ");
        var input = "${Context.Text | trim | upper}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("HELLO WORLD");
    }

    [Trait(Spec.UAC, "13")]
    [Then("it Should Apply Pipe Helper With Argument")]
    [Fact]
    public void ItShouldApplyPipeHelperWithArgument()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "14")]
    [Then("it Should Resolve Context Get With Double Quotes")]
    [Fact]
    public void ItShouldResolveContextGetWithDoubleQuotes()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("UserName", "John");
        var input = "${Context.Get(\"UserName\")}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("John");
    }

    [Trait(Spec.UAC, "15")]
    [Then("it Should Resolve Context Get With Single Quotes")]
    [Fact]
    public void ItShouldResolveContextGetWithSingleQuotes()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "16")]
    [Then("it Should Access String Dictionary Property")]
    [Fact]
    public void ItShouldAccessStringDictionaryProperty()
    {
        ArrangeAndAct();
        // Arrange
        var dict = new Dictionary<string, object?> { ["Name"] = "John", ["Age"] = 30 };
        _context.Set("Data", dict);
        var input = "${Context.Data.Name}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("John");
    }

    [Trait(Spec.UAC, "17")]
    [Then("it Should Access Object Dictionary Property")]
    [Fact]
    public void ItShouldAccessObjectDictionaryProperty()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "18")]
    [Then("it Should Resolve Expression With Context Prefix")]
    [Fact]
    public void ItShouldResolveExpressionWithContextPrefix()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Name", "John");

        // Act
        var result = Sut.ResolveExpression("Context.Name", _context);

        // Assert
        result.Should().Be("John");
    }

    [Trait(Spec.UAC, "19")]
    [Then("it Should Resolve Expression Without Context Prefix")]
    [Fact]
    public void ItShouldResolveExpressionWithoutContextPrefix()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Name", "Jane");

        // Act
        var result = Sut.ResolveExpression("Name", _context);

        // Assert
        result.Should().Be("Jane");
    }

    [Trait(Spec.UAC, "20")]
    [Then("it Should Resolve Nested Property Expression")]
    [Fact]
    public void ItShouldResolveNestedPropertyExpression()
    {
        ArrangeAndAct();
        // Arrange
        var person = new { Name = "John", Details = new { Age = 30 } };
        _context.Set("Person", person);

        // Act
        var result = Sut.ResolveExpression("Context.Person.Details.Age", _context);

        // Assert
        result.Should().Be(30);
    }

    [Trait(Spec.UAC, "21")]
    [Then("it Should Return Null For Non Existent Expression")]
    [Fact]
    public void ItShouldReturnNullForNonExistentExpression()
    {
        ArrangeAndAct();
        // Arrange
        // No variable set

        // Act
        var result = Sut.ResolveExpression("Context.NonExistent", _context);

        // Assert
        result.Should().BeNull();
    }

    [Trait(Spec.UAC, "22")]
    [Then("it Should Resolve Complex Object")]
    [Fact]
    public void ItShouldResolveComplexObject()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "23")]
    [Then("it Should Resolve Inputs Dictionary")]
    [Fact]
    public void ItShouldResolveInputsDictionary()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "24")]
    [Then("it Should Resolve Empty Inputs Dictionary")]
    [Fact]
    public void ItShouldResolveEmptyInputsDictionary()
    {
        ArrangeAndAct();
        // Arrange
        var inputs = new Dictionary<string, object?>();

        // Act
        var result = Sut.ResolveInputs(inputs, _context);

        // Assert
        result.Should().BeEmpty();
    }

    [Trait(Spec.UAC, "25")]
    [Then("it Should Handle Mixed Input Types")]
    [Fact]
    public void ItShouldHandleMixedInputTypes()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "26")]
    [Then("it Should Resolve Null Value")]
    [Fact]
    public void ItShouldResolveNullValue()
    {
        ArrangeAndAct();
        // Arrange
        object? value = null;

        // Act
        var result = Sut.ResolveValue(value, _context);

        // Assert
        result.Should().BeNull();
    }

    [Trait(Spec.UAC, "27")]
    [Then("it Should Resolve String With Variable")]
    [Fact]
    public void ItShouldResolveStringWithVariable()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Name", "John");
        var value = "Hello, ${Context.Name}!";

        // Act
        var result = Sut.ResolveValue(value, _context);

        // Assert
        result.Should().Be("Hello, John!");
    }

    [Trait(Spec.UAC, "28")]
    [Then("it Should Resolve String With Single Expression")]
    [Fact]
    public void ItShouldResolveStringWithSingleExpression()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Name", "John");
        var value = "${Context.Name}";

        // Act
        var result = Sut.ResolveValue(value, _context);

        // Assert
        result.Should().Be("John");
    }

    [Trait(Spec.UAC, "29")]
    [Then("it Should Resolve Plain String")]
    [Fact]
    public void ItShouldResolvePlainString()
    {
        ArrangeAndAct();
        // Arrange
        var value = "Plain text";

        // Act
        var result = Sut.ResolveValue(value, _context);

        // Assert
        result.Should().Be("Plain text");
    }

    [Trait(Spec.UAC, "30")]
    [Then("it Should Resolve List Of Values")]
    [Fact]
    public void ItShouldResolveListOfValues()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "31")]
    [Then("it Should Resolve Nested Dictionary")]
    [Fact]
    public void ItShouldResolveNestedDictionary()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "32")]
    [Then("it Should Resolve Object Dictionary")]
    [Fact]
    public void ItShouldResolveObjectDictionary()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "33")]
    [Then("it Should Pass Through Other Types")]
    [Fact]
    public void ItShouldPassThroughOtherTypes()
    {
        ArrangeAndAct();
        // Arrange
        var value = 42;

        // Act
        var result = Sut.ResolveValue(value, _context);

        // Assert
        result.Should().Be(42);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Trait(Spec.UAC, "34")]
    [Then("it Should Handle Null Property Access")]
    [Fact]
    public void ItShouldHandleNullPropertyAccess()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Object", null);
        var input = "${Context.Object.Property}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("");
    }

    [Trait(Spec.UAC, "35")]
    [Then("it Should Handle Missing Property")]
    [Fact]
    public void ItShouldHandleMissingProperty()
    {
        ArrangeAndAct();
        // Arrange
        var obj = new { Name = "John" };
        _context.Set("Object", obj);
        var input = "${Context.Object.NonExistent}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("");
    }

    [Trait(Spec.UAC, "36")]
    [Then("it Should Not Replace Empty Expression")]
    [Fact]
    public void ItShouldNotReplaceEmptyExpression()
    {
        ArrangeAndAct();
        // Arrange
        var input = "${}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert - Empty expressions are not valid and should not be replaced
        result.Should().Be("${}");
    }

    [Trait(Spec.UAC, "37")]
    [Then("it Should Handle Case Insensitive Property Access")]
    [Fact]
    public void ItShouldHandleCaseInsensitivePropertyAccess()
    {
        ArrangeAndAct();
        // Arrange
        var obj = new { Name = "John" };
        _context.Set("Object", obj);
        var input = "${Context.Object.name}"; // lowercase 'name'

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("John");
    }

    [Trait(Spec.UAC, "38")]
    [Then("it Should Handle Field Access")]
    [Fact]
    public void ItShouldHandleFieldAccess()
    {
        ArrangeAndAct();
        // Arrange
        var obj = new TestClassWithField { PublicField = "FieldValue" };
        _context.Set("Object", obj);
        var input = "${Context.Object.PublicField}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert
        result.Should().Be("FieldValue");
    }

    [Trait(Spec.UAC, "39")]
    [Then("it Should Handle Complex Nested Path")]
    [Fact]
    public void ItShouldHandleComplexNestedPath()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "40")]
    [Then("it Should Handle Numeric Values")]
    [Fact]
    public void ItShouldHandleNumericValues()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "41")]
    [Then("it Should Handle Boolean Values")]
    [Fact]
    public void ItShouldHandleBooleanValues()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "42")]
    [Then("it Should Handle Complex Scenario With All Features")]
    [Fact]
    public void ItShouldHandleComplexScenarioWithAllFeatures()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "43")]
    [Then("it Should Handle Null Coalescing Combined With Pipe Helpers")]
    [Fact]
    public void ItShouldHandleNullCoalescingCombinedWithPipeHelpers()
    {
        ArrangeAndAct();
        // Arrange
        _context.Set("Name", null);
        // When ?? and | are both present, pipes take precedence
        var input = "${Context.Name | upper ?? \"Default\"}";

        // Act
        var result = Sut.ResolveString(input, _context);

        // Assert - The pipe will process the null value, then ?? won't apply
        result.Should().Be("");
    }

    [Trait(Spec.UAC, "44")]
    [Then("it Should Handle Delegate Invocation Failure Gracefully")]
    [Fact]
    public void ItShouldHandleDelegateInvocationFailureGracefully()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "45")]
    [Then("it Should Handle Multiple Properties After Method Call")]
    [Fact]
    public void ItShouldHandleMultiplePropertiesAfterMethodCall()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "46")]
    [Then("it Should Invoke Delegate Property When Method Not Found")]
    [Fact]
    public void ItShouldInvokeDelegatePropertyWhenMethodNotFound()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "47")]
    [Then("it Should Access Property Value Through Reflection")]
    [Fact]
    public void ItShouldAccessPropertyValueThroughReflection()
    {
        ArrangeAndAct();
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

    [Trait(Spec.SPEC, "3303")]
    [Trait(Spec.SC, "01")]
    [UserStory("02", "Variable resolver interpolates template expressions")]
    public class TestClassWithField
    {
        public string PublicField = "";
    }

    [Trait(Spec.SPEC, "3303")]
    [Trait(Spec.SC, "01")]
    [UserStory("02", "Variable resolver interpolates template expressions")]
    public class TestClassWithThrowingMethod
    {
        public string ThrowingMethod(string arg)
        {
            throw new InvalidOperationException("This method always throws");
        }
    }

    [Trait(Spec.SPEC, "3303")]
    [Trait(Spec.SC, "01")]
    [UserStory("02", "Variable resolver interpolates template expressions")]
    public class TestServiceReturningObject
    {
        public TestReturnedObject GetObject()
        {
            return new TestReturnedObject { Name = "TestObject" };
        }
    }

    [Trait(Spec.SPEC, "3303")]
    [Trait(Spec.SC, "01")]
    [UserStory("02", "Variable resolver interpolates template expressions")]
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

    [Trait(Spec.SPEC, "3303")]
    [Trait(Spec.SC, "01")]
    [UserStory("02", "Variable resolver interpolates template expressions")]
    public class TestReturnedObject
    {
        public string Name { get; set; } = "";
    }

    [Trait(Spec.SPEC, "3303")]
    [Trait(Spec.SC, "01")]
    [UserStory("02", "Variable resolver interpolates template expressions")]
    public class TestNestedReturnedObject
    {
        public TestLevel1 Level1 { get; set; } = null!;
    }

    [Trait(Spec.SPEC, "3303")]
    [Trait(Spec.SC, "01")]
    [UserStory("02", "Variable resolver interpolates template expressions")]
    public class TestLevel1
    {
        public TestLevel2 Level2 { get; set; } = null!;
    }

    [Trait(Spec.SPEC, "3303")]
    [Trait(Spec.SC, "01")]
    [UserStory("02", "Variable resolver interpolates template expressions")]
    public class TestLevel2
    {
        public string Value { get; set; } = "";
    }

    [Trait(Spec.SPEC, "3303")]
    [Trait(Spec.SC, "01")]
    [UserStory("02", "Variable resolver interpolates template expressions")]
    public class TestClassWithDelegateProperty
    {
        // This is a property that holds a delegate, not a method
        public Func<string, string> MyDelegate { get; set; } = 
            input => $"Delegate called with: {input}";
    }

    [Trait(Spec.SPEC, "3303")]
    [Trait(Spec.SC, "01")]
    [UserStory("02", "Variable resolver interpolates template expressions")]
    public class TestClassWithProperties
    {
        public string StringProperty { get; set; } = "";
        public int IntProperty { get; set; }
    }

    #endregion
}
