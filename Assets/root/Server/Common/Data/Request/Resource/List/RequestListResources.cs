#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

using System;

namespace com.IvanMurzak.Unity.MCP.Common.Data
{
    public class RequestListResources : IRequestListResources
    {
        public string RequestID { get; set; } = Guid.NewGuid().ToString();
        public string? Cursor { get; set; }
        public string? Filter { get; set; }

        public RequestListResources() { }
        public RequestListResources(string? cursor = null, string? filter = null)
            : this(Guid.NewGuid().ToString(), cursor, filter) { }
        public RequestListResources(string requestId, string? cursor = null, string? filter = null)
        {
            RequestID = requestId ?? throw new ArgumentNullException(nameof(requestId));
            Cursor = cursor;
            Filter = filter;
        }

        public virtual void Dispose()
        {

        }
        ~RequestListResources() => Dispose();
    }
}