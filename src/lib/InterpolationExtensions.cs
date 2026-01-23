namespace LowlandTech.TinyTools;

public static class InterpolationExtensions
{
    private static readonly Lazy<ITemplateEngine> DefaultEngine = new(() => new TinyTemplateEngine());

    public static string Interpolate<T>(this string template, T model, bool hasTags = true)
    {
        Guard.Against.NullOrEmpty(template, nameof(template), "Template should be supplied.");
        Guard.Against.Null(model, nameof(model), "Model should be supplied.");

        if (model is IDictionary<string, string> dictionary)
        {
            foreach (var entry in dictionary)
            {
                if (hasTags)
                {
                    template = template.Replace($"{{{entry.Key}}}", entry.Value);
                }
                else
                {
                    template = template.Replace($"{entry.Key}", entry.Value);
                }
            }
        }
        else
        {
            var props = model.GetType().GetProperties();
            foreach (var prop in props.Where(p => p.GetValue(model) != null))
            {
                if (hasTags)
                {
                    template = template.Replace($"{{{prop.Name}}}", prop.GetValue(model)!.ToString());
                }
                else
                {
                    template = template.Replace($"{prop.Name}", prop.GetValue(model)!.ToString());
                }
            }
        }
        return template;
    }

    /// <summary>
    /// Interpolates a template string using the TinyTemplateEngine with full support for
    /// ${Context.xxx} variable interpolation and @if/@foreach control flow.
    /// </summary>
    /// <param name="template">The template string to interpolate.</param>
    /// <param name="context">The pipeline execution context containing variables and model.</param>
    /// <param name="engine">Optional custom template engine. Uses default TinyTemplateEngine if not provided.</param>
    /// <returns>The interpolated string.</returns>
    public static string Interpolate(this string template, ExecutionContext context, ITemplateEngine? engine = null)
    {
        Guard.Against.NullOrEmpty(template, nameof(template), "Template should be supplied.");
        Guard.Against.Null(context, nameof(context), "Context should be supplied.");

        engine ??= DefaultEngine.Value;
        return engine.Render(template, context);
    }

    /// <summary>
    /// Interpolates a template string using the TinyTemplateEngine, creating a context from the provided model.
    /// Supports ${Context.xxx} variable interpolation and @if/@foreach control flow.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <param name="template">The template string to interpolate.</param>
    /// <param name="model">The model object to use for interpolation.</param>
    /// <param name="engine">Optional custom template engine. Uses default TinyTemplateEngine if not provided.</param>
    /// <returns>The interpolated string.</returns>
    public static string InterpolateWithEngine<T>(this string template, T model, ITemplateEngine? engine = null)
    {
        Guard.Against.NullOrEmpty(template, nameof(template), "Template should be supplied.");
        Guard.Against.Null(model, nameof(model), "Model should be supplied.");

        var context = new ExecutionContext { Model = model };
        engine ??= DefaultEngine.Value;
        return engine.Render(template, context);
    }

    public static List<string> Interpolate<T>(this List<string> templates, T model, bool hasTags = false, bool isFile = false)
    {
        var results = new List<string>();

        foreach (var template in templates)
        {
            var result = template.Interpolate(model, hasTags);
            results.Add(result);

            if (!isFile || !File.Exists(template)) continue;

            var text = File.ReadAllText(template).Interpolate(model, hasTags);
            File.WriteAllText(template, text);
            File.Move(template, result);
        }
        return results;
    }

    /// <summary>
    /// Interpolates a list of template strings using the TinyTemplateEngine.
    /// Supports ${Context.xxx} variable interpolation and @if/@foreach control flow.
    /// </summary>
    /// <param name="templates">The list of template strings to interpolate.</param>
    /// <param name="context">The pipeline execution context containing variables and model.</param>
    /// <param name="engine">Optional custom template engine. Uses default TinyTemplateEngine if not provided.</param>
    /// <returns>A list of interpolated strings.</returns>
    public static List<string> Interpolate(this List<string> templates, ExecutionContext context, ITemplateEngine? engine = null)
    {
        Guard.Against.Null(templates, nameof(templates), "Templates should be supplied.");
        Guard.Against.Null(context, nameof(context), "Context should be supplied.");

        engine ??= DefaultEngine.Value;
        return templates.Select(t => engine.Render(t, context)).ToList();
    }
}
