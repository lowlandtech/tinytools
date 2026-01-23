using System.Text.Json;
using LowlandTech.Specs;

namespace LowlandTech.Specs.UI.Components;

public class TestExecutionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object?> InitialState { get; set; } = new();
    public Dictionary<string, object?> FinalState { get; set; } = new();
    public List<AssertionResult> Assertions { get; set; } = new();
}

public class AssertionResult
{
    public string Description { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public object? Expected { get; set; }
    public object? Actual { get; set; }
}

public class TestRunner
{
    public static TestExecutionResult ExecuteScenario(ScenarioInfo scenario, string jsonData, string? actionData = null, Dictionary<string, string>? customExpectedValues = null)
    {
        var result = new TestExecutionResult();
        
        try
        {
            // Parse JSON into state object
            var stateData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData);
            if (stateData == null)
            {
                result.Success = false;
                result.ErrorMessage = "Invalid JSON format";
                return result;
            }

            // Create initial state from JSON
            var initialState = new Dictionary<string, object?>();
            foreach (var kvp in stateData)
            {
                initialState[kvp.Key] = ConvertJsonElement(kvp.Value);
            }
            result.InitialState = initialState;

            // Apply Given conditions (validate initial state)
            foreach (var given in scenario.GivenAttributes)
            {
                if (!initialState.ContainsKey(given.Property))
                {
                    result.Success = false;
                    result.ErrorMessage = $"Missing property: {given.Property}";
                    return result;
                }

                var actualValue = initialState[given.Property];
                if (!ValuesEqual(actualValue, given.Value))
                {
                    result.Success = false;
                    result.ErrorMessage = $"Initial state mismatch: {given.Property} expected {given.Value} but got {actualValue}";
                    return result;
                }
            }

            // Simulate When action (for demo purposes)
            var finalState = new Dictionary<string, object?>(initialState);
            SimulateAction(scenario.WhenAttribute, finalState, actionData);
            result.FinalState = finalState;

            // Execute Then assertions
            foreach (var then in scenario.ThenAttributes)
            {
                // Get custom expected value if provided
                var expectedValue = then.Attribute.Expected;
                var expectedKey = then.MethodName + "_" + then.Attribute.Property;
                if (customExpectedValues?.TryGetValue(expectedKey, out var customExpected) == true && 
                    !string.IsNullOrWhiteSpace(customExpected))
                {
                    expectedValue = ParseExpectedValue(customExpected);
                }
                
                // Primary assertion
                var assertion = new AssertionResult
                {
                    Description = then.Attribute.Comment ?? $"{then.Attribute.Property} should equal {expectedValue}",
                    Expected = expectedValue
                };

                if (!string.IsNullOrWhiteSpace(then.Attribute.Property))
                {
                    if (finalState.TryGetValue(then.Attribute.Property, out var actualValue))
                    {
                        assertion.Actual = actualValue;
                        assertion.Passed = ValuesEqual(actualValue, expectedValue);
                    }
                    else
                    {
                        assertion.Passed = false;
                        assertion.Actual = null;
                    }
                }

                result.Assertions.Add(assertion);
                
                // Postcondition assertion (if specified)
                if (!string.IsNullOrWhiteSpace(then.Attribute.PostconditionProperty))
                {
                    var postconditionExpected = then.Attribute.PostconditionExpected;
                    var postconditionKey = then.MethodName + "_" + then.Attribute.PostconditionProperty;
                    if (customExpectedValues?.TryGetValue(postconditionKey, out var customPostcondition) == true && 
                        !string.IsNullOrWhiteSpace(customPostcondition))
                    {
                        postconditionExpected = ParseExpectedValue(customPostcondition);
                    }
                    
                    var postconditionAssertion = new AssertionResult
                    {
                        Description = $"{then.Attribute.PostconditionProperty} should equal {postconditionExpected}",
                        Expected = postconditionExpected
                    };
                    
                    if (finalState.TryGetValue(then.Attribute.PostconditionProperty, out var actualValue))
                    {
                        postconditionAssertion.Actual = actualValue;
                        postconditionAssertion.Passed = ValuesEqual(actualValue, postconditionExpected);
                    }
                    else
                    {
                        postconditionAssertion.Passed = false;
                        postconditionAssertion.Actual = null;
                    }
                    
                    result.Assertions.Add(postconditionAssertion);
                }
            }

            result.Success = result.Assertions.All(a => a.Passed);
            
            // Generate detailed error message if any assertions failed
            if (!result.Success)
            {
                var failedAssertions = result.Assertions.Where(a => !a.Passed).ToList();
                result.ErrorMessage = $"{failedAssertions.Count} assertion(s) failed: " + 
                    string.Join("; ", failedAssertions.Select(a => 
                        $"{a.Description} - Expected: {a.Expected}, Actual: {a.Actual}"));
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Execution error: {ex.Message}";
        }

        return result;
    }

