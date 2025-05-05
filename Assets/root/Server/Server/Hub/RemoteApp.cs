#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Threading;
using System.Threading.Tasks;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace com.IvanMurzak.Unity.MCP.Server
{
    public class RemoteApp : BaseHub<RemoteApp>, IRemoteApp
    {
        public RemoteApp(ILogger<RemoteApp> logger, IHubContext<RemoteApp> hubContext)
            : base(logger, hubContext)
        {
        }

        public async Task<IResponseData<ResponseCallTool>> RunCallTool(IRequestCallTool data, CancellationToken cancellationToken = default)
        {
            if (data == null)
                return ResponseData<ResponseCallTool>.Error(Consts.Guid.Zero, "Tool data is null.")
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

                    var invokeTask = client.InvokeAsync<ResponseData<ResponseCallTool>>(Consts.RPC.Client.RunCallTool, data, cancellationToken);
                    var completedTask = await Task.WhenAny(invokeTask, Task.Delay(TimeSpan.FromSeconds(Consts.Hub.TimeoutSeconds), cancellationToken));
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
                    _logger.LogWarning($"Timeout: Client '{Context?.ConnectionId}' did not respond in {Consts.Hub.TimeoutSeconds} seconds. Removing from ConnectedClients.");
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

                var result = await client.InvokeAsync<ResponseData<ResponseListTool[]>>(Consts.RPC.Client.RunListTool, data, cancellationToken);
                if (result == null)
                    return ResponseData<ResponseListTool[]>.Error(data.RequestID, $"'{Consts.RPC.Client.RunListTool}' returned null result.")
                        .Log(_logger);

                return result;
            }
            catch (Exception ex)
            {
                return ResponseData<ResponseListTool[]>.Error(data.RequestID, $"Failed to run '{Consts.RPC.Client.RunListTool}'. Exception: {ex}")
                    .Log(_logger, ex);
            }
        }

        public async Task<IResponseData<ResponseResourceContent[]>> RunResourceContent(IRequestResourceContent data, CancellationToken cancellationToken = default)
        {
            if (data == null)
                return ResponseData<ResponseResourceContent[]>.Error(Consts.Guid.Zero, "Resource content data is null.")
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

                var result = await client.InvokeAsync<ResponseData<ResponseResourceContent[]>>(Consts.RPC.Client.RunResourceContent, data, cancellationToken);
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

                var result = await client.InvokeAsync<ResponseData<ResponseListResource[]>>(Consts.RPC.Client.RunListResources, data, cancellationToken);
                if (result == null)
                    return ResponseData<ResponseListResource[]>.Error(data.RequestID, $"'{Consts.RPC.Client.RunListResources}' returned null result.")
                        .Log(_logger);

                return result;
            }
            catch (Exception ex)
            {
                return ResponseData<ResponseListResource[]>.Error(data.RequestID, $"Failed to run '{Consts.RPC.Client.RunListResources}'. Exception: {ex}")
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

                var result = await client.InvokeAsync<ResponseData<ResponseResourceTemplate[]>>(Consts.RPC.Client.RunListResourceTemplates, data, cancellationToken);
                if (result == null)
                    return ResponseData<ResponseResourceTemplate[]>.Error(data.RequestID, $"'{Consts.RPC.Client.RunListResourceTemplates}' returned null result.")
                        .Log(_logger);

                return result;
            }
            catch (Exception ex)
            {
                return ResponseData<ResponseResourceTemplate[]>.Error(data.RequestID, $"Failed to run '{Consts.RPC.Client.RunListResourceTemplates}'. Exception: {ex}")
                    .Log(_logger, ex);
            }
        }

        public async Task<IResponseData<ResponseMenuItem[]>> RunListMenuItems(IRequestListMenuItems data, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = GetActiveClient();
                if (client == null)
                    return ResponseData<ResponseMenuItem[]>.Error(data.RequestID, $"No connected clients for {GetType().Name}.")
                        .Log(_logger);

                var result = await client.InvokeAsync<ResponseData<ResponseMenuItem[]>>(Consts.RPC.Client.RunListMenuItems, data, cancellationToken);
                if (result == null)
                    return ResponseData<ResponseMenuItem[]>.Error(data.RequestID, $"'{Consts.RPC.Client.RunListMenuItems}' returned null result.")
                        .Log(_logger);

                return result;
            }
            catch (Exception ex)
            {
                return ResponseData<ResponseMenuItem[]>.Error(data.RequestID, $"Failed to run '{Consts.RPC.Client.RunListMenuItems}'. Exception: {ex}")
                    .Log(_logger, ex);
            }
        }

        public async Task<IResponseData<ResponseExecuteMenuItem>> RunExecuteMenuItem(IRequestExecuteMenuItem data, CancellationToken cancellationToken = default)
        {
            if (data == null)
                return ResponseData<ResponseExecuteMenuItem>.Error(Consts.Guid.Zero, "Execute menu item data is null.")
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

                var result = await client.InvokeAsync<ResponseData<ResponseExecuteMenuItem>>(Consts.RPC.Client.RunExecuteMenuItem, data, cancellationToken);
                if (result == null)
                    return ResponseData<ResponseExecuteMenuItem>.Error(data.RequestID, $"Menu path: '{data.MenuPath}' returned null result.")
                        .Log(_logger);

                return result;
            }
            catch (Exception ex)
            {
                return ResponseData<ResponseExecuteMenuItem>.Error(data.RequestID, $"Failed to execute menu item: '{data.MenuPath}'. Exception: {ex}")
                    .Log(_logger, ex);
            }
        }

        public new void Dispose()
        {
            base.Dispose();
        }
    }
}