#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using com.IvanMurzak.Unity.MCP.Common.Data.Unity;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public static class ExtensionsComponentRef
    {
        public static bool Matches(this ComponentRef componentRef, UnityEngine.Component component, int? index = null)
        {
            if (componentRef.instanceID != 0)
            {
                return componentRef.instanceID == component.GetInstanceID();
            }
            if (componentRef.index >= 0 && index != null)
            {
                return componentRef.index == index.Value;
            }
            if (!string.IsNullOrEmpty(componentRef.type))
            {
                return component.GetType().FullName == componentRef.type;
            }
            return false;
        }
    }
}