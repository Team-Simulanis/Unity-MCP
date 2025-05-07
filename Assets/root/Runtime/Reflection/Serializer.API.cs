#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Reflection;
using System.Text;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;
using com.IvanMurzak.Unity.MCP.Common.Utils;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    /// <summary>
    /// Serializes Unity components to JSON format.
    /// </summary>
    public static partial class Serializer
    {
        public static SerializedMember Serialize(object obj, Type? type = null, string? name = null, bool recursive = true,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            type ??= obj?.GetType();

            if (obj == null)
                return SerializedMember.FromJson(type, null);

            foreach (var serializer in Registry.BuildSerializersChain(type))
            {
                if (McpPluginUnity.LogLevel.IsActive(LogLevel.Trace))
                    UnityEngine.Debug.Log($"[Serializer] {serializer.GetType().Name} for type {type?.FullName}");

                var serializedMember = serializer.Serialize(obj, type: type, name: name, recursive, flags);
                if (serializedMember != null)
                    return serializedMember;
            }
            throw new ArgumentException($"[Error] Type '{type?.FullName}' not supported for serialization.");
        }
        public static object Deserialize(SerializedMember data)
        {
            if (string.IsNullOrEmpty(data?.type))
                throw new ArgumentException(Error.DataTypeIsEmpty());

            var type = TypeUtils.GetType(data.type);
            if (type == null)
                throw new ArgumentException(Error.NotFoundType(data.type));

            var deserializer = Registry.BuildDeserializersChain(type);
            if (deserializer == null)
                throw new ArgumentException($"[Error] Type '{type?.FullName}' not supported for deserialization.");
        
            if (McpPluginUnity.LogLevel.IsActive(LogLevel.Trace))
                UnityEngine.Debug.Log($"[Serializer] {deserializer.GetType().Name} for type {type?.FullName}");

            var obj = deserializer.Deserialize(data);
            return obj;
        }
        public static StringBuilder Populate(ref object obj, SerializedMember data, StringBuilder stringBuilder = null, int depth = 0,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            stringBuilder ??= new StringBuilder();

            if (string.IsNullOrEmpty(data?.type))
                return stringBuilder.AppendLine(new string(' ', depth) + Error.DataTypeIsEmpty());

            var type = TypeUtils.GetType(data.type);
            if (type == null)
                return stringBuilder.AppendLine(new string(' ', depth) + Error.NotFoundType(data.type));

            if (obj == null)
                return stringBuilder.AppendLine(new string(' ', depth) + Error.TargetObjectIsNull());

            TypeUtils.CastTo(obj, data.type, out var error);
            if (error != null)
                return stringBuilder.AppendLine(new string(' ', depth) + error);

            if (!type.IsAssignableFrom(obj.GetType()))
                return stringBuilder.AppendLine(new string(' ', depth) + Error.TypeMismatch(data.type, obj.GetType().FullName));

            foreach (var populators in Registry.BuildPopulatorsChain(type))
                populators.Populate(ref obj, data, stringBuilder: stringBuilder, flags: flags);

            return stringBuilder;
        }

        public static StringBuilder PopulateAsProperty(ref object obj, PropertyInfo propertyInfo, SerializedMember data, StringBuilder stringBuilder = null, int depth = 0,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            stringBuilder ??= new StringBuilder();

            if (string.IsNullOrEmpty(data?.type))
                return stringBuilder.AppendLine(new string(' ', depth) + Error.DataTypeIsEmpty());

            var type = TypeUtils.GetType(data.type);
            if (type == null)
                return stringBuilder.AppendLine(new string(' ', depth) + Error.NotFoundType(data.type));

            if (obj == null)
                return stringBuilder.AppendLine(new string(' ', depth) + Error.TargetObjectIsNull());

            TypeUtils.CastTo(obj, data.type, out var error);
            if (error != null)
                return stringBuilder.AppendLine(new string(' ', depth) + error);

            if (!type.IsAssignableFrom(obj.GetType()))
                return stringBuilder.AppendLine(new string(' ', depth) + Error.TypeMismatch(data.type, obj.GetType().FullName));

            foreach (var populators in Registry.BuildPopulatorsChain(type))
                populators.Populate(ref obj, data, stringBuilder: stringBuilder, flags: flags);

            return stringBuilder;
        }
    }
}
