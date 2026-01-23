using System.Reflection;
using LowlandTech.Specs;

namespace LowlandTech.Specs.UI.Components;

public class ScenarioInfo
{
    public ScenarioAttribute ScenarioAttribute { get; set; } = null!;
    public string ClassName { get; set; } = string.Empty;
    public string? StateTypeName { get; set; }  // Changed from Type to string for serialization
    public List<GivenAttribute> GivenAttributes { get; set; } = new();
    public WhenAttribute? WhenAttribute { get; set; }
    public List<ThenInfo> ThenAttributes { get; set; } = new();
}

public class ThenInfo
{
    public ThenAttribute Attribute { get; set; } = null!;
    public string MethodName { get; set; } = string.Empty;
}

public static class ScenarioLoader
{
    public static List<ScenarioInfo> LoadScenariosFromExample()
    {
        var scenarios = new List<ScenarioInfo>();
        var exampleType = typeof(BddAutoExecutionExample);
        var scenarioTypes = exampleType.GetNestedTypes()
            .Where(t => t.GetCustomAttribute<ScenarioAttribute>() != null)
            .ToList();

        foreach (var type in scenarioTypes)
        {
            var scenarioAttr = type.GetCustomAttribute<ScenarioAttribute>();
            if (scenarioAttr == null) continue;

            var scenario = new ScenarioInfo
            {
                ScenarioAttribute = scenarioAttr,
                ClassName = type.Name
            };

            // Get For method
            var forMethod = type.GetMethod("For", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (forMethod != null)
            {
                var forAttr = forMethod.GetCustomAttribute<ForAttribute>();
                scenario.StateTypeName = forAttr?.StateType?.Name;  // Convert Type to string
            }

            // Get Given method
            var givenMethod = type.GetMethod("Given", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (givenMethod != null)
            {
                scenario.GivenAttributes = givenMethod.GetCustomAttributes<GivenAttribute>().ToList();
            }

            // Get When method
            var whenMethod = type.GetMethod("When", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (whenMethod != null)
            {
                scenario.WhenAttribute = whenMethod.GetCustomAttribute<WhenAttribute>();
            }

            // Get Then methods
            var thenMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<ThenAttribute>() != null)
                .ToList();

            foreach (var method in thenMethods)
            {
                var thenAttr = method.GetCustomAttribute<ThenAttribute>();
                if (thenAttr != null)
                {
                    scenario.ThenAttributes.Add(new ThenInfo
                    {
                        Attribute = thenAttr,
                        MethodName = method.Name
                    });
                }
            }

            scenarios.Add(scenario);
        }

        return scenarios;
    }
}

