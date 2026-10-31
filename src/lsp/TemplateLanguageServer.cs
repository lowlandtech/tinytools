using System.Text.Json;
using System.Text.Json.Nodes;

namespace LowlandTech.TinyTools.LanguageServer;

/// <summary>
/// The TinyTools language server: diagnostics, completion and preview over LSP 3.17 on stdio.
///
/// The server owns no project system, so the host declares the model it edits against through
/// <c>initializationOptions.modelType</c> (resolved from the loaded assemblies) or
/// <c>initializationOptions.modelMembers</c> (explicit dotted member names). Everything else - the
/// diagnostics and the preview - comes from the engine this repository ships.
/// </summary>
public sealed class TemplateLanguageServer(TextWriter? log = null)
{
    private readonly TinyTemplateEngine _engine = new();
    private readonly Dictionary<string, string> _documents = new(StringComparer.Ordinal);
    private readonly List<string> _members = new();
    private JsonRpcChannel _channel = null!;

    /// <summary>Bumped for every document revision the server accepts.</summary>
    public int Revision { get; private set; }

    /// <summary>Runs the protocol loop until the client disconnects.</summary>
    public async Task RunAsync(JsonRpcChannel channel, CancellationToken cancellationToken = default)
    {
        _channel = channel;
        while (!cancellationToken.IsCancellationRequested)
        {
            var message = await channel.ReadAsync(cancellationToken).ConfigureAwait(false);
            if (message is null) return;

            await DispatchAsync(message, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task DispatchAsync(JsonObject message, CancellationToken cancellationToken)
    {
        var method = message["method"]?.GetValue<string>();
        var id = message["id"];
        var parameters = message["params"] as JsonObject;

        try
        {
            await RouteAsync(method, id, parameters, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception error) when (error is InvalidOperationException or FormatException
                                          or ArgumentException or JsonException)
        {
            // An editor is allowed to send anything. A request it cannot serve must come back as a
            // JSON-RPC error, not as a dead process: this used to throw out of the read loop and
            // take the whole server down on a single malformed argument.
            log?.WriteLine("rejected " + method + ": " + error.Message);
            if (id is not null)
            {
                await ReplyErrorAsync(id, -32602, "Invalid params for " + method + ": " + error.Message,
                    cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task RouteAsync(string? method, JsonNode? id, JsonObject? parameters,
        CancellationToken cancellationToken)
    {
        switch (method)
        {
            case "initialize":
                DescribeModel(parameters?["initializationOptions"] as JsonObject);
                await ReplyAsync(id, new JsonObject
                {
                    ["capabilities"] = new JsonObject
                    {
                        ["textDocumentSync"] = 1,
                        ["completionProvider"] = new JsonObject { ["triggerCharacters"] = new JsonArray("$", ".") },
                        ["executeCommandProvider"] = new JsonObject { ["commands"] = new JsonArray(PreviewCommand) },
                    },
                    ["serverInfo"] = new JsonObject { ["name"] = "tinytools-lsp", ["version"] = "1.0" },
                }, cancellationToken).ConfigureAwait(false);
                return;

            case "initialized":
                return;

            case "shutdown":
                await ReplyAsync(id, null, cancellationToken).ConfigureAwait(false);
                return;

            case "textDocument/didOpen":
                await OpenAsync(parameters, cancellationToken).ConfigureAwait(false);
                return;

            case "textDocument/didChange":
                await OpenAsync(parameters, cancellationToken).ConfigureAwait(false);
                return;

            case "textDocument/completion":
                await ReplyAsync(id, Complete(parameters), cancellationToken).ConfigureAwait(false);
                return;

            case "workspace/executeCommand":
                if (parameters?["command"]?.GetValue<string>() == PreviewCommand)
                {
                    await ReplyAsync(id, Preview(parameters), cancellationToken).ConfigureAwait(false);
                    return;
                }

                await ReplyAsync(id, null, cancellationToken).ConfigureAwait(false);
                return;

            default:
                if (id is not null) await ReplyAsync(id, null, cancellationToken).ConfigureAwait(false);
                return;
        }
    }

    /// <summary>The single command this server exposes.</summary>
    public const string PreviewCommand = "tinytools.preview";

    private void DescribeModel(JsonObject? options)
    {
        _members.Clear();
        if (options?["modelMembers"] is JsonArray explicitMembers)
        {
            foreach (var member in explicitMembers)
            {
                var name = member?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(name)) _members.Add(name!);
            }
        }

        var typeName = options?["modelType"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(typeName)) return;

        var type = Type.GetType(typeName!, throwOnError: false)
                   ?? AppDomain.CurrentDomain.GetAssemblies()
                       .Select(assembly => assembly.GetType(typeName!, throwOnError: false))
                       .FirstOrDefault(found => found is not null);

        if (type is null) return;

        foreach (var property in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            _members.Add(property.Name);
        }
    }

    private async Task OpenAsync(JsonObject? parameters, CancellationToken cancellationToken)
    {
        var document = parameters?["textDocument"] as JsonObject;
        var uri = document?["uri"]?.GetValue<string>();
        var text = document?["text"]?.GetValue<string>()
                   ?? (parameters?["contentChanges"] is JsonArray changes ? changes[^1]?["text"]?.GetValue<string>() : null);

        if (uri is null || text is null) return;

        _documents[uri] = text;
        Revision++;

        await _channel.WriteAsync(new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["method"] = "textDocument/publishDiagnostics",
            ["params"] = new JsonObject
            {
                ["uri"] = uri,
                ["diagnostics"] = DiagnosticsFor(text),
            },
        }, cancellationToken).ConfigureAwait(false);
    }

    private JsonArray DiagnosticsFor(string text)
    {
        var diagnostics = new JsonArray();
        var result = _engine.ValidateSyntax(text);
        foreach (var diagnostic in result.Diagnostics)
        {
            diagnostics.Add(new JsonObject
            {
                ["range"] = new JsonObject
                {
                    ["start"] = new JsonObject { ["line"] = diagnostic.Line - 1, ["character"] = diagnostic.Column - 1 },
                    ["end"] = new JsonObject { ["line"] = diagnostic.Line - 1, ["character"] = diagnostic.Column },
                },
                ["severity"] = 1,
                ["code"] = diagnostic.Code,
                ["message"] = diagnostic.Message,
                ["source"] = "tinytools",
            });
        }

        return diagnostics;
    }

    private JsonArray Complete(JsonObject? parameters)
    {
        var prefix = parameters?["context"]?["triggerCharacter"]?.GetValue<string>() ?? string.Empty;
        var items = new JsonArray();

        if (prefix == ".")
        {
            foreach (var member in _members.Distinct(StringComparer.Ordinal))
            {
                items.Add(new JsonObject { ["label"] = member, ["kind"] = 10 });
            }

            return items;
        }

        foreach (var helper in TemplateHelpers.Helpers.Keys.OrderBy(name => name, StringComparer.Ordinal))
        {
            items.Add(new JsonObject { ["label"] = helper, ["kind"] = 3, ["detail"] = "pipe helper" });
        }

        return items;
    }

    private JsonObject Preview(JsonObject? parameters)
    {
        var arguments = parameters?["arguments"] as JsonArray;
        var uri = TryReadArgument<string>(arguments, 0, out var requestedUri) ? requestedUri : null;
        var revision = TryReadArgument<int>(arguments, 1, out var requestedRevision) ? requestedRevision : Revision;

        if (uri is null || !_documents.TryGetValue(uri, out var text))
        {
            return new JsonObject { ["revision"] = revision, ["text"] = string.Empty, ["stale"] = true };
        }

        var context = new ExecutionContext();
        var rendered = _engine.Render(text, context);
        return new JsonObject { ["revision"] = revision, ["text"] = rendered, ["stale"] = revision < Revision };
    }

    /// <summary>Reads a command argument defensively; a client may send any shape it likes.</summary>
    private static bool TryReadArgument<T>(JsonArray? arguments, int index, out T value)
    {
        value = default!;
        if (arguments is null || index >= arguments.Count) return false;
        return arguments[index] is JsonValue node && node.TryGetValue<T>(out value!);
    }

    private async Task ReplyAsync(JsonNode? id, JsonNode? result, CancellationToken cancellationToken) =>
        await _channel.WriteAsync(new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id?.DeepClone(),
            ["result"] = result?.DeepClone(),
        }, cancellationToken).ConfigureAwait(false);

    private async Task ReplyErrorAsync(JsonNode id, int code, string message, CancellationToken cancellationToken) =>
        await _channel.WriteAsync(new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id.DeepClone(),
            ["error"] = new JsonObject { ["code"] = code, ["message"] = message },
        }, cancellationToken).ConfigureAwait(false);
}
