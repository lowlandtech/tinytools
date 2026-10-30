namespace LowlandTech.TinyTools.Core.Abstractions;

public interface ITemplateFlowStep
{
    Task ExecuteAsync(object? context, TemplateStep step, object? instance, CancellationToken ct = default);
}
