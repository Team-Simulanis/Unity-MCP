#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.Text.Json;

namespace com.IvanMurzak.Unity.MCP.Common.Data
{
    public interface IResponseListTool
    {
        string Name { get; set; }
        string? Title { get; set; }
        string? Description { get; set; }
        JsonElement InputSchema { get; set; }
    }
}