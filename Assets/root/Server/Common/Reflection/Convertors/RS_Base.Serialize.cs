#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;

namespace com.IvanMurzak.Unity.MCP.Common.Reflection.Convertor
{
    public abstract partial class RS_Base<T> : IReflectionConvertor
    {
        protected virtual IEnumerable<string> ignoredFields => Enumerable.Empty<string>();
        protected virtual IEnumerable<string> ignoredProperties => Enumerable.Empty<string>();

        public virtual SerializedMember Serialize(Reflector reflector, object? obj, Type? type = null, string? name = null, bool recursive = true,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            type ??= obj?.GetType() ?? typeof(T);

            if (obj == null)
                return SerializedMember.FromJson(type, json: null, name: name);

            return InternalSerialize(reflector, obj, type, name, recursive, flags);
        }

        protected virtual List<SerializedMember>? SerializeFields(Reflector reflector, object obj, BindingFlags flags)
        {
            var serialized = default(List<SerializedMember>);
            var objType = obj.GetType();

            var fields = GetSerializableFields(reflector, objType, flags);
            if (fields == null)
                return null;

            foreach (var field in fields)
            {
                if (ignoredFields.Contains(field.Name))
                    continue;

                var value = field.GetValue(obj);
                var fieldType = field.FieldType;

                serialized ??= new();
                serialized.Add(reflector.Serialize(value, fieldType, name: field.Name, recursive: false, flags: flags));
            }
            return serialized;
        }
        public abstract IEnumerable<FieldInfo>? GetSerializableFields(Reflector reflector, Type objType, BindingFlags flags);

        protected virtual List<SerializedMember>? SerializeProperties(Reflector reflector, object obj, BindingFlags flags)
        {
            var serialized = default(List<SerializedMember>);
            var objType = obj.GetType();

            var properties = GetSerializableProperties(reflector, objType, flags);
            if (properties == null)
                return null;

            foreach (var prop in properties)
            {
                if (ignoredProperties.Contains(prop.Name))
                    continue;
                try
                {
                    var value = prop.GetValue(obj);
                    var propType = prop.PropertyType;

                    serialized ??= new();
                    serialized.Add(reflector.Serialize(value, propType, name: prop.Name, recursive: false, flags: flags));
                }
                catch { /* skip inaccessible properties */ }
            }
            return serialized;
        }
        public abstract IEnumerable<PropertyInfo>? GetSerializableProperties(Reflector reflector, Type objType, BindingFlags flags);

        protected abstract SerializedMember InternalSerialize(Reflector reflector, object obj, Type type, string? name = null, bool recursive = true,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }
}