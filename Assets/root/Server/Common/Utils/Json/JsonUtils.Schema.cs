#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using com.IvanMurzak.Unity.MCP.Common.Json;

namespace com.IvanMurzak.Unity.MCP.Common
{
    public static partial class JsonUtils
    {
        public static JsonNode? GetSchema<T>() => GetSchema(typeof(T));
        public static JsonNode? GetSchema(Type type)
        {
            // Handle nullable types
            var underlyingNullableType = Nullable.GetUnderlyingType(type);
            if (underlyingNullableType != null)
                type = underlyingNullableType;

            var jsonConverter = jsonSerializerOptions.GetConverter(type);
            if (jsonConverter is IJsonSchemeConvertor schemeConvertor)
                return schemeConvertor.GetScheme();

            // Use JsonSchemaExporter to get the schema for each parameter type
            var schema = jsonSerializerOptions.GetJsonSchemaAsNode(
                type: type,
                exporterOptions: new JsonSchemaExporterOptions
                {
                    TreatNullObliviousAsNonNullable = true
                });

            if (schema == null)
                return null;

            if (schema is JsonObject parameterSchemaObject)
            {
                var propertyDescription = type.GetCustomAttribute<DescriptionAttribute>()?.Description;
                if (!string.IsNullOrEmpty(propertyDescription))
                    parameterSchemaObject["description"] = JsonValue.Create(propertyDescription);
            }
            else
            {
                throw new InvalidOperationException($"Unexpected schema type for '{type.FullName}'.\nJson Schema type: {schema.GetType()}\n");
            }
            return schema;
        }
        public static JsonNode? GetArgumentsSchema(MethodInfo method)
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
                var parameterSchema = GetSchema(parameter.ParameterType);
                if (parameterSchema == null)
                    continue;

                properties[parameter.Name!] = parameterSchema;

                if (parameterSchema is JsonObject parameterSchemaObject)
                {
                    var propertyDescription = parameter.GetCustomAttribute<DescriptionAttribute>()?.Description;
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