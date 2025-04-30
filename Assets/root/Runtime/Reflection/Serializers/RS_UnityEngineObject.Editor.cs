#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#if UNITY_EDITOR
using System;
using System.Reflection;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public partial class RS_UnityEngineObject<T> : RS_Generic<T> where T : UnityEngine.Object
    {
        public override bool SetAsField(ref object obj, Type type, FieldInfo fieldInfo, SerializedMember? value,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var instanceID = JsonUtils.Deserialize<InstanceID>(value.valueJsonElement?.GetRawText());
            var refObj = UnityEditor.EditorUtility.InstanceIDToObject(instanceID.instanceID);

            fieldInfo.SetValue(obj, refObj);
            return true;
        }

        public override bool SetAsProperty(ref object obj, Type type, PropertyInfo propertyInfo, SerializedMember? value,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var instanceID = JsonUtils.Deserialize<InstanceID>(value.valueJsonElement?.GetRawText());
            var refObj = UnityEditor.EditorUtility.InstanceIDToObject(instanceID.instanceID);

            propertyInfo.SetValue(obj, refObj);
            return true;
        }
    }
}
#endif