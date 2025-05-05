using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using com.IvanMurzak.Unity.MCP.Server.Utils;

namespace com.IvanMurzak.Unity.MCP.Server.API
{
    public partial class Tool_Menu
    {
        private static readonly ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        [McpServerTool
        (
            Name = "Menu_ListItems",
            Title = "List Unity Menu Items"
        )]
        [Description("List available Unity menu items. Provide a parent path to see child items.")]
        public Task<CallToolResponse> ListItems
        (
            [Description("Parent menu path. Empty for top-level menus.")]
            string parentPath = ""
        )
        {
            ToolLogger.LogToolStart(_logger, "Menu_ListItems", $"parentPath: '{parentPath}'");
            
            return ToolRouter.Call("Menu_ListItems", arguments =>
            {
                arguments[nameof(parentPath)] = parentPath ?? "";
                ToolLogger.LogToolDetail(_logger, "Menu_ListItems", "Arguments prepared for tool router");
            });
        }
    }
} 