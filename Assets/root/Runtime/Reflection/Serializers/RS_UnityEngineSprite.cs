#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Reflection;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public partial class RS_UnityEngineSprite : RS_UnityEngineObject<UnityEngine.Sprite>
    {
        protected override SerializedMember InternalSerialize(object obj, Type type, string name = null, bool recursive = true, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            if (obj is UnityEngine.Texture2D texture)
            {
                var instanceID = new InstanceID(texture.GetInstanceID());
                return SerializedMember.FromValue(type, instanceID, name);
            }

            return base.InternalSerialize(obj, type, name, recursive, flags);
        }
        public override bool SetAsField(ref object obj, Type type, FieldInfo fieldInfo, SerializedMember? value, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var currentValue = fieldInfo.GetValue(obj);
            Populate(ref currentValue, value, 0, null, flags);
            fieldInfo.SetValue(obj, currentValue);
            return true;
        }
        public override bool SetAsProperty(ref object obj, Type type, PropertyInfo propertyInfo, SerializedMember? value, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var currentValue = propertyInfo.GetValue(obj);
            Populate(ref currentValue, value, 0, null, flags);
            propertyInfo.SetValue(obj, currentValue);
            return true;
        }
    }
}