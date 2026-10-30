namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US02.VariableResolution;

/// <summary>Public data shapes the resolver scenarios navigate; executable usage stays in the scenarios.</summary>
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
    public Func<string, string> MyDelegate { get; set; } =
        input => $"Delegate called with: {input}";
}

public class TestClassWithProperties
{
    public string StringProperty { get; set; } = "";
    public int IntProperty { get; set; }
}
