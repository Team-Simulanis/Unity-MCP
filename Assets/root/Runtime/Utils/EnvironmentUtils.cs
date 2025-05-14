using System;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public static class EnvironmentUtils
    {
        public static bool IsGitHubActions
            => Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
    }
}