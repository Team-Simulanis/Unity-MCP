// #pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
// using System;
// using System.Collections.Generic;
// using System.Text.Json;
// using System.Text.Json.Serialization;
// using com.IvanMurzak.Unity.MCP.Common.Data.Unity;

// namespace com.IvanMurzak.Unity.MCP.Common.Json
// {
//     public class SerializedMemberConverterFactory : JsonConverterFactory
//     {
//         public override bool CanConvert(Type typeToConvert)
//         {
//             return typeToConvert.IsGenericType &&
//                    typeToConvert.GetGenericTypeDefinition() == typeof(SerializedMember<>);
//         }

//         public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
//         {
//             var elementType = type.GetGenericArguments()[0];
//             var converterType = typeof(SerializedMemberConverter<>).MakeGenericType(elementType);
//             return (JsonConverter)Activator.CreateInstance(converterType)!;
//         }
//     }
// }