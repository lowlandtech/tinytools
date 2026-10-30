#if NET10_0_OR_GREATER
using System.Net;
using System.Text;
using LowlandTech.TinyTools.Frontend.Services.Http;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

/// <summary>Answers each template route with a canned response and records the calls.</summary>
public sealed class RoutedTemplateHandler : HttpMessageHandler
{
    public static readonly Guid RefusingVersion = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private const string Template = """
        {"id":"5f1b2c3d-0000-4000-8000-000000000001","name":"EntityClass","isActive":true,"versions":[],"packages":[]}
        """;

    public List<string> Calls { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri!.AbsolutePath;
        Calls.Add($"{request.Method.Method} {path}");

        var (status, body) = (Path: path, Method: request.Method.Method) switch
        {
            var p when p.Path.EndsWith("/validate", StringComparison.Ordinal) && p.Path.Contains(RefusingVersion.ToString(), StringComparison.Ordinal)
                => (HttpStatusCode.BadRequest, """{"error":"The version carries no test data."}"""),
            var p when p.Path.EndsWith("/validate", StringComparison.Ordinal)
                => (HttpStatusCode.OK, """{"valid":true,"output":"o","expected":"e"}"""),
            var p when p.Path == HttpTemplateLibrary.RoutePrefix && p.Method == "POST"
                => (HttpStatusCode.Created, Template),
            var p when p.Method == "DELETE"
                => (HttpStatusCode.NoContent, string.Empty),
            var p when p.Path == HttpTemplateLibrary.RoutePrefix
                => (HttpStatusCode.OK, "[" + Template + "]"),
            _ => (HttpStatusCode.NotFound, string.Empty),
        };

        return Task.FromResult(new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        });
    }
}
#endif