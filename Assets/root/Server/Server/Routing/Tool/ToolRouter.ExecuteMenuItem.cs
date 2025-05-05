#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Threading;
using System.Threading.Tasks;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using NLog;

namespace com.IvanMurzak.Unity.MCP.Server
{
    public static partial class ToolRouter
    {
        public static async Task<ExecuteMenuItemResponse> ExecuteMenuItem(RequestContext<ExecuteMenuItemRequestParams> request, CancellationToken cancellationToken)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Trace("{0}.ExecuteMenuItem", typeof(ToolRouter).Name);

            if (request == null)
                return new ExecuteMenuItemResponse().SetError("[Error] Request is null");

            if (request.Params == null)
                return new ExecuteMenuItemResponse().SetError("[Error] Request.Params is null");

            if (string.IsNullOrEmpty(request.Params.MenuPath))
                return new ExecuteMenuItemResponse().SetError("[Error] MenuPath is null or empty");

            var mcpServerService = McpServerService.Instance;
            if (mcpServerService == null)
                return new ExecuteMenuItemResponse().SetError($"[Error] '{nameof(mcpServerService)}' is null");

            var toolRunner = mcpServerService.ToolRunner;
            if (toolRunner == null)
                return new ExecuteMenuItemResponse().SetError($"[Error] '{nameof(toolRunner)}' is null");

            var clientConnectionId = RemoteApp.FirstConnectionId;
            if (string.IsNullOrEmpty(clientConnectionId))
            {
                logger.Warn("{0}.ExecuteMenuItem, no connected client. Returning error.", typeof(ToolRouter).Name);
                return new ExecuteMenuItemResponse().SetError("[Error] No client connected");
            }

            var requestData = new RequestExecuteMenuItem(request.Params.MenuPath);
            if (logger.IsTraceEnabled)
                logger.Trace("Execute menu item: '{0}'", request.Params.MenuPath);

            var response = await toolRunner.RunExecuteMenuItem(requestData, connectionId: clientConnectionId, cancellationToken: cancellationToken);
            if (response == null)
                return new ExecuteMenuItemResponse().SetError($"[Error] '{nameof(response)}' is null");

            if (logger.IsTraceEnabled)
                logger.Trace("Execute menu item response: {0}", response.Message);

            if (response.IsError)
                return new ExecuteMenuItemResponse().SetError(response.Message ?? "[Error] Got an error during executing menu item");

            if (response.Value == null)
                return new ExecuteMenuItemResponse().SetError("[Error] Menu execution returned null value");

            return new ExecuteMenuItemResponse { 
                Success = response.Value.Success,
                Message = response.Value.Message
            };
        }
    }

    public class ExecuteMenuItemRequestParams
    {
        public string MenuPath { get; set; } = string.Empty;
    }

    public class ExecuteMenuItemResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Error { get; set; }

        public ExecuteMenuItemResponse SetError(string message)
        {
            Error = true;
            Success = false;
            Message = message;
            return this;
        }
    }
} 