#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Reflection;
using System.Text;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public interface IReflectionSerializer
    {
        bool AllowCascadeSerialize { get; }
        bool AllowCascadePopulate { get; }

        int SerializationPriority(Type type);

        SerializedMember Serialize(object obj, Type? type = null, string? name = null, bool recursive = true,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        object? Deserialize(SerializedMember data);
        StringBuilder? Populate(ref object obj, SerializedMember data, int depth = 0, StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        bool SetAsField(ref object obj, Type type, FieldInfo fieldInfo, SerializedMember? value, StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        bool SetAsProperty(ref object obj, Type type, PropertyInfo propertyInfo, SerializedMember? value, StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        bool SetField(ref object obj, Type type, FieldInfo fieldInfo, SerializedMember? value,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        bool SetProperty(ref object obj, Type type, PropertyInfo propertyInfo, SerializedMember? value,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }
}