using System.Diagnostics;
using System.Text.Json;
using LowlandTech.TinyTools.Workflows;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

if (args.Length == 0 || args[0] is "--help" or "-h")
{
    Console.WriteLine("tinytools generate <workflow.yml> [--model model.json] [--root directory] [--templates directory]");
    return 0;
}
if (!string.Equals(args[0], "generate", StringComparison.OrdinalIgnoreCase) || args.Length < 2)
{
    Console.Error.WriteLine("Usage: tinytools generate <workflow.yml> [--model model.json] [--root directory] [--templates directory]");
    return 2;
}

var workflowFile = Path.GetFullPath(args[1]);
var root = Directory.GetCurrentDirectory();
var templateRoot = Path.GetDirectoryName(workflowFile) ?? root;
string? modelFile = null;
for (var i = 2; i < args.Length; i++)
{
    if (args[i] == "--model" && i + 1 < args.Length) modelFile = args[++i];
    else if (args[i] == "--root" && i + 1 < args.Length) root = args[++i];
    else if (args[i] == "--templates" && i + 1 < args.Length) templateRoot = args[++i];
}

try
{
    var yaml = await File.ReadAllTextAsync(workflowFile);
    var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
    var workflow = deserializer.Deserialize<WorkflowDefinition>(yaml) ?? throw new InvalidOperationException("Workflow is empty.");
    object? model = modelFile is null ? null : ToModel(JsonSerializer.Deserialize<JsonElement>(await File.ReadAllTextAsync(modelFile)));
    var runner = new WorkflowRunner();
    var result = await runner.RunAsync(
        workflow,
        root,
        model,
        (step, stepRoot, cancellationToken) => WorkflowCommandExecutor.RunAsync(step, stepRoot, cancellationToken),
        templateRootDirectory: templateRoot);
    foreach (var step in result.Steps) Console.WriteLine($"{(step.Changed ? "changed" : "unchanged"),-9} {step.Id ?? step.Type}: {step.Message}");
    return 0;
}
catch (WorkflowCommandRefusedException refused)
{
    // The policy is a refusal, not a crash: name the value and exit with its own code.
    Console.Error.WriteLine(refused.Message);
    return 3;
}
catch (Exception error) when (error is InvalidOperationException or IOException or FileNotFoundException)
{
    Console.Error.WriteLine(error.Message);
    return 1;
}

static object? ToModel(JsonElement value) => value.ValueKind switch
{
    JsonValueKind.Object => value.EnumerateObject().ToDictionary(property => property.Name, property => ToModel(property.Value), StringComparer.OrdinalIgnoreCase),
    JsonValueKind.Array => value.EnumerateArray().Select(ToModel).ToList(),
    JsonValueKind.String => value.GetString(),
    JsonValueKind.Number when value.TryGetInt64(out var integer) => integer,
    JsonValueKind.Number => value.GetDouble(),
    JsonValueKind.True => true,
    JsonValueKind.False => false,
    _ => null
};
