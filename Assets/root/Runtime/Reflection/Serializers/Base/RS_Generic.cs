#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public partial class RS_Generic<T> : RS_NotArray<T>
    {
        protected override SerializedMember InternalSerialize(object obj, Type type, string name = null, bool recursive = true, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var isStruct = type.IsValueType && !type.IsPrimitive && !type.IsEnum;
            if (type.IsClass || isStruct)
            {
                return recursive
                    ? new SerializedMember()
                    {
                        name = name,
                        type = type.FullName,
                        fields = SerializeFields(obj, flags),
                        properties = SerializeProperties(obj, flags)
                    }
                    : SerializedMember.FromJson(type, JsonUtils.Serialize(obj), name: name);
            }
            throw new ArgumentException($"Unsupported type: {type.FullName}");
        }
        protected override IEnumerable<FieldInfo> GetSerializableFields(Type objType, BindingFlags flags)
            => objType.GetFields(flags)
                .Where(field => field.GetCustomAttribute<ObsoleteAttribute>() == null)
                .Where(field => field.IsPublic || field.IsPrivate && field.GetCustomAttribute<SerializeField>() != null);

        protected override IEnumerable<PropertyInfo> GetSerializableProperties(Type objType, BindingFlags flags)
            => objType.GetProperties(flags)
                .Where(prop => prop.GetCustomAttribute<ObsoleteAttribute>() == null)
                .Where(prop => prop.CanRead);

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