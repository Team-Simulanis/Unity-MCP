#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using com.IvanMurzak.Unity.MCP.Common.Data.Unity;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public static class ExtensionsRuntimeObjectRef
    {
        public static UnityEngine.Object? FindObject(this ObjectRef? objectRef)
        {
            if (objectRef == null)
                return null;

#if UNITY_EDITOR
            if (objectRef.instanceID != 0)
                return UnityEditor.EditorUtility.InstanceIDToObject(objectRef.instanceID);

            if (!string.IsNullOrEmpty(objectRef.assetPath))
                return UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(objectRef.assetPath);
#endif

            return null;
        }
        public static ObjectRef? ToObjectRef(this UnityEngine.Object? obj)
        {
            if (obj == null)
                return new ObjectRef();

            return new ObjectRef(obj.GetInstanceID());
        }
    }
}