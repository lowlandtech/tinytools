namespace LowlandTech.TinyTools;

public static class InterpolationExtensions
{
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
}
