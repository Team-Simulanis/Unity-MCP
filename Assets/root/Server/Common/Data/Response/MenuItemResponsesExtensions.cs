#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using Microsoft.Extensions.Logging;

namespace com.IvanMurzak.Unity.MCP.Common.Data
{
    public static class MenuItemResponsesExtensions
    {
        public static ResponseExecuteMenuItem Log(this ResponseExecuteMenuItem target, ILogger logger, Exception? ex = null)
        {
            if (!target.Success)
                logger.LogError(ex, "Menu execution failed: {0} - {1}", target.MenuPath, target.Message);
            else
                logger.LogInformation("Menu executed successfully: {0}", target.MenuPath);
            
            return target;
        }

        public static IResponseData<ResponseExecuteMenuItem> Pack(this ResponseExecuteMenuItem target, string requestId, string? message = null)
        {
            if (!target.Success)
                return ResponseData<ResponseExecuteMenuItem>.Error(requestId, message ?? $"Menu item execution failed: {target.Message}")
                    .SetData(target);
            else
                return ResponseData<ResponseExecuteMenuItem>.Success(requestId, message ?? "Menu item executed successfully")
                    .SetData(target);
        }
    }
} 