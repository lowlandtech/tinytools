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

var yaml = await File.ReadAllTextAsync(workflowFile);
var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
var workflow = deserializer.Deserialize<WorkflowDefinition>(yaml) ?? throw new InvalidOperationException("Workflow is empty.");
object? model = modelFile is null ? null : ToModel(JsonSerializer.Deserialize<JsonElement>(await File.ReadAllTextAsync(modelFile)));
var runner = new WorkflowRunner();
var result = await runner.RunAsync(workflow, root, model, RunCommandAsync, templateRootDirectory: templateRoot);
foreach (var step in result.Steps) Console.WriteLine($"{(step.Changed ? "changed" : "unchanged"),-9} {step.Id ?? step.Type}: {step.Message}");
return 0;

static async Task<WorkflowStepResult> RunCommandAsync(WorkflowStep step, string root, CancellationToken cancellationToken)
{
    var command = step.Command?.Trim();
    if (command is not ("dotnet" or "git" or "gh")) throw new InvalidOperationException($"Command is not allowed: {command}");
    var workingDirectory = string.IsNullOrWhiteSpace(step.WorkingDirectory) ? root : Path.GetFullPath(Path.Combine(root, step.WorkingDirectory));
    if (!workingDirectory.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) && !string.Equals(workingDirectory, root, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException($"Working directory escapes workflow root: {step.WorkingDirectory}");
    var startInfo = new ProcessStartInfo(command) { WorkingDirectory = workingDirectory, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
    foreach (var arg in step.Args) startInfo.ArgumentList.Add(arg);
    using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Could not start {command}.");
    var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
    var error = await process.StandardError.ReadToEndAsync(cancellationToken);
    await process.WaitForExitAsync(cancellationToken);
    if (process.ExitCode != 0) throw new InvalidOperationException($"{command} failed ({process.ExitCode}): {error}");
    return new(step.Id, step.Type, true, string.IsNullOrWhiteSpace(output) ? "completed" : output.Trim());
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
