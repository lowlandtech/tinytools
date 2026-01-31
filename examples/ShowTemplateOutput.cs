using LowlandTech.TinyTools;
using LowlandTech.TinyTools.UnitTests.Examples;
using System.Text.Json;

Console.WriteLine("=== ComponentTemplate Actual Output ===");
var componentTemplate = new ComponentTemplate();
var componentData = JsonSerializer.Deserialize<ComponentData>(componentTemplate.TestDataJson);
var componentResult = componentTemplate.Render(componentData!);

Console.WriteLine("Path: " + componentResult.Path);
Console.WriteLine("Namespace: " + componentResult.Namespace);
Console.WriteLine("\nContent:");
Console.WriteLine(componentResult.Content);
Console.WriteLine("\n=== End ComponentTemplate ===\n");

Console.WriteLine("=== CSharpClassTemplate Actual Output ===");
var classTemplate = new CSharpClassTemplate();
var classData = JsonSerializer.Deserialize<ClassData>(classTemplate.TestDataJson);
var classResult = classTemplate.Render(classData!);

Console.WriteLine("Path: " + classResult.Path);
Console.WriteLine("Namespace: " + classResult.Namespace);
Console.WriteLine("\nContent:");
Console.WriteLine(classResult.Content);
Console.WriteLine("\n=== End CSharpClassTemplate ===");
