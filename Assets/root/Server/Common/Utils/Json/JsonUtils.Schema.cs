#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;

namespace com.IvanMurzak.Unity.MCP.Common
{
    public static partial class JsonUtils
    {
        public static JsonNode? GetSchema(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            var parameters = method.GetParameters();
            if (parameters.Length == 0)
                return new JsonObject { ["type"] = "object" };

            var properties = new JsonObject();
            var required = new JsonArray();
            // Create a schema object manually
            var schema = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = properties,
                ["required"] = required
            };

            foreach (var parameter in parameters)
            {
                // Use JsonSchemaExporter to get the schema for each parameter type
                var parameterSchema = JsonSchemaExporter.GetJsonSchemaAsNode(
                    jsonSerializerOptions,
                    type: parameter.ParameterType,
                    exporterOptions: new JsonSchemaExporterOptions
                    {
                        TreatNullObliviousAsNonNullable = true
                    });

                if (parameterSchema == null)
                    continue;

                properties[parameter.Name!] = parameterSchema;

                if (parameterSchema is JsonObject parameterSchemaObject)
                {
                    var propertyDescription = parameter.GetCustomAttribute<DescriptionAttribute>()?.Description
                        ?? parameter.ParameterType.GetCustomAttribute<DescriptionAttribute>()?.Description;
                    if (!string.IsNullOrEmpty(propertyDescription))
                        parameterSchemaObject["description"] = JsonValue.Create(propertyDescription);
                }

                // Check if the parameter has a default value
                if (!parameter.HasDefaultValue)
                    required.Add(parameter.Name!);
            }
            return schema;
        }

        public static JsonElement? ToJsonElement(this JsonNode? node)
        {
            if (node == null)
                return null;

            // Convert JsonNode to JsonElement
            var jsonString = node.ToJsonString();

            // Parse the JSON string into a JsonElement
            using var document = JsonDocument.Parse(jsonString);
            return document.RootElement.Clone();
        }

        private static bool IsNullable(Type type)
        {
            if (!type.IsValueType)
                return true; // Reference types are nullable
            return Nullable.GetUnderlyingType(type) != null; // Nullable value types
        }
    }
}