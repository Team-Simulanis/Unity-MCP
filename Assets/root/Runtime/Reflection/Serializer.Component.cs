using UnityEngine;
using com.IvanMurzak.Unity.MCP.Common.Data.Unity;
namespace com.IvanMurzak.Unity.MCP.Utils
{
    /// <summary>
    /// Serializes Unity components to JSON format.
    /// </summary>
    public static partial class Serializer
    {
        public static class Component
        {
            public static ComponentData.Enabled BuildIsEnabled(UnityEngine.Component component)
            {
                if (component == null)
                    return ComponentData.Enabled.NA;

                if (component is Behaviour behaviour)
                    return behaviour.enabled
                        ? ComponentData.Enabled.True
                        : ComponentData.Enabled.False;

                if (component is Renderer renderer)
                    return renderer.enabled
                        ? ComponentData.Enabled.True
                        : ComponentData.Enabled.False;

                return ComponentData.Enabled.NA;
            }
        }
    }
}