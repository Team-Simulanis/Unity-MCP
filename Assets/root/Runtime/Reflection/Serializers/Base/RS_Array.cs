#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;
using com.IvanMurzak.Unity.MCP.Common.Utils;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public partial class RS_Array : RS_Base<Array>
    {
        public override int SerializationPriority(Type type)
        {
            if (type.IsArray)
                return MAX_DEPTH + 1;

            var isGenericList = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
            if (isGenericList)
                return MAX_DEPTH + 1;

            var isArray = typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string);
            return isArray
                ? MAX_DEPTH / 4
                : 0;
        }

        protected override SerializedMember InternalSerialize(object obj, Type type, string name = null, bool recursive = true, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            int index = 0;
            var serializedList = new List<SerializedMember>();
            var enumerable = (System.Collections.IEnumerable)obj;

            foreach (var element in enumerable)
                serializedList.Add(Serializer.Serialize(element, type: element?.GetType(), name: $"[{index++}]", recursive: recursive, flags: flags));

            return SerializedMember.FromValue(type, serializedList, name: name);
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
            var parsedList = JsonUtils.Deserialize<List<SerializedMember>>(value.Value.GetRawText());
            var enumerable = parsedList
                .Select(element =>
                {
                    var elementType = TypeUtils.GetType(element.type);
                    var elementValue = JsonUtils.Deserialize(element.valueJsonElement.Value.GetRawText(), elementType);
                    return elementValue;
                });

            obj = type.IsArray
                ? enumerable.ToArray()
                : enumerable.ToList();
            return true;
        }

        public override bool SetAsField(ref object obj, Type type, FieldInfo fieldInfo, SerializedMember? value,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var parsedList = JsonUtils.Deserialize<List<SerializedMember>>(value.valueJsonElement?.GetRawText());
            var enumerable = parsedList
                .Select(element =>
                {
                    var elementType = TypeUtils.GetType(element.type);
                    var elementValue = JsonUtils.Deserialize(element.valueJsonElement.Value.GetRawText(), elementType);
                    return elementValue;
                });

            fieldInfo.SetValue(obj, type.IsArray
                ? enumerable.ToArray()
                : enumerable.ToList());
            return true;
        }

        public override bool SetAsProperty(ref object obj, Type type, PropertyInfo propertyInfo, SerializedMember? value,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var parsedList = JsonUtils.Deserialize<List<SerializedMember>>(value.valueJsonElement?.GetRawText());
            var enumerable = parsedList
                .Select(element =>
                {
                    var elementType = TypeUtils.GetType(element.type);
                    var elementValue = JsonUtils.Deserialize(element.valueJsonElement.Value.GetRawText(), elementType);
                    return elementValue;
                });

            propertyInfo.SetValue(obj, type.IsArray
                ? enumerable.ToArray()
                : enumerable.ToList());
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