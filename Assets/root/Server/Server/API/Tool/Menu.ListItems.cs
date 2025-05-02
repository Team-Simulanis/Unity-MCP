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
            return ToolRouter.Call("Menu_ListItems", arguments =>
            {
                arguments[nameof(parentPath)] = parentPath ?? "";
            });
        }
    }
} 