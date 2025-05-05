#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Reflection;
using System.Text;
using System.Text.Json;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;
using com.IvanMurzak.Unity.MCP.Common.Utils;
using static com.IvanMurzak.Unity.MCP.Utils.Serializer;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public abstract partial class RS_Base<T> : IReflectionSerializer
    {
        public virtual StringBuilder? Populate(ref object obj, SerializedMember data, int depth = 0, StringBuilder? stringBuilder = null, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            if (string.IsNullOrEmpty(data.type))
                return stringBuilder?.AppendLine("[Error] Type is null or empty.");

            var type = TypeUtils.GetType(data.type);
            if (type == null)
                return stringBuilder?.AppendLine($"[Error] Type not found: {data.type}");

            TypeUtils.CastTo(obj, data.type, out var error);
            if (error != null)
                return stringBuilder?.AppendLine(error);

            if (!type.IsAssignableFrom(obj.GetType()))
                return stringBuilder?.AppendLine($"[Error] Type mismatch: {data.type} vs {obj.GetType().FullName}");

            if (data.valueJsonElement != null)
            {
                try
                {
                    SetValue(ref obj, type, data.valueJsonElement);
                    stringBuilder?.AppendLine(new string(' ', depth) + $"[Success] Object '{obj}' modified to '{data.valueJsonElement}'.");
                }
                catch (Exception ex)
                {
                    stringBuilder?.AppendLine(new string(' ', depth) + $"[Error] Object '{obj}' modification failed: {ex.Message}");
                }
            }

            var nextDepth = depth + 1;

            if (data.fields != null)
                foreach (var field in data.fields)
                    ModifyField(ref obj, field, stringBuilder, depth: nextDepth, flags: flags);

            if (data.properties != null)
                foreach (var property in data.properties)
                    ModifyProperty(ref obj, property, stringBuilder, depth: nextDepth, flags: flags);

            return stringBuilder;
        }
        protected abstract bool SetValue(ref object obj, Type type, JsonElement? value);

        protected virtual StringBuilder? ModifyField(ref object obj, SerializedMember fieldValue, StringBuilder? stringBuilder = null, int depth = 0,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            if (string.IsNullOrEmpty(fieldValue.name))
                return stringBuilder?.AppendLine(new string(' ', depth) + Error.ComponentFieldNameIsEmpty());

            if (string.IsNullOrEmpty(fieldValue.type))
                return stringBuilder?.AppendLine(new string(' ', depth) + Error.ComponentFieldTypeIsEmpty());

            var fieldInfo = obj.GetType().GetField(fieldValue.name, flags);
            if (fieldInfo == null)
                return stringBuilder?.AppendLine(new string(' ', depth) + $"[Error] Field '{fieldValue.name}'. Make sure the name is right, it is case sensitive. Make sure this is a field, maybe is it a property?.");

            var targetType = TypeUtils.GetType(fieldValue.type);
            if (targetType == null)
                return stringBuilder?.AppendLine(new string(' ', depth) + Error.InvalidComponentFieldType(fieldValue, fieldInfo));

            try
            {
                foreach (var populators in Registry.BuildPopulatorsChain(targetType))
                    populators.SetAsField(ref obj, targetType, fieldInfo, value: fieldValue, flags: flags);

                return stringBuilder?.AppendLine(new string(' ', depth) + $"[Success] Field '{fieldValue.name}' modified to '{fieldValue.valueJsonElement}'.");
            }
            catch (Exception ex)
            {
                var message = $"[Error] Field '{fieldValue.name}' modification failed: {ex.Message}";
                return stringBuilder?.AppendLine(new string(' ', depth) + message);
            }
        }

        protected virtual StringBuilder? ModifyProperty(ref object obj, SerializedMember propertyValue, StringBuilder? stringBuilder = null, int depth = 0,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            if (string.IsNullOrEmpty(propertyValue.name))
                return stringBuilder?.AppendLine(new string(' ', depth) + Error.ComponentPropertyNameIsEmpty());

            if (string.IsNullOrEmpty(propertyValue.type))
                return stringBuilder?.AppendLine(new string(' ', depth) + Error.ComponentPropertyTypeIsEmpty());

            var propInfo = obj.GetType().GetProperty(propertyValue.name, flags);
            if (propInfo == null)
            {
                var warningMessage = $"[Error] Property '{propertyValue.name}' not found. Make sure the name is right, it is case sensitive. Make sure this is a property, maybe is it a field?.";
                return stringBuilder?.AppendLine(new string(' ', depth) + warningMessage);
            }
            if (!propInfo.CanWrite)
            {
                var warningMessage = $"[Error] Property '{propertyValue.name}' is not writable. Can't modify property '{propertyValue.name}'.";
                return stringBuilder?.AppendLine(new string(' ', depth) + warningMessage);
            }

            var targetType = TypeUtils.GetType(propertyValue.type);
            if (targetType == null)
                return stringBuilder?.AppendLine(new string(' ', depth) + Error.InvalidComponentPropertyType(propertyValue, propInfo));

            try
            {
                foreach (var populators in Registry.BuildPopulatorsChain(targetType))
                    populators.SetAsProperty(ref obj, targetType, propInfo, value: propertyValue, flags: flags);

                return stringBuilder?.AppendLine(new string(' ', depth) + $"[Success] Property '{propertyValue.name}' modified to '{propertyValue.valueJsonElement}'.");
            }
            catch (Exception ex)
            {
                var message = $"[Error] Property '{propertyValue.name}' modification failed: {ex.Message}";
                return stringBuilder?.AppendLine(new string(' ', depth) + message);
            }
        }

        public abstract bool SetAsField(ref object obj, Type type, FieldInfo fieldInfo, SerializedMember? value,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public abstract bool SetAsProperty(ref object obj, Type type, PropertyInfo propertyInfo, SerializedMember? value,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public abstract bool SetField(ref object obj, Type type, FieldInfo fieldInfo, SerializedMember? value,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public abstract bool SetProperty(ref object obj, Type type, PropertyInfo propertyInfo, SerializedMember? value,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }
}