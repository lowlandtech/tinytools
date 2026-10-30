using System.Reflection;
using Xunit.v3;

namespace LowlandTech.TinyTools.Tests.Infrastructure;

/// <summary>
/// Validates the shape of migrated scenarios rather than their behavior: a story
/// declares its vocabulary, every fact names exactly one case from it, and every
/// fact keeps its acceptance identity. Scenarios that have not declared a story
/// yet are reported separately so migration progress stays measurable.
/// </summary>
public static class SpecLayout
{
    /// <summary>Scenario classes carrying a user story identity.</summary>
    public static IReadOnlyList<Type> Scenarios(Assembly assembly) =>
        assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .Where(type => type.GetCustomAttributes().OfType<UserStoryAttribute>().Any())
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToList();

    /// <summary>Scenarios that declare a story and are therefore checked.</summary>
    public static IReadOnlyList<Type> Migrated(Assembly assembly) =>
        Scenarios(assembly).Where(type => type.GetCustomAttribute<StoryAttribute>() is not null).ToList();

    /// <summary>Scenarios still waiting for a story vocabulary.</summary>
    public static IReadOnlyList<Type> Unmigrated(Assembly assembly) =>
        Scenarios(assembly).Where(type => type.GetCustomAttribute<StoryAttribute>() is null).ToList();

    /// <summary>Facts and theories declared by a scenario.</summary>
    public static IReadOnlyList<MethodInfo> Facts(Type scenario) =>
        scenario.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Where(method => IsTest(method))
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .ToList();

    /// <summary>Every shape violation across the assembly's migrated scenarios.</summary>
    public static IReadOnlyList<string> Violations(Assembly assembly)
    {
        var violations = new List<string>();
        foreach (var scenario in Migrated(assembly))
        {
            var story = scenario.GetCustomAttribute<StoryAttribute>()!.Create();
            if (string.IsNullOrWhiteSpace(story.Goal))
                violations.Add($"{scenario.Name}: story goal is empty");

            var cases = new Dictionary<string, string>(StringComparer.Ordinal);
            var facts = Facts(scenario);
            if (facts.Count == 0)
                violations.Add($"{scenario.Name}: migrated scenario declares no facts");

            foreach (var fact in facts)
            {
                var annotation = fact.GetCustomAttributes().OfType<ICaseAnnotation>().SingleOrDefault();
                if (annotation is null)
                {
                    violations.Add($"{scenario.Name}.{fact.Name}: missing [Case<...>] identity");
                    continue;
                }

                if (!story.Cases.ContainsKey(annotation.CaseId))
                    violations.Add($"{scenario.Name}.{fact.Name}: case {annotation.CaseId} is not in {scenario.GetCustomAttribute<StoryAttribute>()!.StoryType.Name}");

                if (cases.TryGetValue(annotation.CaseId, out var owner))
                    violations.Add($"{scenario.Name}.{fact.Name}: case {annotation.CaseId} is already asserted by {owner}");
                else
                    cases[annotation.CaseId] = fact.Name;

                if (!HasAcceptanceIdentity(fact))
                    violations.Add($"{scenario.Name}.{fact.Name}: missing [Trait(Spec.UAC, ...)] acceptance identity");
            }
        }

        return violations;
    }

    private static bool IsTest(MethodInfo method) =>
        method.GetCustomAttributesData().Any(data =>
            data.AttributeType.Name is "FactAttribute" or "TheoryAttribute");

    private static bool HasAcceptanceIdentity(MethodInfo method) =>
        method.GetCustomAttributesData().Any(data =>
            data.AttributeType.Name == "TraitAttribute"
            && data.ConstructorArguments.Count == 2
            && data.ConstructorArguments[0].Value as string == Spec.UAC
            && data.ConstructorArguments[1].Value is string value
            && !string.IsNullOrWhiteSpace(value));
}
