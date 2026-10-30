namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US01.ExecutionContext;

/// <summary>A registered template service that prefixes its input with its own name.</summary>
internal sealed class TestService : ITemplateService
{
    public string Name => "testservice";
    public object? Transform(object? input) => $"TEST:{input}";
}
