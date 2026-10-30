using System.Diagnostics;
using System.Text.Json;

using System.Diagnostics;
using System.Text.Json;

namespace LowlandTech.TinyTools.Tests.SPEC3320.Gates.US01.Vendored.UseCases;

/// <summary>Caller goal and captured result vocabulary only; executable API usage stays in each scenario.</summary>
public sealed class VendoredGatesUseCase
{
    private bool executed;

    public Dictionary<string, object?> Cases { get; } = new(StringComparer.Ordinal);

    public void RunOnce(Action given, Action when)
    {
        lock (Cases)
        {
            if (executed) return;
            given();
            when();
            executed = true;
        }
    }

    /// <summary>The repository root, found by walking up to the solution file.</summary>
    public static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "tinytools.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not reachable.");
    }

    /// <summary>Runs a vendored gate and captures its combined output.</summary>
    public static (int Exit, string Output) Run(string script, string[] arguments, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo("python")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add(script);
        foreach (var argument in arguments) startInfo.ArgumentList.Add(argument);

        using var process = Process.Start(startInfo)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        process.WaitForExit(180_000);
        return (process.ExitCode, output.GetAwaiter().GetResult() + error.GetAwaiter().GetResult());
    }

    /// <summary>Counts the layout violations a JSON report contains, or -1 when it is not JSON.</summary>
    public static int CountViolations(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.TryGetProperty("violations", out var violations) ? violations.GetArrayLength() : -1;
        }
        catch (JsonException)
        {
            return -1;
        }
    }}
