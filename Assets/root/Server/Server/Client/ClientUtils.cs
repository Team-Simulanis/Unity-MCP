using System;
using System.Threading;
using System.Threading.Tasks;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data;
using com.IvanMurzak.Unity.MCP.Common.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace com.IvanMurzak.Unity.MCP.Server
{
    public static class ClientUtils
    {
        const int maxRetries = 10; // Maximum number of retries
        const int retryDelayMs = 1000; // Delay between retries in milliseconds

        public static async Task<IResponseData<TResponse>> InvokeAsync<TRequest, TResponse, THub>(
            ILogger logger,
            IHubContext<THub> hubContext,
            string methodName,
            string? connectionId,
            TRequest requestData,
            CancellationToken cancellationToken = default)
            where TRequest : IRequestID
            where THub : Hub
        {
            if (string.IsNullOrEmpty(connectionId))
                return ResponseData<TResponse>.Error(requestData.RequestID, $"'{nameof(connectionId)}' is null. Can't Invoke method '{methodName}'.")
                    .Log(logger);

            if (hubContext == null)
                return ResponseData<TResponse>.Error(requestData.RequestID, $"'{nameof(hubContext)}' is null.").Log(logger);

            if (string.IsNullOrEmpty(methodName))
                return ResponseData<TResponse>.Error(requestData.RequestID, $"'{nameof(methodName)}' is null.").Log(logger);

            try
            {
                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("ConnectionId: {0}. Invoke '{1}':\n{2}", connectionId, methodName, requestData);
                }

                // if (logger.IsEnabled(LogLevel.Information))
                // {
                //     var message = requestData.Arguments == null
                //         ? $"Run tool '{requestData.Name}' with no parameters."
                //         : $"Run tool '{requestData.Name}' with parameters[{requestData.Arguments.Count}]:\n{string.Join(",\n", requestData.Arguments)}";
                //     logger.LogInformation(message);
                // }

                var retryCount = 0;
                while (retryCount < maxRetries)
                {
                    retryCount++;
                    var client = hubContext.Clients.Client(connectionId);
                    if (client == null)
                    {
                        logger.LogWarning($"No connected clients. Retrying [{retryCount}/{maxRetries}]...");
                        await Task.Delay(retryDelayMs, cancellationToken); // Wait before retrying
                        continue;
                    }

                    var invokeTask = client.InvokeAsync<ResponseData<TResponse>>(methodName, requestData, cancellationToken);
                    var completedTask = await Task.WhenAny(invokeTask, Task.Delay(TimeSpan.FromSeconds(com.IvanMurzak.Unity.MCP.Common.Utils.Consts.Hub.TimeoutSeconds), cancellationToken));
                    if (completedTask == invokeTask)
                    {
                        try
                        {
                            var result = await invokeTask;
                            if (result == null)
                                return ResponseData<TResponse>.Error(requestData.RequestID, $"Invoke '{requestData}' returned null result.")
                                    .Log(logger);

                            return result;
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Error invoking '{requestData}' on client '{connectionId}': {ex.Message}");
                            // RemoveCurrentClient(client);
                            await Task.Delay(retryDelayMs, cancellationToken); // Wait before retrying
                            continue;
                        }
                    }

                    // Timeout occurred
                    logger.LogWarning($"Timeout: Client '{connectionId}' did not respond in {com.IvanMurzak.Unity.MCP.Common.Utils.Consts.Hub.TimeoutSeconds} seconds. Removing from ConnectedClients.");
                    // RemoveCurrentClient(client);
                    await Task.Delay(retryDelayMs, cancellationToken); // Wait before retrying
                    // Restart the loop to try again with a new client
                }
                return ResponseData<TResponse>.Error(requestData.RequestID, $"Failed to invoke '{requestData}' after {retryCount} retries.")
                    .Log(logger);
            }
            catch (Exception ex)
            {
                return ResponseData<TResponse>.Error(requestData.RequestID, $"Failed to invoke '{requestData}'. Exception: {ex}")
                    .Log(logger, ex);
            }
        }
    }
}