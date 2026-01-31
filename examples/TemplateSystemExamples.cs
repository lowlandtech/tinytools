using LowlandTech.TinyTools;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace Examples;

/// <summary>
/// Demonstrates using ITemplate for large-scale code generation.
/// </summary>
public class TemplateSystemExamples
{
    public static void Main()
    {
        BasicTemplateUsage();
        ValidationExample();
        RegistryUsage();
        LargeScaleGeneration();
    }

    static void BasicTemplateUsage()
    {
        Console.WriteLine("=== Basic Template Usage ===\n");

        var template = new ComponentTemplate();
        
        var data = new ComponentData
        {
            ComponentName = "ProfileCard",
            Props = new()
            {
                new PropDefinition { Name = "userName", Type = "string" },
                new PropDefinition { Name = "avatarUrl", Type = "string" },
                new PropDefinition { Name = "bio", Type = "string" }
            },
            PropsDestructured = "{ userName, avatarUrl, bio }"
        };

        var result = template.Render(data);

        Console.WriteLine($"Generated File: {result.Path}");
        Console.WriteLine($"Namespace: {result.Namespace}");
        Console.WriteLine($"\nContent:\n{result.Content}");
    }

    static void ValidationExample()
    {
        Console.WriteLine("\n\n=== Template Validation ===\n");

        var template = new ComponentTemplate();
        
        // Simple validation
        if (template.Validate())
        {
            Console.WriteLine("? Template validation passed!");
        }
        else
        {
            Console.WriteLine("? Template validation failed!");
        }

        // Detailed validation
        var template2 = new CSharpClassTemplate();
        var validationResult = template2.ValidateDetailed();
        
        Console.WriteLine($"\nDetailed Validation: {(validationResult.IsValid ? "? PASS" : "? FAIL")}");
        
        if (!validationResult.IsValid)
        {
            Console.WriteLine($"Error: {validationResult.ErrorMessage}");
            
            if (validationResult.Differences != null)
            {
                foreach (var (key, (expected, actual)) in validationResult.Differences)
                {
                    Console.WriteLine($"\n{key}:");
                    Console.WriteLine($"  Expected: {expected.Substring(0, Math.Min(50, expected.Length))}...");
                    Console.WriteLine($"  Actual:   {actual.Substring(0, Math.Min(50, actual.Length))}...");
                }
            }
        }
    }

    static void RegistryUsage()
    {
        Console.WriteLine("\n\n=== Template Registry ===\n");

        var registry = new TemplateRegistry();
        
        // Manual registration
        registry.Register("component", new ComponentTemplate());
        registry.Register("class", new CSharpClassTemplate());

        Console.WriteLine("Registered templates:");
        foreach (var name in registry.GetNames())
        {
            Console.WriteLine($"  - {name}");
        }

        // Validate all
        Console.WriteLine("\nValidating all templates:");
        var validationResults = registry.ValidateAll();
        
        foreach (var (name, result) in validationResults)
        {
            Console.WriteLine($"  {name}: {(result.IsValid ? "?" : "?")} {result.ErrorMessage ?? ""}");
        }

        // Batch rendering
        Console.WriteLine("\nBatch rendering:");
        var batch = new Dictionary<string, object>
        {
            ["component"] = new ComponentData 
            { 
                ComponentName = "Button",
                Props = new() { new() { Name = "label", Type = "string" } },
                PropsDestructured = "{ label }"
            },
            ["class"] = new ClassData
            {
                Namespace = "MyApp.Services",
                ClassName = "UserService",
                Description = "Handles user operations"
            }
        };

        var results = registry.RenderBatch(batch);
        
        foreach (var (name, result) in results)
        {
            Console.WriteLine($"  {name} -> {result.Path}");
        }
    }

    static void LargeScaleGeneration()
    {
        Console.WriteLine("\n\n=== Large-Scale Generation (100 components) ===\n");

        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());

        var componentNames = new[]
        {
            "Button", "Input", "Checkbox", "Radio", "Select", "Textarea",
            "Card", "Modal", "Dialog", "Drawer", "Popover", "Tooltip",
            "Alert", "Badge", "Avatar", "Spinner", "Progress", "Skeleton",
            "Table", "List", "Tree", "Menu", "Tabs", "Accordion",
            "Form", "FormField", "FormLabel", "FormError", "FormHelper",
            "Header", "Footer", "Sidebar", "Navbar", "Breadcrumb",
            "Pagination", "Dropdown", "Autocomplete", "DatePicker", "TimePicker",
            "Slider", "Switch", "Toggle", "Rating", "Stepper",
            "Chip", "Tag", "Icon", "Image", "Video", "Audio",
            "Chart", "Graph", "Map", "Calendar", "Timeline",
            "Grid", "Flex", "Box", "Container", "Stack",
            "Text", "Heading", "Paragraph", "Link", "Code",
            "Divider", "Separator", "Spacer", "Portal", "Overlay",
            "Toast", "Snackbar", "Notification", "Banner", "Callout",
            "Hero", "Feature", "Testimonial", "Pricing", "FAQ",
            "Footer", "CTA", "Newsletter", "Social", "Copyright",
            "SearchBar", "FilterPanel", "SortMenu", "ViewToggle", "ActionBar",
            "ProfileCard", "UserAvatar", "StatusBadge", "ActivityFeed", "CommentSection",
            "FileUpload", "ImageCropper", "ColorPicker", "RichTextEditor", "MarkdownEditor",
            "CodeEditor", "Terminal", "Console", "Logger", "Debugger"
        };

        Console.WriteLine($"Generating {componentNames.Length} components...\n");

        var generatedFiles = new List<string>();
        var startTime = DateTime.Now;

        foreach (var componentName in componentNames)
        {
            var data = new ComponentData
            {
                ComponentName = componentName,
                Props = new()
                {
                    new PropDefinition { Name = "className", Type = "string" },
                    new PropDefinition { Name = "children", Type = "React.ReactNode" }
                },
                PropsDestructured = "{ className, children }"
            };

            var result = registry.Render("component", data);
            generatedFiles.Add(result.Path);

            // In real scenario, you'd write to disk:
            // File.WriteAllText(result.Path, result.Content);
        }

        var elapsed = DateTime.Now - startTime;

        Console.WriteLine($"? Generated {generatedFiles.Count} files in {elapsed.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Average: {elapsed.TotalMilliseconds / generatedFiles.Count:F2}ms per file");
        Console.WriteLine($"\nFirst 5 files:");
        
        foreach (var file in generatedFiles.Take(5))
        {
            Console.WriteLine($"  - {file}");
        }
        
        Console.WriteLine($"  ... and {generatedFiles.Count - 5} more");
    }
}
