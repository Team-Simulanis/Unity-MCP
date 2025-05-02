#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

using System;

namespace com.IvanMurzak.Unity.MCP.Common.Data
{
    public class RequestListResourceTemplates : IRequestListResourceTemplates
    {
        public string RequestID { get; set; } = string.Empty;
        public string? Filter { get; set; }
        
        public RequestListResourceTemplates() { }
        public RequestListResourceTemplates(string requestID, string? filter = null)
        {
            RequestID = requestID ?? throw new ArgumentNullException(nameof(requestID));
            Filter = filter;
        }

        public virtual void Dispose()
        {

        }
        ~RequestListResourceTemplates() => Dispose();
    }
}