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
            Name = "Menu_GetMenuReference",
            Title = "Get Menu Item Reference"
        )]
        [Description("Returns a detailed reference for all menu items under a specific path")]
        public Task<CallToolResponse> GetMenuReference
        (
            [Description("Root menu path to document (e.g., 'Tools/McpTool'). Leave empty for all top-level menus.")]
            string menuPath = ""
        )
        {
            return ToolRouter.Call("Menu_GetMenuReference", arguments =>
            {
                arguments[nameof(menuPath)] = menuPath ?? "";
            });
        }
    }
} 