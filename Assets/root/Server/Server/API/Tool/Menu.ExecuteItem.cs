using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Threading.Tasks;

namespace com.IvanMurzak.Unity.MCP.Server.API
{
    public partial class Tool_Menu
    {
        [McpServerTool
        (
            Name = "Menu_ExecuteItem",
            Title = "Execute Menu Item"
        )]
        [Description("Execute a Unity menu item by path")]
        public Task<CallToolResponse> ExecuteItem
        (
            [Description("Path to the menu item to execute")]
            string menuPath = "Window/AI Connector (Unity-MCP)"
        )
        {
            return ToolRouter.Call("Menu_ExecuteItem", arguments =>
            {
                arguments[nameof(menuPath)] = menuPath ?? "";
            });
        }
    }
} 