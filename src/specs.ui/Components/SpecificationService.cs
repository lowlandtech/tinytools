using System.Reflection;
using LowlandTech.Specs;

namespace LowlandTech.Specs.UI.Components;

public class FeatureInfo
{
    // Card-level (Specification)
    public string SpecId { get; set; } = string.Empty;
    public string? Epic { get; set; }
    public string SpecName { get; set; } = string.Empty;
    public string? EpicDescription { get; set; }
    
    // Feature-level (User Story)
    public List<UserStoryFeature> UserStories { get; set; } = new();
}

public class UserStoryFeature
{
    public string UserStoryId { get; set; } = string.Empty;
    public string FeatureName { get; set; } = string.Empty;
    public string? UserStoryDescription { get; set; }
    public List<ScenarioInfo> Scenarios { get; set; } = new();
}

public class SpecificationService
{
    public static List<FeatureInfo> LoadFeatures()
    {
        var scenarios = ScenarioLoader.LoadScenariosFromExample();
        
        // Group by SPEC first (card level)
        var features = scenarios
            .GroupBy(s => s.ScenarioAttribute.SpecId)
            .Select(specGroup =>
            {
                var first = specGroup.First();
                
                // Within each spec, group by User Story (feature file level)
                var userStories = specGroup
                    .GroupBy(s => s.ScenarioAttribute.UserStory)
                    .Select(usGroup =>
                    {
                        var usFirst = usGroup.First();
                        return new UserStoryFeature
                        {
                            UserStoryId = usFirst.ScenarioAttribute.UserStory ?? "UNKNOWN",
                            FeatureName = DeriveUserStoryFeatureName(usFirst.ScenarioAttribute.UserStory),
                            UserStoryDescription = DeriveUserStoryDescription(usFirst.ScenarioAttribute.UserStory),
                            Scenarios = usGroup.OrderBy(s => s.ScenarioAttribute.ScenarioId).ToList()
                        };
                    })
                    .OrderBy(us => us.UserStoryId)
                    .ToList();
                
                return new FeatureInfo
                {
                    SpecId = first.ScenarioAttribute.SpecId ?? "UNKNOWN",
                    Epic = DeriveEpic(first.ScenarioAttribute.SpecId),
                    SpecName = DeriveSpecName(first.ScenarioAttribute.SpecId),
                    EpicDescription = DeriveEpicDescription(first.ScenarioAttribute.SpecId),
                    UserStories = userStories
                };
            })
            .OrderBy(f => f.SpecId)
            .ToList();

        return features;
    }

    private static string? DeriveEpic(string? specId)
    {
        return specId switch
        {
            "SPEC0001" => "EPIC01",
            _ => null
        };
    }

    private static string DeriveSpecName(string? specId)
    {
        return specId switch
        {
            "SPEC0001" => "Account Balance Management",
            _ => $"Specification {specId}"
        };
    }

    private static string? DeriveEpicDescription(string? specId)
    {
        // Epic step description - the cross-cutting traversal step
        return specId switch
        {
            "SPEC0001" => "Financial transaction management and balance tracking across the platform",
            _ => null
        };
    }

    private static string DeriveUserStoryFeatureName(string? userStory)
    {
        // Feature name at user story level - specific action/capability
        return userStory switch
        {
            "US01" => "Deposit funds",
            _ => $"User Story {userStory}"
        };
    }

    private static string? DeriveUserStoryDescription(string? userStory)
    {
        // User story description in "As a... I want... So that..." format
        return userStory switch
        {
            "US01" => "As a user, I want to deposit funds into my account, so that I can increase my available balance.",
            _ => null
        };
    }
}


