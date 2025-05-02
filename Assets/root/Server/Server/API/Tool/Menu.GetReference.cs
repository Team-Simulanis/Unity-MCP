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
            Name = "Menu_GetReference",
            Title = "Get Menu Reference"
        )]
        [Description("Returns a reference guide for commonly used Unity menu items")]
        public Task<CallToolResponse> GetReference
        (
            [Description("Category to filter by (e.g., 'File', 'Tools', 'Window'). Leave empty for all categories.")]
            string category = ""
        )
        {
            return ToolRouter.Call("Menu_GetReference", arguments =>
            {
                arguments[nameof(category)] = category ?? "";
            });
        }
    }
} 