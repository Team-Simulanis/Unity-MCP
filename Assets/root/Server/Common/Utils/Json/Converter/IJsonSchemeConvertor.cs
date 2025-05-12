#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.Text.Json.Nodes;

namespace com.IvanMurzak.Unity.MCP.Common.Json
{
    public interface IJsonSchemeConvertor
    {
        JsonNode GetScheme();
    }
}