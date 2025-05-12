#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;

namespace com.IvanMurzak.Unity.MCP.Common.Json
{
    public class ObjectRefConverter : JsonConverter<ObjectRef>
    {
        public override ObjectRef? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            var instanceID = new ObjectRef();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read(); // Move to the value token

                    switch (propertyName)
                    {
                        case "instanceID":
                            instanceID.instanceID = reader.GetInt32();
                            break;
                        case "assetPath":
                            instanceID.assetPath = reader.GetString();
                            break;
                        default:
                            // Skip unknown properties
                            reader.Skip();
                            break;
                    }
                }
            }

            return instanceID;
        }

        public override void Write(Utf8JsonWriter writer, ObjectRef value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            // Write the "instanceID" property
            writer.WritePropertyName("instanceID");
            writer.WriteNumberValue(value.instanceID);

            // Write the "assetPath" property
            if (!string.IsNullOrEmpty(value.assetPath))
            {
                writer.WritePropertyName("assetPath");
                writer.WriteStringValue(value.assetPath);
            }

            writer.WriteEndObject();
        }
    }
}