// #pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
// using System;
// using System.Collections.Generic;
// using System.Text.Json;
// using System.Text.Json.Serialization;
// using com.IvanMurzak.Unity.MCP.Common.Data.Unity;

// namespace com.IvanMurzak.Unity.MCP.Common.Json
// {
//     public class SerializedMemberConverter<T> : JsonConverter<SerializedMember<T>>
//     {
//         public override SerializedMember<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//         {
//             if (reader.TokenType == JsonTokenType.Null)
//             {
//                 return null;
//             }

//             var member = new SerializedMember<T>();

//             while (reader.Read())
//             {
//                 if (reader.TokenType == JsonTokenType.EndObject)
//                 {
//                     break;
//                 }

//                 if (reader.TokenType == JsonTokenType.PropertyName)
//                 {
//                     var propertyName = reader.GetString();
//                     reader.Read(); // Move to the value token

//                     switch (propertyName)
//                     {
//                         case "name":
//                             member.name = reader.GetString();
//                             break;
//                         case "type":
//                             member.type = reader.GetString();
//                             break;
//                         case "value":
//                             // Handle the "value" property
//                             // Deserialize the "value" field based on the type
//                             var valueType = TypeUtils.GetType(member.type);
//                             if (valueType != null)
//                             {
//                                 member.Value = JsonUtils.Deserialize<T>(ref reader, options);
//                             }
//                             else
//                             {
//                                 throw new JsonException($"Unknown type: {member.type}");
//                                 // try
//                                 // {
//                                 //     using (var jsonDocument = JsonDocument.Parse(ref reader.ToRea, JsonUtils.JsonSerializerOptions))
//                                 //     {
//                                 //         jsonDocument.RootElement.WriteTo(writer);
//                                 //     }
//                                 // }
//                                 // catch
//                                 // {
//                                 //     // If the Json property is invalid, write an empty object
//                                 //     writer.WriteStartObject();
//                                 //     writer.WriteEndObject();
//                                 // }
//                             }
//                             break;
//                         case "json":
//                             // Handle the "json" property
//                             // Read the raw JSON string and store it in the member
//                             member.json = reader.GetString();
//                             break;
//                         case "properties":
//                             // Handle the "properties" property
//                             // Deserialize the properties as a list of SerializedMember objects
//                             member.properties = JsonUtils.Deserialize<List<SerializedMember>>(ref reader, options);
//                             break;
//                         default:
//                             // Skip unknown properties
//                             reader.Skip();
//                             break;
//                     }
//                 }
//             }


//             return member;
//         }

//         public override void Write(Utf8JsonWriter writer, SerializedMember<T> value, JsonSerializerOptions options)
//         {
//             if (value == null)
//             {
//                 writer.WriteNullValue();
//                 return;
//             }

//             writer.WriteStartObject();

//             // Write the "name" property
//             writer.WriteString("name", value.name);

//             // Write the "type" property
//             writer.WriteString("type", value.type);

//             if (value.data != null)
//             {
//                 try
//                 {
//                     var jsonElement = JsonUtils.SerializeToElement(value.data, options);

//                     writer.WritePropertyName("value");
//                     jsonElement.WriteTo(writer);
//                 }
//                 catch (Exception ex)
//                 {
//                     // Handle or log the exception as needed
//                     throw new JsonException($"Failed to serialize value: {ex.Message}", ex);
//                 }
//             }
//             else if (!string.IsNullOrEmpty(value.json))
//             {
//                 writer.WritePropertyName("json");
//                 writer.WriteRawValue(value.json);
//             }
//             // Write the "json" property as an unwrapped object
//             // if (!string.IsNullOrEmpty(json))
//             // {
//             //     try
//             //     {
//             //         using (var jsonDocument = JsonDocument.Parse(json))
//             //         {
//             //             jsonDocument.RootElement.WriteTo(writer);
//             //         }
//             //     }
//             //     catch
//             //     {
//             //         // If the Json property is invalid, write an empty object
//             //         writer.WriteStartObject();
//             //         writer.WriteEndObject();
//             //     }
//             // }
//             // else
//             // {
//             //     // If the Json property is null or empty, write an empty object
//             //     writer.WriteStartObject();
//             //     writer.WriteEndObject();
//             // }

//             writer.WriteEndObject();
//         }
//     }
// }