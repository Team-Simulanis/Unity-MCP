using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Threading.Tasks;

namespace com.IvanMurzak.Unity.MCP.Server.API
{
    public partial class Tool_Scene
    {
        [McpServerTool
        (
            Name = "Reflection_MethodCall",
            Title = "Find method using reflection"
        )]
        [Description("Find method in the project using C# Reflection.")]
        public Task<CallToolResponse> MethodCall
        (
            [Description("Path to the scene file.")]
            string path
        )
        {
            return ToolRouter.Call("Reflection_MethodCall", arguments =>
            {
                arguments[nameof(path)] = path;
            });
        }
    }
}