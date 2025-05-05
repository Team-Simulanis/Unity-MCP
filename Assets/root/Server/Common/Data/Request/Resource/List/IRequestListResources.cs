#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;

namespace com.IvanMurzak.Unity.MCP.Common.Data
{
    public interface IRequestListResources : IRequestID, IDisposable
    {
        public string? Cursor { get; set; }
        public string? Filter { get; set; }
    }
}