using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Diagnostics.Client;

namespace Microsoft.Diagnostics.Tools.Trace
{
    [Command(Name = Name, Description = "Lists Event Sources that exist in the target process, and lists new ones as they are created.")]
    internal class SourcesCommand
    {
        public const string Name = "sources";

        [Option("-s|--server <SERVER>", Description = "The server to connect to, in the form of '<port>' (for localhost) or '<host>:<port>'")]
        public string Target { get; }

        public async Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
        {
            var cancellationToken = console.GetCtrlCToken();

            if (string.IsNullOrEmpty(Target))
            {
                console.Error.WriteLine("Missing required option: --server");
                return 1;
            }

            if (!EndPointParser.TryParseEndpoint(Target, out var endPoint))
            {
                console.Error.WriteLine($"Invalid server value: {Target}");
                return 1;
            }

            var client = new DiagnosticsClient(endPoint);

            console.WriteLine("Connecting to application...");

            client.OnEventSourceCreated += (eventSource) =>
            {
                console.WriteLine($"* {eventSource.Name} [{eventSource.Guid}] (settings: {eventSource.Settings})");
            };

            await client.ConnectAsync();

            console.WriteLine("Connected, press Ctrl-C to terminate...");
            await cancellationToken.WaitForCancellationAsync();

            return 0;
        }
    }
}
