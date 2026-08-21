namespace LowlandTech.TinyTools.Workflows;

/// <summary>Runs the portable, file-producing part of a workflow.</summary>
public sealed class WorkflowRunner
{
    private readonly TinyTemplateEngine _engine;

    public WorkflowRunner(TinyTemplateEngine? engine = null) => _engine = engine ?? new TinyTemplateEngine();

    public async Task<WorkflowResult> RunAsync(
        WorkflowDefinition workflow,
        string rootDirectory,
        object? model = null,
        Func<WorkflowStep, string, CancellationToken, Task<WorkflowStepResult>>? commandRunner = null,
        CancellationToken cancellationToken = default,
        string? templateRootDirectory = null)
    {
        if (workflow is null) throw new ArgumentNullException(nameof(workflow));
        var root = Path.GetFullPath(rootDirectory);
        var templateRoot = Path.GetFullPath(templateRootDirectory ?? rootDirectory);
        Directory.CreateDirectory(root);
        var context = new ToolContext { Model = model };
        foreach (var variable in workflow.Variables) context.Set(variable.Key, variable.Value);

        var results = new List<WorkflowStepResult>();
        foreach (var step in workflow.Steps)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.IsNullOrWhiteSpace(step.ForEach))
            {
                var values = _engine.ResolveExpression(step.ForEach, context) as System.Collections.IEnumerable
                    ?? throw new InvalidOperationException($"ForEach expression resolved to no collection: {step.ForEach}");
                var index = 0;
                foreach (var value in values)
                {
                    var child = context.CreateChild($"{step.Id ?? step.Type}[{index++}]");
                    child.Set(step.Item, value);
                    results.Add(await RunStepAsync(step, root, templateRoot, child, commandRunner, cancellationToken));
                }
            }
            else
            {
                results.Add(await RunStepAsync(step, root, templateRoot, context, commandRunner, cancellationToken));
            }
        }
        return new WorkflowResult(results);
    }

    private async Task<WorkflowStepResult> RunStepAsync(
        WorkflowStep step,
        string root,
        string templateRoot,
        ToolContext context,
        Func<WorkflowStep, string, CancellationToken, Task<WorkflowStepResult>>? commandRunner,
        CancellationToken cancellationToken)
    {
        var resolvedStep = ResolveStep(step, context);
        if (string.Equals(resolvedStep.Type, "template", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(resolvedStep.Template) || string.IsNullOrWhiteSpace(resolvedStep.Output))
                throw new InvalidOperationException("Template steps require 'template' and 'output'.");

                var templatePath = ResolvePath(templateRoot, resolvedStep.Template);
                var outputPath = ResolvePath(root, resolvedStep.Output);
                if (!File.Exists(templatePath)) throw new FileNotFoundException("Template not found.", templatePath);
                if (ShouldSkip(resolvedStep.When, outputPath))
                {
                    return new(resolvedStep.Id, resolvedStep.Type, false, "output exists");
                }

                var rendered = _engine.Render(await File.ReadAllTextAsync(templatePath, cancellationToken), context);
                var changed = !File.Exists(outputPath) || !string.Equals(await File.ReadAllTextAsync(outputPath, cancellationToken), rendered, StringComparison.Ordinal);
                if (changed)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                    await File.WriteAllTextAsync(outputPath, rendered, cancellationToken);
                }
                return new(resolvedStep.Id, resolvedStep.Type, changed, outputPath);
            }
        if (commandRunner is null) throw new InvalidOperationException($"No command runner was supplied for step '{resolvedStep.Id ?? resolvedStep.Type}'.");
        return await commandRunner(resolvedStep, root, cancellationToken);
    }

    private WorkflowStep ResolveStep(WorkflowStep step, ToolContext context) => new()
    {
        Id = Resolve(step.Id, context), Type = Resolve(step.Type, context) ?? step.Type, Template = Resolve(step.Template, context), Output = Resolve(step.Output, context),
        Command = Resolve(step.Command, context), Args = step.Args.Select(argument => Resolve(argument, context) ?? string.Empty).ToList(),
        WorkingDirectory = Resolve(step.WorkingDirectory, context), When = Resolve(step.When, context) ?? step.When, ForEach = step.ForEach, Item = step.Item
    };

    string? Resolve(string? value, ToolContext context) => value is null ? null : _engine.ResolveVariables(value, context);

    private static bool ShouldSkip(string when, string outputPath) =>
        string.Equals(when, "if-missing", StringComparison.OrdinalIgnoreCase) && File.Exists(outputPath);

    private static string ResolvePath(string root, string path)
    {
        var full = Path.GetFullPath(Path.Combine(root, path));
        if (!full.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(full, root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Path escapes workflow root: {path}");
        return full;
    }
}
