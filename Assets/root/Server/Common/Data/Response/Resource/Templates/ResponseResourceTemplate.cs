#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
namespace com.IvanMurzak.Unity.MCP.Common.Data
{
    public class ResponseResourceTemplate : IResponseResourceTemplate
    {
        public string uriTemplate { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string? mimeType { get; set; }
        public string? description { get; set; }
        
        // Alias for uriTemplate for easier usage
        public string Uri
        {
            get => uriTemplate;
            set => uriTemplate = value;
        }
        
        // Alias for name for easier usage
        public string Title
        {
            get => name;
            set => name = value;
        }

        public ResponseResourceTemplate() { }
        public ResponseResourceTemplate(string uri, string name, string? mimeType = null, string? description = null)
        {
            this.uriTemplate = uri;
            this.name = name;
            this.mimeType = mimeType;
            this.description = description;
        }
    }
}