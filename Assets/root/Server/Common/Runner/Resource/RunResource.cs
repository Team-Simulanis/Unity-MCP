#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using com.IvanMurzak.Unity.MCP.Common.Data;

namespace com.IvanMurzak.Unity.MCP.Common
{
    public class RunResource : IRunResource
    {
        public string Route { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? MimeType { get; set; }

        [JsonIgnore]
        public IRunResourceContent RunGetContent { get; set; }

        [JsonIgnore]
        public IRunResourceContext RunListContext { get; set; }

        public RunResource(string route, string name, IRunResourceContent runnerGetContent, IRunResourceContext runnerListContext, string? description = null, string? mimeType = null)
        {
            Route = route;
            Name = name;
            RunGetContent = runnerGetContent;
            RunListContext = runnerListContext;
            Description = description;
            MimeType = mimeType;
        }
        
        public async Task<ResponseResourceContent[]> Run(string resourceId)
        {
            return await RunGetContent.Run(resourceId);
        }
        
        public async Task<IList<string>> RunResourceList()
        {
            var results = await RunListContext.Run();
            
            // Extract the resource URIs from the ResponseListResource array
            var resourceUris = new List<string>();
            foreach (var result in results)
            {
                // Assuming each ResponseListResource corresponds to a single URI
                resourceUris.Add(result.uri);
            }
            
            return resourceUris;
        }
        
        public async Task<IList<ResponseResourceTemplate>> RunResourceTemplates()
        {
            var templates = new List<ResponseResourceTemplate>();
            var resourceUris = await RunResourceList();
            
            foreach (var resourceUri in resourceUris)
            {
                templates.Add(new ResponseResourceTemplate(
                    $"{Route}:{resourceUri}", 
                    resourceUri, 
                    MimeType, 
                    Description
                ));
            }
            
            return templates;
        }
    }
}