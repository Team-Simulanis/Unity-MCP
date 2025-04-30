#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using System.Linq;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;
using com.IvanMurzak.Unity.MCP.Common.Utils;
using com.IvanMurzak.Unity.MCP.Utils;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_GameObject
    {
        [McpPluginTool
        (
            "GameObject_ModifyComponent",
            Title = "Modify Component at GameObject in opened Prefab or in a Scene",
            Description = "Modify existed component at GameObject."
        )]
        public string ModifyComponent
        (
            [Description(@"Json Object with required readonly 'instanceID' and 'type' fields.
Each field and property requires to have 'type' and 'name' fields to identify the exact modification target.
Follow the object schema to specify what to change, ignore values that should not be modified.
Any unknown or wrong located fields and properties will be ignored.
Check the result of this command to see what was changed. The ignored fields and properties will not be listed.")]
            SerializedMember componentData,
            [Description("GameObject by 'instanceID' (int). Priority: 1. (Recommended)")]
            int instanceID = 0,
            [Description("GameObject by 'path'. Priority: 2.")]
            string? path = null,
            [Description("GameObject by 'name'. Priority: 3.")]
            string? name = null
        )
        => MainThread.Run(() =>
        {
            if (string.IsNullOrEmpty(componentData?.type))
                return Error.InvalidComponentType(componentData?.type);

            var type = TypeUtils.GetType(componentData.type);
            if (type == null)
                return Error.InvalidComponentType(componentData.type);

            var go = GameObjectUtils.FindBy(instanceID, path, name, out var error);
            if (error != null)
                return error;

            var componentInstanceID = componentData.GetInstanceID();
            if (componentInstanceID == 0 && !string.IsNullOrEmpty(componentData.name) && !componentData.HasIndexName())
                return $"[Error] Component 'instanceID' is not provided. Use 'instanceID' or name '[index]' to specify the component. '{componentData.name}' is not valid.";

            componentData.GetIndexFromName(out var index);

            var allComponents = go.GetComponents<UnityEngine.Component>();
            var component = componentInstanceID == 0
                ? index >= 0 && index < allComponents.Length
                    ? allComponents[index]
                    : null
                : allComponents.FirstOrDefault(c => c.GetInstanceID() == componentInstanceID);

            if (component == null)
                return Error.NotFoundComponent(componentInstanceID, allComponents);

            var componentObject = (object)component;
            var result = Serializer.Populate(ref componentObject, componentData);

            return @$"Component with instanceID '{componentInstanceID}' modification result:

{result.ToString()}

at GameObject.
{go.Print()}";

        });
    }
}