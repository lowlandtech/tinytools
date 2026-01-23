using System.Reflection;

namespace LowlandTech.Specs;

/// <summary>
/// Provides auto-execution capabilities for BDD-style tests,
/// reading attributes and applying preconditions and validations automatically.
/// </summary>
public static class BddAutoExecutor
{
    /// <summary>
    /// Validates a test instance against its [Then] attribute specifications.
    /// </summary>
    public static void AutoValidate(object testInstance, string methodName)
    {
        Guard.Against.Null(testInstance);
        Guard.Against.NullOrEmpty(methodName);

        var method = testInstance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        if (method == null)
            throw new InvalidOperationException($"Method '{methodName}' not found on type '{testInstance.GetType().Name}'");

        var thenAttr = method.GetCustomAttribute<ThenAttribute>();
        if (thenAttr == null)
            return; // No auto-validation needed

        ValidateThenAssertion(testInstance, thenAttr);
    }

    /// <summary>
    /// Applies all [Given] preconditions to the test instance state.
    /// </summary>
    public static void AutoApplyGivens(object testInstance)
    {
        Guard.Against.Null(testInstance);

        var givenMethod = testInstance.GetType().GetMethod("Given", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (givenMethod == null)
            return;

        var givenAttrs = givenMethod.GetCustomAttributes<GivenAttribute>().ToList();
        if (givenAttrs.Count == 0)
            return;

        // Get the state object - look for common field/property names
        var state = GetStateFromInstance(testInstance);
        if (state == null)
            return;

        foreach (var given in givenAttrs)
        {
            ApplyGivenCondition(state, given);
        }
    }

    /// <summary>
    /// Retrieves state information from [For] attribute.
    /// </summary>
    public static Type? GetStateType(object testInstance)
    {
        Guard.Against.Null(testInstance);

        var forMethod = testInstance.GetType().GetMethod("For", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (forMethod == null)
            return null;

        var forAttr = forMethod.GetCustomAttribute<ForAttribute>();
        return forAttr?.StateType;
    }

    private static void ValidateThenAssertion(object testInstance, ThenAttribute thenAttr)
    {
        if (string.IsNullOrWhiteSpace(thenAttr.Property))
            return; // No structured assertion to auto-validate

        var state = GetStateFromInstance(testInstance);
        if (state == null)
            throw new InvalidOperationException("Cannot auto-validate: no state object found. Expected field/property named '_state' or 'State'.");

        // Main assertion
        ValidateCondition(state, thenAttr.Property, thenAttr.Operator, thenAttr.Expected, "Then");

        // Postcondition
        if (!string.IsNullOrWhiteSpace(thenAttr.PostconditionProperty))
        {
            ValidateCondition(state, thenAttr.PostconditionProperty, thenAttr.PostconditionOperator, thenAttr.PostconditionExpected, "And");
        }
    }

    private static void ApplyGivenCondition(object state, GivenAttribute given)
    {
        if (string.IsNullOrWhiteSpace(given.Property) || given.Value == null)
            return;

        var prop = state.GetType().GetProperty(given.Property, BindingFlags.Public | BindingFlags.Instance);
        if (prop != null && prop.CanWrite)
        {
            var convertedValue = ConvertValue(given.Value, prop.PropertyType);
            prop.SetValue(state, convertedValue);
            return;
        }

        var field = state.GetType().GetField(given.Property, BindingFlags.Public | BindingFlags.Instance);
        if (field != null)
        {
            var convertedValue = ConvertValue(given.Value, field.FieldType);
            field.SetValue(state, convertedValue);
        }
    }

    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value == null)
            return null;

        if (targetType.IsAssignableFrom(value.GetType()))
            return value;

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Special handling for decimal from int
        if (underlyingType == typeof(decimal) && value is int intValue)
            return (decimal)intValue;

        return Convert.ChangeType(value, underlyingType);
    }

    private static void ValidateCondition(object state, string? property, string? operatorName, object? expected, string clauseType)
    {
        if (string.IsNullOrWhiteSpace(property))
            return;

        var actualValue = GetPropertyValue(state, property);
        var op = operatorName ?? "Equals";

        // Convert expected to match actual type if needed
        if (actualValue != null && expected != null && actualValue.GetType() != expected.GetType())
        {
            expected = ConvertValue(expected, actualValue.GetType());
        }

        var isValid = op switch
        {
            "Equals" => Equals(actualValue, expected),
            "NotEquals" => !Equals(actualValue, expected),
            "GreaterThan" => Compare(actualValue, expected) > 0,
            "GreaterThanOrEquals" => Compare(actualValue, expected) >= 0,
            "LessThan" => Compare(actualValue, expected) < 0,
            "LessThanOrEquals" => Compare(actualValue, expected) <= 0,
            "Contains" => actualValue?.ToString()?.Contains(expected?.ToString() ?? "") ?? false,
            "IsNull" => actualValue == null,
            "IsNotNull" => actualValue != null,
            _ => throw new NotSupportedException($"Operator '{op}' is not supported")
        };

        if (!isValid)
        {
            throw new InvalidOperationException(
                $"{clauseType} assertion failed: {property} {op} {expected} (actual: {actualValue})");
        }
    }

    private static object? GetPropertyValue(object obj, string propertyName)
    {
        var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop != null)
            return prop.GetValue(obj);

        var field = obj.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return field?.GetValue(obj);
    }

    private static object? GetStateFromInstance(object testInstance)
    {
        // Try common field names
        var stateField = testInstance.GetType().GetField("_state", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (stateField != null)
            return stateField.GetValue(testInstance);

        var stateProp = testInstance.GetType().GetProperty("State", BindingFlags.Public | BindingFlags.Instance);
        if (stateProp != null)
            return stateProp.GetValue(testInstance);

        // Try to find first field that matches StateType from [For]
        var stateType = GetStateType(testInstance);
        if (stateType != null)
        {
            var matchingField = testInstance.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(f => f.FieldType == stateType);
            
            if (matchingField != null)
                return matchingField.GetValue(testInstance);
        }

        return null;
    }

    private static int Compare(object? left, object? right)
    {
        if (left is IComparable comparable && right != null)
            return comparable.CompareTo(right);
        
        throw new InvalidOperationException($"Cannot compare {left?.GetType().Name} with {right?.GetType().Name}");
    }
}
