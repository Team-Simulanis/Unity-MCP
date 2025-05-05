#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
namespace com.IvanMurzak.Unity.MCP.Common.Utils
{
    public static class Consts
    {
        public static class Env
        {
            public const string Port = "UNITY_MCP_PORT";
        }
        public static class Hub
        {
            public const int DefaultPort = 60606;
            public const int MaxPort = 65535;
            public const string DefaultEndpoint = "http://localhost:60606";
            public const string RemoteApp = "/mcp/remote-app";
            public const float TimeoutSeconds = 10f;
        }

        public static class RPC
        {
            // Connection Id
            public const string ConnectionId = "connectionId";
            
            // Hub Name
            public const string RemoteApp = "/mcp/remote-app";
            public const float TimeoutSeconds = 10f;

            public static class Client
            {
                public const string RunCallTool = "/mcp/run-call-tool";
                public const string RunListTool = "/mcp/run-list-tool";
                public const string RunResourceContent = "/mcp/run-resource-content";
                public const string RunListResources = "/mcp/run-list-resources";
                public const string RunListResourceTemplates = "/mcp/run-list-resource-templates";
                public const string RunListMenuItems = "/mcp/run-list-menu-items";
                public const string RunExecuteMenuItem = "/mcp/run-execute-menu-item";
            }

            public static class Server
            {
                public const string GetToolInfo = nameof(GetToolInfo);
                public const string GetResourceInfo = nameof(GetResourceInfo);

                public const string ToggleConnection = nameof(ToggleConnection);
                public const string GetConnectionInfo = nameof(GetConnectionInfo);

                public const string CallTool = nameof(CallTool);
                public const string ToolSubscribe = nameof(ToolSubscribe);

                public const string ListResources = nameof(ListResources);
                public const string ResourceContent = nameof(ResourceContent);
                public const string ResourceTemplates = nameof(ResourceTemplates);
            }
        }

        public static class Guid
        {
            public const string Zero = "00000000-0000-0000-0000-000000000000";
        }
    }
}
#pragma warning restore CS8632