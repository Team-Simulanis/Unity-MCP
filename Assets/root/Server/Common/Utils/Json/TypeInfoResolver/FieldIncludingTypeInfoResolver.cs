using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace com.IvanMurzak.Unity.MCP.Common.Json
{
    public class FieldIncludingTypeInfoResolver : DefaultJsonTypeInfoResolver
    {
        // public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        // {
        //     var typeInfo = base.GetTypeInfo(type, options);

        //     // Only process object types
        //     if (typeInfo.Kind == JsonTypeInfoKind.Object)
        //     {
        //         var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
        //         foreach (var field in fields)
        //         {
        //             // Skip if already present as a property
        //             if (type.GetProperty(field.Name) != null)
        //                 continue;

        //             // Add field as a property
        //             typeInfo.Properties.Add(JsonPropertyInfo.Create(
        //                 field.FieldType,
        //                 field.Name,
        //                 options,
        //                 getter: obj => field.GetValue(obj),
        //                 setter: (obj, value) => field.SetValue(obj, value)
        //             ));
        //         }
        //     }

        //     return typeInfo;
        // }
    }
}