#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.Collections.Generic;
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

            public static string TestTimeout(int timeoutMs)
                => $"[Error] Test execution timed out after {timeoutMs} ms";

            public static string TestRunnerNotAvailable()
                => $"[Error] Unity Test Runner API is not available";

            public static string NoTestsFound(string? testAssembly, string? testClass, string? testMethod)
            {
                var filters = new List<string>();
                if (!string.IsNullOrEmpty(testAssembly)) filters.Add($"assembly '{testAssembly}'");
                if (!string.IsNullOrEmpty(testClass)) filters.Add($"class '{testClass}'");
                if (!string.IsNullOrEmpty(testMethod)) filters.Add($"method '{testMethod}'");
                
                var filterText = filters.Count > 0 ? $" matching {string.Join(", ", filters)}" : "";
                return $"[Error] No tests found{filterText}. Please check that the specified assembly, class, and method names are correct and that your Unity project contains tests.";
            }
        }
    }
}