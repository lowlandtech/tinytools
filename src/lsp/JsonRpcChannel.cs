using System.Text;
using System.Text.Json.Nodes;

namespace LowlandTech.TinyTools.LanguageServer;

/// <summary>
/// The header-framed JSON transport LSP uses over stdio: one <c>Content-Length</c> header, a blank
/// line, then that many bytes of UTF-8 JSON.
///
/// The reader works on raw bytes rather than a text reader: <c>Content-Length</c> counts bytes, and
/// a buffered text reader would both mis-count non-ASCII bodies and swallow the bytes of the next
/// message. One small buffer serves both the header scan and the body.
/// </summary>
public sealed class JsonRpcChannel(Stream input, Stream output, bool ownsStreams = false) : IDisposable
{
    private readonly byte[] _buffer = new byte[8192];
    private int _buffered;
    private int _offset;

    /// <summary>Reads one message, or null at end of stream.</summary>
    public async Task<JsonObject?> ReadAsync(CancellationToken cancellationToken = default)
    {
        var header = new StringBuilder();
        while (true)
        {
            var next = await NextByteAsync(cancellationToken).ConfigureAwait(false);
            if (next < 0) return null;

            header.Append((char)next);
            if (header.Length >= 4 && header.ToString(header.Length - 4, 4) == "\r\n\r\n") break;
        }

        var length = -1;
        foreach (var line in header.ToString().Split("\r\n", StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
            {
                length = int.Parse(line["Content-Length:".Length..].Trim(), System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        if (length < 0) return null;

        var body = new byte[length];
        for (var read = 0; read < length; read++)
        {
            var next = await NextByteAsync(cancellationToken).ConfigureAwait(false);
            if (next < 0) return null;
            body[read] = (byte)next;
        }

        return JsonNode.Parse(body) as JsonObject;
    }

    /// <summary>Writes one message with its header.</summary>
    public async Task WriteAsync(JsonObject message, CancellationToken cancellationToken = default)
    {
        var body = Encoding.UTF8.GetBytes(message.ToJsonString());
        var header = Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n");
        await output.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        await output.WriteAsync(body, cancellationToken).ConfigureAwait(false);
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<int> NextByteAsync(CancellationToken cancellationToken)
    {
        if (_offset >= _buffered)
        {
            _buffered = await input.ReadAsync(_buffer, cancellationToken).ConfigureAwait(false);
            _offset = 0;
            if (_buffered == 0) return -1;
        }

        return _buffer[_offset++];
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (ownsStreams)
        {
            input.Dispose();
            output.Dispose();
        }
    }
}
