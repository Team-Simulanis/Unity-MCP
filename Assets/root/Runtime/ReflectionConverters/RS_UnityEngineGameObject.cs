#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using com.IvanMurzak.Unity.MCP.Common.Data.Unity;
using com.IvanMurzak.Unity.MCP.Common.Reflection;
using com.IvanMurzak.Unity.MCP.Common.Reflection.Convertor;
using com.IvanMurzak.Unity.MCP.Common.Utils;
using com.IvanMurzak.Unity.MCP.Utils;

namespace com.IvanMurzak.Unity.MCP.Reflection.Convertor
{
    public partial class RS_UnityEngineGameObject : RS_GenericUnity<UnityEngine.GameObject>
    {
        protected override IEnumerable<string> ignoredProperties => base.ignoredProperties
            .Concat(new[]
            {
                nameof(UnityEngine.GameObject.gameObject),
                nameof(UnityEngine.GameObject.transform),
                nameof(UnityEngine.GameObject.scene)
            });
        protected override SerializedMember InternalSerialize(Reflector reflector, object obj, Type type, string name = null, bool recursive = true, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var unityObject = obj as UnityEngine.GameObject;
            if (recursive)
            {
                return new SerializedMember()
                {
                    name = name,
                    type = type.FullName,
                    fields = SerializeFields(reflector, obj, flags),
                    properties = SerializeProperties(reflector, obj, flags)
                }.SetValue(new ObjectRef(unityObject.GetInstanceID()));
            }
            else
            {
                var objectRef = new ObjectRef(unityObject.GetInstanceID());
                return SerializedMember.FromValue(type, objectRef, name);
            }
        }

        protected override List<SerializedMember> SerializeFields(Reflector reflector, object obj, BindingFlags flags)
        {
            var serializedFields = base.SerializeFields(reflector, obj, flags) ?? new();

            var go = obj as UnityEngine.GameObject;
            var components = go.GetComponents<UnityEngine.Component>();

            serializedFields.Capacity += components.Length;

            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                var componentType = component.GetType();
                var componentSerialized = reflector.Serialize(component, componentType, name: $"component_{i}", recursive: true, flags: flags);
                serializedFields.Add(componentSerialized);
            }
            return serializedFields;
        }

        protected override bool SetValue(Reflector reflector, ref object obj, Type type, JsonElement? value)
        {
            return true;
        }

        protected override StringBuilder? ModifyField(Reflector reflector, ref object obj, SerializedMember fieldValue, StringBuilder? stringBuilder = null, int depth = 0,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            var go = obj as UnityEngine.GameObject;

            var type = TypeUtils.GetType(fieldValue.type);
            if (type == null)
                return stringBuilder?.AppendLine($"[Error] Type not found: {fieldValue.type}");

            // If not a component, use base method
            if (!typeof(UnityEngine.Component).IsAssignableFrom(type))
                return base.ModifyField(reflector, ref obj, fieldValue, stringBuilder, depth, flags);

            var index = -1;
            if (fieldValue.name.StartsWith("component_"))
                int.TryParse(fieldValue.name
                    .Replace("component_", "")
                    .Replace("[", "")
                    .Replace("]", ""), out index);

            var componentInstanceID = fieldValue.GetInstanceID();
            if (componentInstanceID == 0 && index == -1)
                return stringBuilder?.AppendLine($"[Error] Component 'instanceID' is not provided. Use 'instanceID' or name '[index]' to specify the component. '{fieldValue.name}' is not valid.");

            var allComponents = go.GetComponents<UnityEngine.Component>();
            var component = componentInstanceID == 0
                ? index >= 0 && index < allComponents.Length
                    ? allComponents[index]
                    : null
                : allComponents.FirstOrDefault(c => c.GetInstanceID() == componentInstanceID);

            if (component == null)
                return stringBuilder?.AppendLine($"[Error] Component not found. Use 'instanceID' or name 'component_[index]' to specify the component.");

            var componentObject = (object)component;
            return reflector.Populate(ref componentObject, fieldValue);
        }
    }
}