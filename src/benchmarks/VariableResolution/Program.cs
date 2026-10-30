using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace LowlandTech.TinyTools.Benchmarks.VariableResolution;

/// <summary>
/// The fixed variable-resolution workload: two thousand warmup and twenty thousand
/// measured <see cref="ITemplateEngine.ResolveVariables"/> operations over the same
/// context and template, for both alias variants. It writes one machine-readable row
/// per run and fails when the output is not byte-for-byte the expected text.
/// </summary>
internal static class Program
{
    private const string Template =
        "Customer: ${Context.Model.Customer.Address.City | upper}; Owner: ${Context.Owner | trim | upper}; Alias: ${Context.Alias ?? \"anonymous\"}";

    private const int Warmup = 2_000;
    private const int Measured = 20_000;

    private static int Main(string[] args)
    {
        var label = args.Length > 0 ? args[0] : "run";
        var engine = new TinyTemplateEngine();

        var results = new List<object>();
        foreach (var (variant, alias, expected) in Variants())
        {
            var context = Build(alias);

            var probe = engine.ResolveVariables(Template, context);
            if (!string.Equals(probe, expected, StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"{variant}: output mismatch. Expected '{expected}', got '{probe}'.");
                return 1;
            }

            for (var i = 0; i < Warmup; i++)
            {
                engine.ResolveVariables(Template, context);
            }

            var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < Measured; i++)
            {
                engine.ResolveVariables(Template, context);
            }

            watch.Stop();
            var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

            results.Add(new
            {
                label,
                variant,
                operations = Measured,
                ticks = watch.ElapsedTicks,
                throughput = Math.Round(Measured / watch.Elapsed.TotalSeconds, 2),
                allocatedBytesPerOperation = Math.Round((double)allocated / Measured, 2),
                outputBytes = Encoding.UTF8.GetByteCount(probe),
                output = probe,
            });
        }

        Console.WriteLine(JsonSerializer.Serialize(new
        {
            runtime = Environment.Version.ToString(),
            os = Environment.OSVersion.VersionString,
            processor = Environment.ProcessorCount,
            warmup = Warmup,
            measured = Measured,
            rows = results,
        }));

        return 0;
    }

    private static IEnumerable<(string Variant, string? Alias, string Expected)> Variants()
    {
        yield return ("alias-absent", null, "Customer: AMSTERDAM; Owner: ADA; Alias: anonymous");
        yield return ("alias-ops", "ops", "Customer: AMSTERDAM; Owner: ADA; Alias: ops");
    }

    private static ExecutionContext Build(string? alias)
    {
        var context = new ExecutionContext();
        context.Set("Model", new
        {
            Customer = new
            {
                Name = "Acme",
                Address = new { City = "Amsterdam", Country = "NL" },
            },
        });
        context.Set("Owner", " Ada ");
        if (alias is not null) context.Set("Alias", alias);
        return context;
    }
}
