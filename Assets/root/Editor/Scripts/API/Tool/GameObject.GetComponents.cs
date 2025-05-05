#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using System.Linq;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data.Unity;
using com.IvanMurzak.Unity.MCP.Utils;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_GameObject
    {
        [McpPluginTool
        (
            "GameObject_GetComponents",
            Title = "Get GameObject components in opened Prefab or in a Scene"
        )]
        [Description(@"Get components of the target GameObject. Returns property values of each component.
Returns list of all available components preview if no requested components found.")]
        public string GetComponents
        (
            GameObjectRef gameObjectRef,
            ComponentRefList? filterComponentRefs = null
        )
        => MainThread.Run(() =>
        {
            var go = GameObjectUtils.FindBy(gameObjectRef, out var error);
            if (error != null)
                return error;

            var allComponents = go.GetComponents<UnityEngine.Component>();

            var needToFilterComponents = (filterComponentRefs?.Count ?? 0) > 0;

            var tempComponents = needToFilterComponents
                ? allComponents.Where(c => filterComponentRefs.Any(fcr => fcr.Matches(c)))
                : allComponents;

            var components = tempComponents
                .Select((c, i) => Serializer.Serialize(c, name: $"[{i}]"))
                .ToList();

            if (components.Count == 0)
                return Error.NotFoundComponents(filterComponentRefs, allComponents);

            var componentsJson = JsonUtils.Serialize(components);

            return @$"[Success] Found {components.Count} components in GameObject with 'instanceID'={go.GetInstanceID()}.
{go.Print()}

# Components:
{componentsJson}";
        });
    }
}