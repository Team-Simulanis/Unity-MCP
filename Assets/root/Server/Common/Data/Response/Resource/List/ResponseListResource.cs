#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.Collections.Generic;

namespace com.IvanMurzak.Unity.MCP.Common.Data
{
    public class ResponseListResource : IResponseListResource
    {
        public string uri { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string? mimeType { get; set; }
        public string? description { get; set; }
        public long? size { get; set; }
        public IList<string>? Content { get; set; }

        public ResponseListResource() { }
        
        public ResponseListResource(string uri, string name, string? mimeType = null, string? description = null, long? size = null)
        {
            this.uri = uri;
            this.name = name;
            this.mimeType = mimeType;
            this.description = description;
            this.size = size;
        }
        
        public ResponseListResource(string uri, IList<string> content, string? mimeType = null, string? description = null, long? size = null)
        {
            this.uri = uri;
            this.name = uri;
            this.Content = content;
            this.mimeType = mimeType;
            this.description = description;
            this.size = size;
        }
    }
}