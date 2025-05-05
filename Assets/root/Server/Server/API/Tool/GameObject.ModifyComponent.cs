using com.IvanMurzak.Unity.MCP.Common.Data.Unity;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Threading.Tasks;

namespace com.IvanMurzak.Unity.MCP.Server.API
{
    public partial class Tool_GameObject
    {
        [McpServerTool
        (
            Name = "GameObject_ModifyComponent",
            Title = "Modify Component at GameObject in opened Prefab or in a Scene"
        )]
        [Description("Modify existed component at GameObject.")]
        public Task<CallToolResponse> ModifyComponent
        (
            [Description(@"Json Object with required readonly 'instanceID' and 'type' fields.
Each field and property requires to have 'type' and 'name' fields to identify the exact modification target.
Follow the object schema to specify what to change, ignore values that should not be modified.
Any unknown or wrong located fields and properties will be ignored.
Check the result of this command to see what was changed. The ignored fields and properties will not be listed.")]
            SerializedMember componentData,
            GameObjectRef gameObjectRef
        )
        {
            return ToolRouter.Call("GameObject_ModifyComponent", arguments =>
            {
                arguments[nameof(componentData)] = componentData;
                arguments[nameof(gameObjectRef)] = gameObjectRef;
            });
        }
    }
}