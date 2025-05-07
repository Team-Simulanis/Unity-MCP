#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Text;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public partial class RS_UnityEngineObject<T> : RS_Generic<T> where T : UnityEngine.Object
    {
        public override bool SetAsField(ref object obj, Type type, FieldInfo fieldInfo, SerializedMember? value, StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var refObj = value.valueJsonElement.ToObjectRef().FindObject();
            stringBuilder?.AppendLine($"[Success] Field '{value.name}' modified to '{refObj?.GetInstanceID()}'.");

            fieldInfo.SetValue(obj, refObj);
            return true;
        }

        public override bool SetAsProperty(ref object obj, Type type, PropertyInfo propertyInfo, SerializedMember? value, StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var refObj = value.valueJsonElement.ToObjectRef().FindObject();
            stringBuilder?.AppendLine($"[Success] Property '{value.name}' modified to '{refObj?.GetInstanceID()}'.");

            propertyInfo.SetValue(obj, refObj);
            return true;
        }
    }
}
#endif