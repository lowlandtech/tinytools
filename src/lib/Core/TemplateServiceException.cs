namespace LowlandTech.TinyTools;

/// <summary>
/// Raised when an asynchronous template service fails during an asynchronous render. The
/// message names only the service, so a template author gets a stable diagnostic without
/// the service's internal exception text leaking into rendered output.
/// </summary>
public sealed class TemplateServiceException(string serviceName, Exception? innerException = null)
    : Exception($"Asynchronous template service '{serviceName}' failed.", innerException)
{
    /// <summary>The service key that failed.</summary>
    public string ServiceName { get; } = serviceName;
}
