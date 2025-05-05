#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data;
using com.IvanMurzak.Unity.MCP.Common.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace com.IvanMurzak.Unity.MCP.Server
{
    public class RemoteApp : BaseHub<RemoteApp>, IRemoteApp
    {
        // Add static property for connected clients
        private static readonly List<string> _connectedClients = new List<string>();
        
        // Static property to access connected clients
        public static IReadOnlyList<string> AllConnections => _connectedClients;
        
        // Static property to get the first connected client
        public static string? FirstConnectionId => _connectedClients.FirstOrDefault();
        
        // Add client connection tracking methods
        public override Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            if (!string.IsNullOrEmpty(connectionId) && !_connectedClients.Contains(connectionId))
            {
                _connectedClients.Add(connectionId);
                _logger.LogInformation($"Client connected: {connectionId}. Total clients: {_connectedClients.Count}");
            }
            return base.OnConnectedAsync();
        }
        
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            if (!string.IsNullOrEmpty(connectionId) && _connectedClients.Contains(connectionId))
            {
                _connectedClients.Remove(connectionId);
                _logger.LogInformation($"Client disconnected: {connectionId}. Total clients: {_connectedClients.Count}");
            }
            return base.OnDisconnectedAsync(exception);
        }
        
        // Helper methods for active client management
        private ISingleClientProxy? GetActiveClient()
        {
            var connectionId = FirstConnectionId;
            return string.IsNullOrEmpty(connectionId) 
                ? null 
                : Clients.Client(connectionId);
        }
        
        private void RemoveCurrentClient(IClientProxy client)
        {
            // The current approach doesn't allow direct removal by IClientProxy reference
            // We'd need the actual connectionId, which we don't have here
            // This is a placeholder for the original method call
            _logger.LogWarning("RemoveCurrentClient called but cannot remove by IClientProxy reference");
        }

        public RemoteApp(ILogger<RemoteApp> logger, IHubContext<RemoteApp> hubContext)
            : base(logger, hubContext)
        {
        }

        public async Task<IResponseData<ResponseCallTool>> RunCallTool(IRequestCallTool data, CancellationToken cancellationToken = default)
        {
            if (data == null)
                return ResponseData<ResponseCallTool>.Error(com.IvanMurzak.Unity.MCP.Common.Utils.Consts.Guid.Zero, "Tool data is null.")
                    .Log(_logger);

            if (string.IsNullOrEmpty(data.Name))
                return ResponseData<ResponseCallTool>.Error(data.RequestID, "Tool.Name is null.")
                    .Log(_logger);
            try
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    var message = data.Arguments == null
                        ? $"Run tool '{data.Name}' with no parameters."
                        : $"Run tool '{data.Name}' with parameters[{data.Arguments.Count}]:\n{string.Join(",\n", data.Arguments)}";
                    _logger.LogInformation(message);
                }

                const int maxRetries = 10; // Maximum number of retries
                var retryCount = 0;        // Retry counter

                while (retryCount < maxRetries)
                {
                    retryCount++;
                    var client = GetActiveClient();
                    if (client == null)
                    {
                        _logger.LogWarning($"No connected clients for {GetType().Name}. Retrying [{retryCount}/{maxRetries}]...");
                        await Task.Delay(1000, cancellationToken); // Wait before retrying
                        continue;
                    }

                    var invokeTask = client.InvokeAsync<ResponseData<ResponseCallTool>>(com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunCallTool, data, cancellationToken);
                    var completedTask = await Task.WhenAny(invokeTask, Task.Delay(TimeSpan.FromSeconds(com.IvanMurzak.Unity.MCP.Common.Utils.Consts.Hub.TimeoutSeconds), cancellationToken));
                    if (completedTask == invokeTask)
                    {
                        try
                        {
                            var result = await invokeTask;
                            if (result == null)
                                return ResponseData<ResponseCallTool>.Error(data.RequestID, $"Tool '{data.Name}' returned null result.")
                                    .Log(_logger);

                            return result;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error invoking tool '{data.Name}' on client '{Context?.ConnectionId}': {ex.Message}");
                            RemoveCurrentClient(client);
                            continue;
                        }
                    }

                    // Timeout occurred
                    _logger.LogWarning($"Timeout: Client '{Context?.ConnectionId}' did not respond in {com.IvanMurzak.Unity.MCP.Common.Utils.Consts.Hub.TimeoutSeconds} seconds. Removing from ConnectedClients.");
                    RemoveCurrentClient(client);
                    // Restart the loop to try again with a new client
                }
                return ResponseData<ResponseCallTool>.Error(data.RequestID, $"Failed to run tool '{data.Name}' after {maxRetries} retries.")
                    .Log(_logger);
            }
            catch (Exception ex)
            {
                return ResponseData<ResponseCallTool>.Error(data.RequestID, $"Failed to run tool '{data.Name}'. Exception: {ex}")
                    .Log(_logger, ex);
            }
        }

        public async Task<IResponseData<ResponseListTool[]>> RunListTool(IRequestListTool data, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = GetActiveClient();
                if (client == null)
                    return ResponseData<ResponseListTool[]>.Error(data.RequestID, $"No connected clients for {GetType().Name}.")
                        .Log(_logger);

                var result = await client.InvokeAsync<ResponseData<ResponseListTool[]>>(com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunListTool, data, cancellationToken);
                if (result == null)
                    return ResponseData<ResponseListTool[]>.Error(data.RequestID, $"'{com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunListTool}' returned null result.")
                        .Log(_logger);

                return result;
            }
            catch (Exception ex)
            {
                return ResponseData<ResponseListTool[]>.Error(data.RequestID, $"Failed to run '{com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunListTool}'. Exception: {ex}")
                    .Log(_logger, ex);
            }
        }

        public async Task<IResponseData<ResponseResourceContent[]>> RunResourceContent(IRequestResourceContent data, CancellationToken cancellationToken = default)
        {
            if (data == null)
                return ResponseData<ResponseResourceContent[]>.Error(com.IvanMurzak.Unity.MCP.Common.Utils.Consts.Guid.Zero, "Resource content data is null.")
                    .Log(_logger);

            if (string.IsNullOrEmpty(data.Uri))
                return ResponseData<ResponseResourceContent[]>.Error(data.RequestID, "Resource content Uri is null.")
                    .Log(_logger);

            try
            {
                var client = GetActiveClient();
                if (client == null)
                    return ResponseData<ResponseResourceContent[]>.Error(data.RequestID, $"No connected clients for {GetType().Name}.")
                        .Log(_logger);

                var result = await client.InvokeAsync<ResponseData<ResponseResourceContent[]>>(com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunResourceContent, data, cancellationToken);
                if (result == null)
                    return ResponseData<ResponseResourceContent[]>.Error(data.RequestID, $"Resource uri: '{data.Uri}' returned null result.")
                        .Log(_logger);

                return result;
            }
            catch (Exception ex)
            {
                return ResponseData<ResponseResourceContent[]>.Error(data.RequestID, $"Failed to get resource uri: '{data.Uri}'. Exception: {ex}")
                    .Log(_logger, ex);
            }
        }

        public async Task<IResponseData<ResponseListResource[]>> RunListResources(IRequestListResources data, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = GetActiveClient();
                if (client == null)
                    return ResponseData<ResponseListResource[]>.Error(data.RequestID, $"No connected clients for {GetType().Name}.")
                        .Log(_logger);

                var result = await client.InvokeAsync<ResponseData<ResponseListResource[]>>(com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunListResources, data, cancellationToken);
                if (result == null)
                    return ResponseData<ResponseListResource[]>.Error(data.RequestID, $"'{com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunListResources}' returned null result.")
                        .Log(_logger);

                return result;
            }
            catch (Exception ex)
            {
                return ResponseData<ResponseListResource[]>.Error(data.RequestID, $"Failed to run '{com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunListResources}'. Exception: {ex}")
                    .Log(_logger, ex);
            }
        }

        public async Task<IResponseData<ResponseResourceTemplate[]>> RunResourceTemplates(IRequestListResourceTemplates data, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = GetActiveClient();
                if (client == null)
                    return ResponseData<ResponseResourceTemplate[]>.Error(data.RequestID, $"No connected clients for {GetType().Name}.")
                        .Log(_logger);

                var result = await client.InvokeAsync<ResponseData<ResponseResourceTemplate[]>>(com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunListResourceTemplates, data, cancellationToken);
                if (result == null)
                    return ResponseData<ResponseResourceTemplate[]>.Error(data.RequestID, $"'{com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunListResourceTemplates}' returned null result.")
                        .Log(_logger);

                return result;
            }
            catch (Exception ex)
            {
                return ResponseData<ResponseResourceTemplate[]>.Error(data.RequestID, $"Failed to run '{com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunListResourceTemplates}'. Exception: {ex}")
                    .Log(_logger, ex);
            }
        }

        public async Task<IResponseData<ResponseMenuItem[]>> RunListMenuItems(IRequestListMenuItems data, string? connectionId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = GetActiveClient();
                if (client == null)
                    return ResponseData<ResponseMenuItem[]>.Error(data.RequestID, $"No connected clients for {GetType().Name}.")
                        .Log(_logger);

                var result = await client.InvokeAsync<ResponseData<ResponseMenuItem[]>>(com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunListMenuItems, data, cancellationToken);
                if (result == null)
                    return ResponseData<ResponseMenuItem[]>.Error(data.RequestID, $"'{com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunListMenuItems}' returned null result.")
                        .Log(_logger);

                return result;
            }
            catch (Exception ex)
            {
                return ResponseData<ResponseMenuItem[]>.Error(data.RequestID, $"Failed to run '{com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunListMenuItems}'. Exception: {ex}")
                    .Log(_logger, ex);
            }
        }

        public async Task<IResponseData<ResponseExecuteMenuItem>> RunExecuteMenuItem(IRequestExecuteMenuItem data, string? connectionId = null, CancellationToken cancellationToken = default)
        {
            if (data == null)
                return ResponseData<ResponseExecuteMenuItem>.Error(com.IvanMurzak.Unity.MCP.Common.Utils.Consts.Guid.Zero, "Execute menu item data is null.")
                    .Log(_logger);

            if (string.IsNullOrEmpty(data.MenuPath))
                return ResponseData<ResponseExecuteMenuItem>.Error(data.RequestID, "Menu path is null.")
                    .Log(_logger);

            try
            {
                var client = GetActiveClient();
                if (client == null)
                    return ResponseData<ResponseExecuteMenuItem>.Error(data.RequestID, $"No connected clients for {GetType().Name}.")
                        .Log(_logger);

                var result = await client.InvokeAsync<ResponseData<ResponseExecuteMenuItem>>(com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunExecuteMenuItem, data, cancellationToken);
                if (result == null)
                    return ResponseData<ResponseExecuteMenuItem>.Error(data.RequestID, $"Menu path: '{data.MenuPath}' returned null result.")
                        .Log(_logger);

                return result;
            }
            catch (Exception ex)
            {
                return ResponseData<ResponseExecuteMenuItem>.Error(data.RequestID, $"Failed to execute menu path: '{data.MenuPath}'. Exception: {ex}")
                    .Log(_logger, ex);
            }
        }

        public new void Dispose()
        {
            base.Dispose();
        }

        public Task<IResponseData<string>> OnListToolsUpdated(string data)
        {
            _logger.LogInformation("OnListToolsUpdated called");
            return Task.FromResult<IResponseData<string>>(ResponseData<string>.Success(com.IvanMurzak.Unity.MCP.Common.Utils.Consts.Guid.Zero, "Tools updated"));
        }

        public Task<IResponseData<string>> OnListResourcesUpdated(string data)
        {
            _logger.LogInformation("OnListResourcesUpdated called");
            return Task.FromResult<IResponseData<string>>(ResponseData<string>.Success(com.IvanMurzak.Unity.MCP.Common.Utils.Consts.Guid.Zero, "Resources updated"));
        }
    }
}