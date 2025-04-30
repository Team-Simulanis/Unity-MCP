#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public partial class RS_Primitive : RS_NotArray<object>
    {
        public override int SerializationPriority(Type type)
        {
            var isPrimitive = type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(DateTime);

            return isPrimitive
                ? MAX_DEPTH + 1
                : 0;
        }
        protected override SerializedMember InternalSerialize(object obj, Type type, string name = null, bool recursive = true, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            => SerializedMember.FromValue(type, obj, name: name);

        protected override IEnumerable<FieldInfo> GetSerializableFields(Type objType, BindingFlags flags)
            => throw new NotImplementedException("Primitive types do not support field serialization.");

        protected override IEnumerable<PropertyInfo> GetSerializableProperties(Type objType, BindingFlags flags)
            => throw new NotImplementedException("Primitive types do not support property serialization.");

        protected override bool SetValue(ref object obj, Type type, JsonElement? value)
        {
            var parsedValue = JsonUtils.Deserialize(value.Value.GetRawText(), type);
            obj = parsedValue;
            return true;
        }

        public override bool SetAsField(ref object obj, Type type, FieldInfo fieldInfo, SerializedMember? value,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var parsedValue = JsonUtils.Deserialize(value.valueJsonElement?.GetRawText(), type);
            fieldInfo.SetValue(obj, parsedValue);
            return true;
        }

        public override bool SetAsProperty(ref object obj, Type type, PropertyInfo propertyInfo, SerializedMember? value,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var parsedValue = JsonUtils.Deserialize(value.valueJsonElement?.GetRawText(), type);
            propertyInfo.SetValue(obj, parsedValue);
            return true;
        }

        public override bool SetField(ref object obj, Type type, FieldInfo fieldInfo, SerializedMember? value,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var parsedValue = JsonUtils.Deserialize(value.valueJsonElement?.GetRawText(), type);
            fieldInfo.SetValue(obj, parsedValue);
            return true;
        }

        public override bool SetProperty(ref object obj, Type type, PropertyInfo propertyInfo, SerializedMember? value,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var parsedValue = JsonUtils.Deserialize(value.valueJsonElement?.GetRawText(), type);
            propertyInfo.SetValue(obj, parsedValue);
            return true;
        }
    }
}