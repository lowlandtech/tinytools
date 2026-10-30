namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US01.ExecutionContext;

/// <summary>A second registered template service used by the multiple-service registration case.</summary>
internal sealed class AnotherTestService : ITemplateService
{
    public string Name => "another";
    public object? Transform(object? input) => $"ANOTHER:{input}";
}
