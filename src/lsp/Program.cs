using LowlandTech.TinyTools.LanguageServer;

// stdio is the whole transport: the editor spawns this process and speaks LSP over its pipes.
using var input = Console.OpenStandardInput();
using var output = Console.OpenStandardOutput();
using var channel = new JsonRpcChannel(input, output);

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) => { eventArgs.Cancel = true; cancellation.Cancel(); };

var server = new TemplateLanguageServer();
await server.RunAsync(channel, cancellation.Token);
