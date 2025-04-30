#if !IGNORE
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol.Messages;
using ModelContextProtocol.Server;
using R3;

namespace com.IvanMurzak.Unity.MCP.Server
{
    public class McpServerService : IHostedService
    {
        readonly ILogger<McpServerService> _logger;
        readonly IMcpServer _mcpServer;
        readonly IMcpRunner _mcpRunner;
        readonly IRemoteApp _remoteApp;
        readonly ILocalServer _localServer;
        readonly CompositeDisposable _disposables = new();

        public IMcpServer McpServer => _mcpServer;
        public IMcpRunner McpRunner => _mcpRunner;
        public IRemoteApp RemoteApp => _remoteApp;
        public ILocalServer LocalServer => _localServer;

        public static McpServerService? Instance { get; private set; }

        public McpServerService(ILogger<McpServerService> logger, IMcpServer mcpServer, IMcpRunner mcpRunner, IRemoteApp remoteApp, ILocalServer localServer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogTrace("Ctor.");
            _mcpServer = mcpServer ?? throw new ArgumentNullException(nameof(mcpServer));
            _mcpRunner = mcpRunner ?? throw new ArgumentNullException(nameof(mcpRunner));
            _remoteApp = remoteApp ?? throw new ArgumentNullException(nameof(remoteApp));
            _localServer = localServer ?? throw new ArgumentNullException(nameof(localServer));

            if (Instance != null)
                throw new InvalidOperationException($"{typeof(McpServerService).Name} is already initialized.");
            Instance = this;

            LocalServer.OnListToolUpdated
                .Subscribe(OnListToolUpdated)
                .AddTo(_disposables);

            // LocalServer.OnListResourcesUpdated
            //     .Subscribe(OnListResourcesUpdated)
            //     .AddTo(_disposables);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("StartAsync.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("StopAsync.");
            _disposables.Clear();
            Instance = null;
            return McpPlugin.StaticDisposeAsync();
        }

        async void OnListToolUpdated(Unit _)
        {
            try
            {
                var tools = _mcpServer.ServerOptions.Capabilities?.Tools?.ToolCollection;
                if (tools == null)
                {
                    _logger.LogError("Tools capability is not set. Cannot update tools.");
                    return;
                }
                // Fetch new tools from Plugin
                var request = new RequestListTool();
                var response = await _remoteApp.RunListTool(request);
                if (response.IsError)
                {
                    _logger.LogError("Failed to fetch tools from plugin: {Error}", response.Message);
                    return;
                }
                var pluginTools = response?.Value
                    ?.Select(x => x.ToMcpServerTool())
                    ?.ToArray() ?? Array.Empty<McpServerTool>();

                // Update the tools collection
                tools.Clear();
                foreach (var tool in pluginTools)
                    tools.Add(tool);

                await _mcpServer.SendNotificationAsync(NotificationMethods.ToolListChangedNotification);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating tools: {Message}", ex.Message);
            }
        }

        // async void OnListResourcesUpdated(Unit _)
        // {
        //     var resources = _mcpServer.ServerOptions.Capabilities?.Resources?.;
        //     if (resources == null)
        //     {
        //         _logger.LogError("Resources capability is not set. Cannot update resources.");
        //         return;
        //     }
        //     // Fetch new resources from Plugin
        //     var request = new RequestListResources();
        //     var response = await _remoteApp.RunListResources(request);
        //     if (response.IsError)
        //     {
        //         _logger.LogError("Failed to fetch resources from plugin: {Error}", response.Message);
        //         return;
        //     }
        //     var pluginResources = response?.Value
        //         ?.Select(x => x.ToResource())
        //         ?.ToArray() ?? Array.Empty<Resource>();


        //     // Update the resources collection
        //     resources.ResourceCollection.
        // }
    }
}
#endif