    private static void SimulateAction(WhenAttribute? when, Dictionary<string, object?> state, string? actionData = null)
    {
        if (when == null) return;

        // Simple simulation based on action type
        if (when.ActionType == "DEPOSIT")
        {
            // Parse action data to get the amount
            var amount = ParseActionDataValue(actionData ?? when.ActionData, "Amount", 50);
            
            // Apply deposit to balance
            if (state.TryGetValue("Balance", out var balanceObj))
            {
                if (balanceObj is int balance)
                {
                    state["Balance"] = balance + (int)amount;
                }
                else if (balanceObj is decimal decimalBalance)
                {
                    state["Balance"] = decimalBalance + amount;
                }
                else if (balanceObj is double doubleBalance)
                {
                    state["Balance"] = doubleBalance + (double)amount;
                }
            }
            
            // Increment transaction count if present
            if (state.TryGetValue("TransactionCount", out var countObj) && countObj is int count)
            {
                state["TransactionCount"] = count + 1;
            }
            else
            {
                // Initialize TransactionCount to 1 if it doesn't exist
                state["TransactionCount"] = 1;
            }
        }
        // Add more action simulations as needed
    }

    private static decimal ParseActionDataValue(string? actionData, string propertyName, decimal defaultValue)
    {
        if (string.IsNullOrWhiteSpace(actionData))
            return defaultValue;

        // Parse "PropertyName: Value" format
        var parts = actionData.Split(',', StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            var keyValue = part.Split(':', StringSplitOptions.TrimEntries);
            if (keyValue.Length == 2 && keyValue[0].Equals(propertyName, StringComparison.OrdinalIgnoreCase))
            {
                if (decimal.TryParse(keyValue[1], out var value))
                {
                    return value;
                }
            }
        }

        return defaultValue;
    }

    private static object? ParseExpectedValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // Try to parse as number
        if (int.TryParse(value, out var intValue))
            return intValue;
        
        if (decimal.TryParse(value, out var decimalValue))
            return decimalValue;
        
        if (double.TryParse(value, out var doubleValue))
            return doubleValue;
        
        // Try to parse as boolean
        if (bool.TryParse(value, out var boolValue))
            return boolValue;
        
        // Return as string
        return value.Trim('"', '\'');
    }

    private static object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }

    private static bool ValuesEqual(object? actual, object? expected)
    {
        if (actual == null && expected == null) return true;
        if (actual == null || expected == null) return false;
        
        // Handle type conversions
        if (actual is JsonElement jsonElement)
        {
            actual = ConvertJsonElement(jsonElement);
        }

        return actual.ToString() == expected.ToString();
    }

    public static string GetExampleJson(ScenarioInfo scenario)
    {
        var example = new Dictionary<string, object?>();
        
        // Include all Given properties
        foreach (var given in scenario.GivenAttributes)
        {
            example[given.Property] = given.Value;
        }
        
        // Include postcondition properties if they don't already exist
        // Initialize them to sensible defaults (0 for numeric types, etc.)
        foreach (var then in scenario.ThenAttributes)
        {
            if (!string.IsNullOrWhiteSpace(then.Attribute.PostconditionProperty) &&
                !example.ContainsKey(then.Attribute.PostconditionProperty))
            {
                // Initialize to 0 for properties that look like counters
                if (then.Attribute.PostconditionProperty.Contains("Count", StringComparison.OrdinalIgnoreCase))
                {
                    example[then.Attribute.PostconditionProperty] = 0;
                }
            }
        }

        return JsonSerializer.Serialize(example, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }
}
