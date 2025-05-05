// using ModelContextProtocol.Protocol.Types;
// using ModelContextProtocol.Server;
// using System.ComponentModel;
// using System.Threading.Tasks;

// namespace com.IvanMurzak.Unity.MCP.Server.API
// {
//     public partial class Tool_Assets
//     {
//         [McpServerTool
//         (
//             Name = "Assets_Load",
//             Title = "Assets Load"
//         )]
//         [Description(@"Load specific asset to get the 'instanceID' which could be used later for linking asset.")]
//         public Task<CallToolResponse> Load
//         (
//             [Description("Path to the asset. See 'Assets_Search' for more details. Starts with 'Assets/' or 'Packages/'. Priority: 1. (Recommended)")]
//             string? assetPath = null,
//             [Description("GUID of the asset. Priority: 2.")]
//             string? assetGuid = null
//         )
//         {
//             return ToolRouter.Call("Assets_Load", arguments =>
//             {
//                 if (assetPath != null && assetPath.Length > 0)
//                     arguments[nameof(assetPath)] = assetPath;

//                 if (assetGuid != null && assetGuid.Length > 0)
//                     arguments[nameof(assetGuid)] = assetGuid;
//             });
//         }
//     }
// }