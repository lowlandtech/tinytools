using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;

namespace LowlandTech.TinyTools.Vsix
{
    /// <summary>
    /// The content type the templates of this product are edited as.
    /// </summary>
    public static class TemplateContentType
    {
        /// <summary>The name the manifest and the client agree on.</summary>
        public const string Name = "tmpl";

        [Export]
        [Name(Name)]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition TemplateContentTypeDefinition;

        [Export]
        [ContentType(Name)]
        [FileExtension(".tmpl")]
        internal static FileExtensionToContentTypeDefinition TemplateFileExtension;
    }

    /// <summary>
    /// Connects Visual Studio to the language server this package ships.
    /// <para>
    /// The server is a separate process, so the extension carries it as content and
    /// starts it through the dotnet host rather than loading it into the editor.
    /// </para>
    /// </summary>
    [ContentType(TemplateContentType.Name)]
    [Export(typeof(ILanguageClient))]
    public sealed class TinyToolsLanguageClient : ILanguageClient
    {
        /// <summary>The name shown while the server starts.</summary>
        public string Name => "Tinytools template language server";

        /// <summary>
        /// The server publishes diagnostics on open, so it is started with the solution
        /// rather than on the first file a person opens.
        /// </summary>
        public IEnumerable<string> ConfigurationSections => null;

        /// <summary>No client-side options are exposed yet.</summary>
        public object InitializationOptions => null;

        /// <summary>The capabilities the server advertises are read from the server.</summary>
        public IEnumerable<string> FilesToWatch => null;

        /// <summary>Start the server when a template is edited, not only when a solution loads.</summary>
        public bool ShowNotificationOnInitializeFailed => true;

        /// <summary>Raised when the server reports it is ready.</summary>
        public event AsyncEventHandler<EventArgs> StartAsync;

        /// <summary>Raised when the server asks to be stopped.</summary>
        public event AsyncEventHandler<EventArgs> StopAsync;

        /// <summary>
        /// Describes how to start the server that travels in this package.
        /// </summary>
        public Task<Connection> ActivateAsync(CancellationToken token)
        {
            var process = TinyToolsServerProcess.Start();

            var connection = new Connection(
                process.StandardInput.BaseStream,
                process.StandardOutput.BaseStream);

            return Task.FromResult(connection);
        }

        /// <summary>Called once the server has started.</summary>
        public async Task OnLoadedAsync()
        {
            var handler = StartAsync;
            if (handler != null)
            {
                await handler.InvokeAsync(this, EventArgs.Empty);
            }
        }

        /// <summary>Called when the server cannot start; the reason is surfaced to the editor.</summary>
        public Task<InitializationFailureContext> OnServerInitializeFailedAsync(
            ILanguageClientInitializationInfo initializationInfo)
        {
            return Task.FromResult<InitializationFailureContext>(new InitializationFailureContext
            {
                FailureMessage = initializationInfo?.StatusMessage
                    ?? "The Tinytools language server could not be started.",
            });
        }

        /// <summary>Called when the server has initialised.</summary>
        public Task OnServerInitializedAsync()
        {
            return Task.CompletedTask;
        }
    }
}
