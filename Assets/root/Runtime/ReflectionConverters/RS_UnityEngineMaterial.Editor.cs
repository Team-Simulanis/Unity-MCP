#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Text;
using com.IvanMurzak.Unity.MCP.Common.Data.Unity;
using com.IvanMurzak.Unity.MCP.Common.Reflection;
using com.IvanMurzak.Unity.MCP.Common.Reflection.Convertor;
using com.IvanMurzak.Unity.MCP.Common.Utils;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Reflection.Convertor
{
    public partial class RS_UnityEngineMaterial : RS_GenericUnity<Material>
    {

        protected override StringBuilder? ModifyProperty(Reflector reflector, ref object obj, SerializedMember property, StringBuilder? stringBuilder = null, int depth = 0,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            var material = obj as Material;
            var propType = TypeUtils.GetType(property.type);
            if (propType == null)
                return stringBuilder.AppendLine(new string(' ', depth) + $"[Error] Property type '{property.type}' not found.");

            switch (propType)
            {
                case Type t when t == typeof(int):
                    if (material.HasInt(property.name))
                    {
                        material.SetInt(property.name, property.GetValue<int>());
                        return stringBuilder.AppendLine(new string(' ', depth) + $"[Success] Property '{property.name}' modified to '{property.GetValue<int>()}'.");
                    }
                    return stringBuilder.AppendLine(new string(' ', depth) + $"[Error] Property '{property.name}' not found.");
                case Type t when t == typeof(float):
                    if (material.HasFloat(property.name))
                    {
                        material.SetFloat(property.name, property.GetValue<float>());
                        return stringBuilder.AppendLine(new string(' ', depth) + $"[Success] Property '{property.name}' modified to '{property.GetValue<float>()}'.");
                    }
                    return stringBuilder.AppendLine(new string(' ', depth) + $"[Error] Property '{property.name}' not found.");
                case Type t when t == typeof(Color):
                    if (material.HasColor(property.name))
                    {
                        material.SetColor(property.name, property.GetValue<Color>());
                        return stringBuilder.AppendLine(new string(' ', depth) + $"[Success] Property '{property.name}' modified to '{property.GetValue<Color>()}'.");
                    }
                    return stringBuilder.AppendLine(new string(' ', depth) + $"[Error] Property '{property.name}' not found.");
                case Type t when t == typeof(Vector4):
                    if (material.HasVector(property.name))
                    {
                        material.SetVector(property.name, property.GetValue<Vector4>());
                        return stringBuilder.AppendLine(new string(' ', depth) + $"[Success] Property '{property.name}' modified to '{property.GetValue<Vector4>()}'.");
                    }
                    return stringBuilder.AppendLine(new string(' ', depth) + $"[Error] Property '{property.name}' not found.");
                case Type t when t == typeof(Texture):
                    if (material.HasTexture(property.name))
                    {
                        var objTexture = property.GetValue<ObjectRef>().FindObject();
                        var texture = objTexture as Texture;
                        material.SetTexture(property.name, texture);
                        return stringBuilder.AppendLine(new string(' ', depth) + $"[Success] Property '{property.name}' modified to '{texture?.name ?? "null"}'.");
                    }
                    return stringBuilder.AppendLine(new string(' ', depth) + $"[Error] Property '{property.name}' not found.");
                default:
                    return stringBuilder.AppendLine(new string(' ', depth) + $"[Error] Property type '{property.type}' is not supported.");
            }
        }

        public override bool SetAsField(Reflector reflector, ref object obj, Type type, FieldInfo fieldInfo, SerializedMember? value, StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var refObj = value.valueJsonElement.ToObjectRef().FindObject();
            stringBuilder?.AppendLine($"[Success] Field '{value.name}' modified to '{refObj?.GetInstanceID()}'.");

            fieldInfo.SetValue(obj, refObj);
            return true;
        }

        public override bool SetAsProperty(Reflector reflector, ref object obj, Type type, PropertyInfo propertyInfo, SerializedMember? value, StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var refObj = value.valueJsonElement.ToObjectRef().FindObject();
            stringBuilder?.AppendLine($"[Success] Property '{value.name}' modified to '{refObj?.GetInstanceID()}'.");

            propertyInfo.SetValue(obj, refObj);
            return true;
        }
    }
}
#endif