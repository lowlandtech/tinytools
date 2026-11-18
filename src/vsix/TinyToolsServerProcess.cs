using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LowlandTech.TinyTools.Vsix
{
    /// <summary>
    /// Starts the language server that travels in this package.
    /// <para>
    /// The server is published as a framework-dependent application beside the
    /// manifest, so the extension starts it with the dotnet host and speaks the
    /// protocol over its standard streams.
    /// </para>
    /// </summary>
    internal static class TinyToolsServerProcess
    {
        /// <summary>The folder of the extension, which is where the server is packed.</summary>
        public static string InstallDirectory =>
            Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath);

        /// <summary>The server entry point inside the package.</summary>
        public static string ServerAssembly =>
            Path.Combine(InstallDirectory, "server", "tinytools-lsp.dll");

        /// <summary>
        /// Starts the server process, failing loudly when the package is missing
        /// the file it promised to carry.
        /// </summary>
        public static Process Start()
        {
            var assembly = ServerAssembly;
            if (!File.Exists(assembly))
            {
                throw new FileNotFoundException(
                    "The package does not carry its language server at " + assembly + ".", assembly);
            }

            var start = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "\"" + assembly + "\"",
                WorkingDirectory = InstallDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var process = Process.Start(start);
            if (process == null)
            {
                throw new InvalidOperationException("The language server process could not be started.");
            }

            return process;
        }
    }
}
