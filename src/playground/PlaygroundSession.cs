namespace LowlandTech.TinyTools.Playground;

/// <summary>
/// The rendering surface the playground page uses. It exists as its own type so the page can be
/// rendered and asserted in a component test without a browser: the same engine call the WebAssembly
/// build performs runs here.
/// </summary>
public sealed class PlaygroundSession
{
    private readonly TinyTemplateEngine _engine = new();

    /// <summary>The template being edited.</summary>
    public string Template { get; set; } = "Hello ${Context.Model.Name}!";

    /// <summary>The JSON model the template resolves against.</summary>
    public string Model { get; set; } = "{\n  \"Name\": \"Ada\"\n}";

    /// <summary>The last rendered text, or the diagnostic when the template is invalid.</summary>
    public string Output { get; private set; } = string.Empty;

    /// <summary>The diagnostic code of the last failed render, if any.</summary>
    public string? DiagnosticCode { get; private set; }

    /// <summary>Transactional cost of the last render, in characters, as a coarse payload signal.</summary>
    public int OutputLength => Output.Length;

    /// <summary>Renders the current template and model.</summary>
    public void Render()
    {
        DiagnosticCode = null;

        var validation = _engine.ValidateSyntax(Template);
        if (!validation.IsValid)
        {
            var first = validation.Diagnostics[0];
            DiagnosticCode = first.Code;
            Output = $"{first.Code} at line {first.Line}, column {first.Column}: {first.Message}";
            return;
        }

        var context = new ExecutionContext();
        try
        {
            context.Model = TemplateModel.FromJson(Model);
        }
        catch (System.Text.Json.JsonException error)
        {
            DiagnosticCode = "TR004";
            Output = $"TR004: the model is not valid JSON - {error.Message}";
            return;
        }

        Output = _engine.Render(Template, context);
    }

    /// <summary>The C# a caller would write to render this template.</summary>
    public string CSharpSnippet =>
        $"var context = new ExecutionContext {{ Model = model }};\n" +
        $"var text = new TinyTemplateEngine().Render(\"{Template.Replace("\"", "\\\"")}\", context);";

    /// <summary>The workflow YAML equivalent.</summary>
    public string WorkflowSnippet =>
        "steps:\n  - id: render\n    type: template\n    template: template.tmpl\n    output: output.txt";

    /// <summary>The CLI equivalent.</summary>
    public string CommandSnippet => "tinytools render template.tmpl --model model.json --output output.txt";
}
