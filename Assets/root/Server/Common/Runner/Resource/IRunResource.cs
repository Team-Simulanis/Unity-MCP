#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using com.IvanMurzak.Unity.MCP.Common.Data;

namespace com.IvanMurzak.Unity.MCP.Common
{
    public interface IRunResource
    {
        string Route { get; set; }
        string Name { get; set; }
        string? Description { get; set; }
        string? MimeType { get; set; }

        [JsonIgnore]
        IRunResourceContent RunGetContent { get; set; }

        [JsonIgnore]
        IRunResourceContext RunListContext { get; set; }
        
        Task<ResponseResourceContent[]> Run(string resourceId);
        Task<IList<string>> RunResourceList();
        Task<IList<ResponseResourceTemplate>> RunResourceTemplates();
    }
}