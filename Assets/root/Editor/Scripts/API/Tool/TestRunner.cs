#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using com.IvanMurzak.Unity.MCP.Common;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    [McpPluginToolType]
    public partial class Tool_TestRunner
    {
        private static class Error
        {
            public static string InvalidTestMode(string testMode)
                => $"[Error] Invalid test mode '{testMode}'. Valid modes: EditMode, PlayMode, All";

            public static string TestExecutionFailed(string reason)
                => $"[Error] Test execution failed: {reason}";

            public static string TestTimeout(int timeoutSeconds)
                => $"[Error] Test execution timed out after {timeoutSeconds} seconds";

            public static string TestAssemblyNotFound(string assemblyName)
                => $"[Error] Test assembly '{assemblyName}' not found";

            public static string TestClassNotFound(string className)
                => $"[Error] Test class '{className}' not found";

            public static string TestMethodNotFound(string methodName)
                => $"[Error] Test method '{methodName}' not found";

            public static string TestRunnerNotAvailable()
                => $"[Error] Unity Test Runner API is not available";
        }
    }
}