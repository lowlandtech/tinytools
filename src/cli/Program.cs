using System.Diagnostics;
using System.Text.Json;
using LowlandTech.TinyTools.Rendering;
using LowlandTech.TinyTools.Testing;
using LowlandTech.TinyTools.Workflows;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

const string Usage = """
    tinytools generate <workflow.yml> [--model model.json] [--root directory] [--templates directory]
    tinytools render <template> [<template>...] --output <file> | --out-dir <dir> [--model model.json] [--root directory]
    tinytools test <directory> [--approve]
    tinytools watch <template> --output <file> [--model model.json] [--root directory]
    """;

if (args.Length == 0 || args[0] is "--help" or "-h")
{
    Console.WriteLine(Usage);
    return 0;
}

if (string.Equals(args[0], "test", StringComparison.OrdinalIgnoreCase))
{
    return RunTest(args);
}

if (string.Equals(args[0], "render", StringComparison.OrdinalIgnoreCase))
{
    return RunRender(args);
}

if (string.Equals(args[0], "watch", StringComparison.OrdinalIgnoreCase))
{
    return await RunWatch(args);
}

if (!string.Equals(args[0], "generate", StringComparison.OrdinalIgnoreCase) || args.Length < 2)
{
    Console.Error.WriteLine(Usage);
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

static int RunTest(string[] args)
{
    var directory = Directory.GetCurrentDirectory();
    var approve = false;
    for (var i = 1; i < args.Length; i++)
    {
        if (args[i] == "--approve") approve = true;
        else if (!args[i].StartsWith("--", StringComparison.Ordinal)) directory = args[i];
    }

    var cases = TemplateSnapshotRunner.Discover(directory);
    if (cases.Count == 0)
    {
        Console.Error.WriteLine($"No .tmpl cases found in '{directory}'.");
        return 2;
    }

    var results = new TemplateSnapshotRunner().Run(cases, approve);
    foreach (var result in results)
    {
        Console.WriteLine($"{(result.Ok ? "ok" : "failed"),-7} {result.Case.Name}: {result.Message}");
        if (!result.Ok && result.Diff.Length > 0) Console.WriteLine(result.Diff);
    }

    return results.All(result => result.Ok) ? 0 : 1;
}

static int RunRender(string[] args)
{
    var templates = new List<string>();
    string? model = null;
    string? output = null;
    string? outDir = null;
    var root = Directory.GetCurrentDirectory();

    for (var i = 1; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--model" when i + 1 < args.Length: model = args[++i]; break;
            case "--output" when i + 1 < args.Length: output = args[++i]; break;
            case "--out-dir" when i + 1 < args.Length: outDir = args[++i]; break;
            case "--root" when i + 1 < args.Length: root = args[++i]; break;
            default: templates.Add(args[i]); break;
        }
    }

    if (templates.Count == 0 || (output is null && outDir is null))
    {
        Console.Error.WriteLine("render needs at least one template and --output or --out-dir.");
        return 2;
    }

    var renderer = new TemplateFileRenderer();
    var failures = 0;
    try
    {
        foreach (var template in templates)
        {
            var destination = output ?? Path.Combine(outDir!, Path.GetFileNameWithoutExtension(template) + ".txt");
            var result = renderer.Render(template, model, destination, root);
            Console.WriteLine($"{(result.Success ? "rendered" : "failed"),-9} {template}: {result.Message}");
            if (!result.Success) failures++;
        }
    }
    catch (InvalidOperationException error)
    {
        Console.Error.WriteLine(error.Message);
        return 3;
    }

    return failures == 0 ? 0 : 1;
}

static async Task<int> RunWatch(string[] args)
{
    var templates = new List<string>();
    string? model = null;
    string? output = null;
    string? outDir = null;
    var root = Directory.GetCurrentDirectory();

    for (var i = 1; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--model" when i + 1 < args.Length: model = args[++i]; break;
            case "--output" when i + 1 < args.Length: output = args[++i]; break;
            case "--out-dir" when i + 1 < args.Length: outDir = args[++i]; break;
            case "--root" when i + 1 < args.Length: root = args[++i]; break;
            default: templates.Add(args[i]); break;
        }
    }

    if (templates.Count == 0)
    {
        Console.Error.WriteLine("watch needs at least one template.");
        return 2;
    }

    var renderer = new TemplateFileRenderer();
    var relevant = templates.Concat(model is null ? [] : new[] { model }).ToList();
    using var cancellation = new CancellationTokenSource();
    Console.CancelKeyPress += (_, eventArgs) => { eventArgs.Cancel = true; cancellation.Cancel(); };

    void RenderAll(IReadOnlyList<string> changed)
    {
        foreach (var template in templates)
        {
            var destination = output ?? Path.Combine(outDir ?? root, Path.GetFileNameWithoutExtension(template) + ".txt");
            var result = renderer.Render(template, model, destination, root);
            Console.WriteLine($"{(result.Success ? "rendered" : "failed"),-9} {template}: {result.Message}");
        }
    }

    RenderAll(relevant);
    Console.WriteLine($"watching {string.Join(", ", relevant)}; press Ctrl+C to stop.");
    var watcher = new TemplateWatcher(relevant, RenderAll);
    try
    {
        await watcher.RunAsync(cancellation.Token);
    }
    catch (OperationCanceledException)
    {
        // Ctrl+C is the documented shutdown path.
    }

    return 0;
}