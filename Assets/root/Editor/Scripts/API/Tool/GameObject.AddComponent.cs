#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Utils;
using com.IvanMurzak.Unity.MCP.Utils;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_GameObject
    {
        [McpPluginTool
        (
            "GameObject_AddComponent",
            Title = "Add Component to a GameObject in opened Prefab or in a Scene",
            Description = "Add a component to a GameObject."
        )]
        public string AddComponent
        (
            [Description("Full name of the Component. It should include full namespace path and the class name.")]
            string componentName,
            [Description("GameObject by 'instanceID' (int). Priority: 1. (Recommended)")]
            int instanceID = 0,
            [Description("GameObject by 'path'. Priority: 2.")]
            string? path = null,
            [Description("GameObject by 'name'. Priority: 3.")]
            string? name = null
        )
        => MainThread.Run(() =>
        {
            var go = GameObjectUtils.FindBy(instanceID, path, name, out var error);
            if (error != null)
                return error;

            var type = TypeUtils.GetType(componentName);
            if (type == null)
                return Tool_Component.Error.NotFoundComponentType(componentName);

            // Check if type is a subclass of UnityEngine.Component
            if (!typeof(UnityEngine.Component).IsAssignableFrom(type))
                return Tool_Component.Error.TypeMustBeComponent(componentName);

            go.AddComponent(type);

            return $"[Success] Added component '{componentName}' to GameObject.\n{go.Print()}";
        });
    }
}