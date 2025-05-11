#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;
using com.IvanMurzak.Unity.MCP.Common.Utils;
using Microsoft.Extensions.Logging;

namespace com.IvanMurzak.Unity.MCP.Common.Reflection
{
    /// <summary>
    /// Serializes Unity components to JSON format.
    /// </summary>
    public partial class Reflector
    {
        public static Reflector? Instance { get; private set; } = null;
        public Registry Convertors { get; }

        public Reflector()
        {
            Instance ??= this;
            Convertors = new Registry();
        }

        public SerializedMember Serialize(object? obj, Type? type = null, string? name = null, bool recursive = true,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, ILogger? logger = null)
        {
            type ??= obj?.GetType();

            if (type == null)
                throw new ArgumentException($"No type provided for serialization.");

            if (obj == null)
                return SerializedMember.FromJson(type, json: null, name: name);

            foreach (var serializer in Convertors.BuildSerializersChain(type))
            {
                logger?.LogTrace("[Serializer] {0} for type {1}", serializer.GetType().Name, type?.FullName);

                var serializedMember = serializer.Serialize(this, obj, type: type, name: name, recursive, flags);
                if (serializedMember != null)
                    return serializedMember;
            }
            throw new ArgumentException($"[Error] Type '{type?.FullName}' not supported for serialization.");
        }
        public object? Deserialize(SerializedMember data, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(data?.type))
                throw new ArgumentException(Error.DataTypeIsEmpty());

            var type = TypeUtils.GetType(data.type);
            if (type == null)
                throw new ArgumentException(Error.NotFoundType(data.type));

            var deserializer = Convertors.BuildDeserializersChain(type);
            if (deserializer == null)
                throw new ArgumentException($"[Error] Type '{type?.FullName}' not supported for deserialization.");

            logger?.LogTrace($"[Serializer] {deserializer.GetType().Name} for type {type?.FullName}");

            var obj = deserializer.Deserialize(this, data);
            return obj;
        }
        public IEnumerable<FieldInfo>? GetSerializableFields(Type type,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            => Convertors.BuildDeserializersChain(type)?.GetSerializableFields(this, type, flags);
        public IEnumerable<PropertyInfo>? GetSerializableProperties(Type type,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            => Convertors.BuildDeserializersChain(type)?.GetSerializableProperties(this, type, flags);
        public StringBuilder Populate(ref object obj, SerializedMember data, StringBuilder? stringBuilder = null, int depth = 0,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, ILogger? logger = null)
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
                return stringBuilder.AppendLine(new string(' ', depth) + Error.TypeMismatch(data.type, obj.GetType().FullName ?? string.Empty));

            foreach (var convertor in Convertors.BuildPopulatorsChain(type))
                convertor.Populate(this, ref obj, data, stringBuilder: stringBuilder, flags: flags);

            return stringBuilder;
        }

        public StringBuilder PopulateAsProperty(ref object obj, PropertyInfo propertyInfo, SerializedMember data, StringBuilder? stringBuilder = null, int depth = 0,
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
                return stringBuilder.AppendLine(new string(' ', depth) + Error.TypeMismatch(data.type, obj.GetType().FullName ?? string.Empty));

            foreach (var convertor in Convertors.BuildPopulatorsChain(type))
                convertor.Populate(this, ref obj, data, stringBuilder: stringBuilder, flags: flags);

            return stringBuilder;
        }
    }
}
