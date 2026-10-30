using System.Text;
using System.Text.Json.Nodes;
using LowlandTech.TinyTools.LanguageServer;

namespace LowlandTech.TinyTools.Tests.SPEC3324.LanguageServer.US01.Protocol.UseCases;

/// <summary>Caller goal and captured result vocabulary only; executable API usage stays in each scenario.</summary>
public sealed class LanguageServerUseCase
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
}

/// <summary>The model the fake editor declares through initialization options.</summary>
public sealed class ServerModel
{
    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
}

/// <summary>
/// Drives the server over a buffered loopback pair: every message is written up front, the server
/// runs to end of input, and the responses are read back in order. No sleeps and no stream rewinds.
/// </summary>
public static class ServerHarness
{
    public static (JsonObject Initialize, JsonObject Diagnostics, JsonObject Members, JsonObject Helpers, JsonObject Preview) Exchange()
    {
        using var input = new MemoryStream();
        using var output = new MemoryStream();

        Write(input, "initialize", 1, new JsonObject
        {
            ["initializationOptions"] = new JsonObject { ["modelType"] = typeof(ServerModel).FullName },
        });
        Write(input, "textDocument/didOpen", null, new JsonObject
        {
            ["textDocument"] = new JsonObject { ["uri"] = "file:///broken.tmpl", ["text"] = "a\n@else\nb" },
        });
        Write(input, "textDocument/completion", 3, new JsonObject
        {
            ["context"] = new JsonObject { ["triggerCharacter"] = "." },
        });
        Write(input, "textDocument/completion", 4, new JsonObject
        {
            ["context"] = new JsonObject { ["triggerCharacter"] = "|" },
        });
        Write(input, "textDocument/didOpen", null, new JsonObject
        {
            ["textDocument"] = new JsonObject { ["uri"] = "file:///hello.tmpl", ["text"] = "Hello Ada" },
        });
        Write(input, "workspace/executeCommand", 6, new JsonObject
        {
            ["command"] = "tinytools.preview",
            ["arguments"] = new JsonArray("file:///hello.tmpl", 0),
        });

        input.Position = 0;
        new TemplateLanguageServer().RunAsync(new JsonRpcChannel(input, output)).GetAwaiter().GetResult();

        var messages = Read(output);
        JsonObject ById(int id) => messages.FirstOrDefault(message => message["id"]?.GetValue<int>() == id) ?? new JsonObject();
        JsonObject ByMethod(string method) => messages.FirstOrDefault(message => message["method"]?.GetValue<string>() == method) ?? new JsonObject();

        return (ById(1), ByMethod("textDocument/publishDiagnostics"), ById(3), ById(4), ById(6));
    }

    private static void Write(Stream stream, string method, int? id, JsonObject parameters)
    {
        var message = new JsonObject { ["jsonrpc"] = "2.0", ["method"] = method, ["params"] = parameters };
        if (id is not null) message["id"] = id.Value;

        var body = Encoding.UTF8.GetBytes(message.ToJsonString());
        var header = Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n");
        stream.Write(header);
        stream.Write(body);
    }

    private static List<JsonObject> Read(Stream stream)
    {
        var messages = new List<JsonObject>();
        var bytes = ((MemoryStream)stream).ToArray();
        var text = Encoding.UTF8.GetString(bytes);
        var index = 0;

        while (index < text.Length)
        {
            var headerEnd = text.IndexOf("\r\n\r\n", index, StringComparison.Ordinal);
            if (headerEnd < 0) break;

            var header = text[index..headerEnd];
            var lengthLine = header.Split("\r\n").FirstOrDefault(line => line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase));
            if (lengthLine is null) break;

            var length = int.Parse(lengthLine["Content-Length:".Length..].Trim(), System.Globalization.CultureInfo.InvariantCulture);
            var bodyStart = headerEnd + 4;
            if (bodyStart + length > text.Length) break;

            var message = JsonNode.Parse(text.Substring(bodyStart, length)) as JsonObject;
            if (message is not null) messages.Add(message);
            index = bodyStart + length;
        }

        return messages;
    }
}
