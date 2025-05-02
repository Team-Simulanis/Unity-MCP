#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.Text.Json;
using System.Text.Json.Nodes;

namespace com.IvanMurzak.Unity.MCP.Common.Data
{
    public class ResponseListTool : IResponseListTool
    {
        public string Name { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public JsonNode? InputSchema { get; set; }

        public ResponseListTool() { }
        
        public ResponseListTool(string name, string? title, string? description, JsonNode? inputSchema)
        {
            Name = name;
            Title = title;
            Description = description;
            InputSchema = inputSchema;
        }
    }
}