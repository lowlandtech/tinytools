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
            // Parse JSON - expecting Given/When/Then structure
            var useCaseData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData);
            if (useCaseData == null)
            {
                result.Success = false;
                result.ErrorMessage = "Invalid JSON format";
                return result;
            }

            // Extract Given (initial state)
            Dictionary<string, object?> initialState;
            if (useCaseData.TryGetValue("Given", out var givenElement))
            {
                initialState = ParseStateObject(givenElement);
            }
            else
            {
                // Fallback: treat entire JSON as state (backwards compatibility)
                initialState = new Dictionary<string, object?>();
                foreach (var kvp in useCaseData)
                {
                    initialState[kvp.Key] = ConvertJsonElement(kvp.Value);
                }
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

            // Extract When (action data) - use custom or from JSON
            string? effectiveActionData = actionData;
            if (string.IsNullOrWhiteSpace(effectiveActionData) && 
                useCaseData.TryGetValue("When", out var whenElement))
            {
                effectiveActionData = ExtractActionData(whenElement);
            }

            // Simulate When action
            var finalState = new Dictionary<string, object?>(initialState);
            SimulateAction(scenario.WhenAttribute, finalState, effectiveActionData);
            result.FinalState = finalState;

            // Extract Then (expected outcomes)
            Dictionary<string, object?>? expectedOutcomes = null;
            if (useCaseData.TryGetValue("Then", out var thenElement))
            {
                expectedOutcomes = ParseStateObject(thenElement);
            }

            // Execute Then assertions
            foreach (var then in scenario.ThenAttributes)
            {
                // Get expected value: custom UI input > Then JSON > attribute default
                var expectedValue = then.Attribute.Expected;
                var expectedKey = then.MethodName + "_" + then.Attribute.Property;
                
                if (customExpectedValues?.TryGetValue(expectedKey, out var customExpected) == true && 
                    !string.IsNullOrWhiteSpace(customExpected))
                {
                    expectedValue = ParseExpectedValue(customExpected);
                }
                else if (expectedOutcomes?.TryGetValue(then.Attribute.Property, out var jsonExpected) == true)
                {
                    expectedValue = jsonExpected;
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
                    else if (expectedOutcomes?.TryGetValue(then.Attribute.PostconditionProperty, out var jsonPostcondition) == true)
                    {
                        postconditionExpected = jsonPostcondition;
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

    private static Dictionary<string, object?> ParseStateObject(JsonElement element)
    {
        var state = new Dictionary<string, object?>();
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                state[property.Name] = ConvertJsonElement(property.Value);
            }
        }
        return state;
    }

    private static string? ExtractActionData(JsonElement whenElement)
    {
        if (whenElement.ValueKind != JsonValueKind.Object)
            return null;

        var parts = new List<string>();
        foreach (var property in whenElement.EnumerateObject())
        {
            if (!property.Name.Equals("ActionType", StringComparison.OrdinalIgnoreCase))
            {
                parts.Add($"{property.Name}: {property.Value}");
            }
        }
        return parts.Any() ? string.Join(", ", parts) : null;
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
        
        // Build Given section from Given attributes
        var given = new Dictionary<string, object?>();
        foreach (var givenAttr in scenario.GivenAttributes)
        {
            given[givenAttr.Property] = givenAttr.Value;
        }
        
        // Add postcondition properties to Given if they don't exist (for initial state)
        foreach (var thenAttr in scenario.ThenAttributes)
        {
            if (!string.IsNullOrWhiteSpace(thenAttr.Attribute.PostconditionProperty) &&
                !given.ContainsKey(thenAttr.Attribute.PostconditionProperty))
            {
                // Initialize to 0 for properties that look like counters
                if (thenAttr.Attribute.PostconditionProperty.Contains("Count", StringComparison.OrdinalIgnoreCase))
                {
                    given[thenAttr.Attribute.PostconditionProperty] = 0;
                }
            }
        }
        
        example["Given"] = given;
        
        // Build When section from WhenAttribute
        if (scenario.WhenAttribute != null)
        {
            var when = new Dictionary<string, object?>();
            
            if (!string.IsNullOrWhiteSpace(scenario.WhenAttribute.ActionType))
            {
                when["ActionType"] = scenario.WhenAttribute.ActionType;
                
                // Add default parameters based on action type
                // These are example values that users can modify
                switch (scenario.WhenAttribute.ActionType)
                {
                    case "DEPOSIT":
                        when["Amount"] = 50;
                        break;
                    case "WITHDRAWAL":
                        when["Amount"] = 25;
                        break;
                    case "TRANSFER":
                        when["Amount"] = 100;
                        when["ToAccount"] = "ACC-002";
                        break;
                    // Add more action types as needed
                }
            }
            
            // Override with ActionData from attribute if provided (backwards compatibility)
            if (!string.IsNullOrWhiteSpace(scenario.WhenAttribute.ActionData))
            {
                var parts = scenario.WhenAttribute.ActionData.Split(',', StringSplitOptions.TrimEntries);
                foreach (var part in parts)
                {
                    var keyValue = part.Split(':', StringSplitOptions.TrimEntries);
                    if (keyValue.Length == 2)
                    {
                        // Try to parse as number
                        if (int.TryParse(keyValue[1], out var intVal))
                            when[keyValue[0]] = intVal;
                        else if (decimal.TryParse(keyValue[1], out var decVal))
                            when[keyValue[0]] = decVal;
                        else
                            when[keyValue[0]] = keyValue[1];
                    }
                }
            }
            
            example["When"] = when;
        }
        
        // Build Then section from Then attributes
        var then = new Dictionary<string, object?>();
        foreach (var thenAttr in scenario.ThenAttributes)
        {
            // Debug: Log what we're reading
            Console.WriteLine($"DEBUG Then: Property={thenAttr.Attribute.Property}, Expected={thenAttr.Attribute.Expected}, ExpectedType={thenAttr.Attribute.Expected?.GetType().Name}");
            
            if (!string.IsNullOrWhiteSpace(thenAttr.Attribute.Property) && 
                thenAttr.Attribute.Expected != null)
            {
                then[thenAttr.Attribute.Property] = thenAttr.Attribute.Expected;
            }
            if (!string.IsNullOrWhiteSpace(thenAttr.Attribute.PostconditionProperty) &&
                thenAttr.Attribute.PostconditionExpected != null)
            {
                Console.WriteLine($"DEBUG Postcondition: Property={thenAttr.Attribute.PostconditionProperty}, Expected={thenAttr.Attribute.PostconditionExpected}");
                then[thenAttr.Attribute.PostconditionProperty] = thenAttr.Attribute.PostconditionExpected;
            }
        }
        
        Console.WriteLine($"DEBUG Final Then JSON: {JsonSerializer.Serialize(then)}");
        example["Then"] = then;

        return JsonSerializer.Serialize(example, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }
}
