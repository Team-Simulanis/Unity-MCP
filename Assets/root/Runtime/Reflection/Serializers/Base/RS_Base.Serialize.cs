#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public abstract partial class RS_Base<T> : IReflectionSerializer
    {
        protected virtual IEnumerable<string> ignoredFields => Enumerable.Empty<string>();
        protected virtual IEnumerable<string> ignoredProperties => Enumerable.Empty<string>();

        public virtual SerializedMember Serialize(object obj, Type? type = null, string? name = null, bool recursive = true,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            type ??= obj?.GetType() ?? typeof(T);

            if (obj == null)
                return SerializedMember.FromJson(type, json: null, name: name);

            return InternalSerialize(obj, type, name, recursive, flags);
        }

        protected virtual List<SerializedMember> SerializeFields(object obj, BindingFlags flags)
        {
            var serialized = default(List<SerializedMember>);
            var objType = obj.GetType();

            foreach (var field in GetSerializableFields(objType, flags))
            {
                if (ignoredFields.Contains(field.Name))
                    continue;

                var value = field.GetValue(obj);
                var fieldType = field.FieldType;

                serialized ??= new();
                serialized.Add(Serializer.Serialize(value, fieldType, name: field.Name, recursive: false, flags: flags));
            }
            return serialized;
        }
        protected abstract IEnumerable<FieldInfo> GetSerializableFields(Type objType, BindingFlags flags);

        protected virtual List<SerializedMember> SerializeProperties(object obj, BindingFlags flags)
        {
            var serialized = default(List<SerializedMember>);
            var objType = obj.GetType();

            foreach (var prop in GetSerializableProperties(objType, flags))
            {
                if (ignoredProperties.Contains(prop.Name))
                    continue;
                try
                {
                    var value = prop.GetValue(obj);
                    var propType = prop.PropertyType;

                    serialized ??= new();
                    serialized.Add(Serializer.Serialize(value, propType, name: prop.Name, recursive: false, flags: flags));
                }
                catch { /* skip inaccessible properties */ }
            }
            return serialized;
        }
        protected abstract IEnumerable<PropertyInfo> GetSerializableProperties(Type objType, BindingFlags flags);

        protected abstract SerializedMember InternalSerialize(object obj, Type type, string? name = null, bool recursive = true,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }
}