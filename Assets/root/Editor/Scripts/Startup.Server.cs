#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using com.IvanMurzak.Unity.MCP.Common;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using com.IvanMurzak.Unity.MCP.Utils;
using System.Linq;
using System.Runtime.CompilerServices;

namespace com.IvanMurzak.Unity.MCP.Editor
{
    static partial class Startup
    {
        public const string PackageName = "com.simulanis.unity.mcp";
        public const string ServerProjectName = "com.IvanMurzak.Unity.MCP.Server";

        // Server source path
        public static string PackageCache => Path.GetFullPath(Path.Combine(Application.dataPath, "../Library", "PackageCache"));

        // Helper to get the directory of the current script file
        private static string GetCurrentScriptDirectory([CallerFilePath] string sourceFilePath = "")
        {
            return Path.GetDirectoryName(sourceFilePath);
        }

        public static string? ServerSourcePath
        {
            get
            {
                // Get the directory where this script (Startup.Server.cs) is located
                string scriptDir = GetCurrentScriptDirectory();
                
                // Navigate up from Editor/Scripts to the package root
                // Assumes structure: <PackageRoot>/Editor/Scripts/Startup.Server.cs
                string packageRootPath = Path.GetFullPath(Path.Combine(scriptDir, "..", "..")); 

                // Construct the path to the Server source within the package
                string serverSourceInPackage = Path.Combine(packageRootPath, "Server");

                // Check if the Server directory exists at the calculated path
                if (Directory.Exists(serverSourceInPackage))
                {
                    return serverSourceInPackage;
                }
                else
                {
                    // Fallback for local development environment (if needed, though maybe remove later)
                    string devPath = Path.GetFullPath(Path.Combine(Application.dataPath, "root", "Server"));
                    if (Directory.Exists(devPath))
                    {
                        return devPath;
                    }
                }
                
                // If neither path is found, return null or log an error
                Debug.LogError($"[{PackageName}] Could not determine Server source path. Checked: {serverSourceInPackage} and dev path.");
                return null; 
            }
        }

        // Server executable path
        public static string ServerExecutableRootPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../Library", PackageName.ToLower()));
        public static string ServerExecutableFolder => Path.Combine(ServerExecutableRootPath, "bin~", "Release", "net9.0");
        public static string ServerExecutableFile => Path.Combine(ServerExecutableFolder, $"{ServerProjectName}");

        // Log files
        public static string ServerLogsPath => Path.Combine(ServerExecutableFolder, "logs", "server-log.txt");
        public static string ServerErrorLogsPath => Path.Combine(ServerExecutableFolder, "logs", "server-log-error.txt");

        // Version files
        public static string ServerSourceVersionPath => Path.GetFullPath(Path.Combine(ServerSourcePath, "version"));
        public static string ServerExecutableVersionPath => Path.GetFullPath(Path.Combine(ServerExecutableFolder, "version"));

        // Versions
        public static string ServerSourceVersion => FileUtils.ReadFileContent(ServerSourceVersionPath)?.Trim() ?? "unknown";
        public static string ServerExecutableVersion => FileUtils.ReadFileContent(ServerExecutableVersionPath)?.Trim() ?? "unknown";

        // Verification
        public static bool IsServerCompiled => FileUtils.FileExistsWithoutExtension(ServerExecutableFolder, ServerProjectName);
        public static bool ServerVersionMatched => ServerSourceVersion == ServerExecutableVersion;

        // -------------------------------------------------------------------------------------------------------------------------------------------------

        public static string RawJsonConfiguration(int port) => Consts.MCP_Client.ClaudeDesktop.Config(
            ServerExecutableFile.Replace('\\', '/'),
            port
        );

        public static Task BuildServerIfNeeded(bool force = true)
        {
            if (IsServerCompiled && ServerVersionMatched)
                return Task.CompletedTask;

            return BuildServer(force);
        }

        public static async Task BuildServer(bool force = true)
        {
            var message = "<b><color=yellow>Server Build</color></b>";
            Debug.Log($"{Consts.Log.Tag} {message} <color=orange>⊂(◉‿◉)つ</color>");
            Debug.Log($"{Consts.Log.Tag} Current Server version: <color=#8CFFD1>{ServerExecutableVersion}</color>. New Server version: <color=#8CFFD1>{ServerSourceVersion}</color>");

            CopyServerSources();
            DeleteSlnFiles();

            Debug.Log($"{Consts.Log.Tag} Building server at <color=#8CFFD1>{ServerExecutableRootPath}</color>");

            (string output, string error) = await ProcessUtils.Run(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "build -c Release",
                WorkingDirectory = ServerExecutableRootPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            await MainThread.RunAsync(() => HandleBuildResult(output, error, force));
        }

        private static async Task HandleBuildResult(string output, string error, bool force)
        {
            var isError = !string.IsNullOrEmpty(error) ||
                output.Contains("Build FAILED") ||
                output.Contains("MSBUILD : error") ||
                output.Contains("error MSB");

            if (isError)
            {
                Debug.LogError($"{Consts.Log.Tag} <color=red>Build failed</color>. Check the output for details:\n{output}");
                if (force)
                {
                    if (ErrorUtils.ExtractProcessId(output, out var processId))
                    {
                        Debug.Log($"{Consts.Log.Tag} Detected another process which locks the file. Killing the process with ID: {processId}");
                        // Kill the process that locks the file
                        (string _output, string _error) = await ProcessUtils.Run(new ProcessStartInfo
                        {
                            FileName = "taskkill",
                            Arguments = $"/PID {processId} /F",
                            WorkingDirectory = ServerExecutableRootPath,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        });
                        Debug.Log($"{Consts.Log.Tag} Trying to rebuild server one more time");
                        await BuildServer(force: false);
                    }
                    else
                    {
                        await BuildServer(force: false);
                    }
                }
            }
            else
            {
                Debug.Log($"{Consts.Log.Tag} <color=green>Build succeeded</color>. Check the output for details:\n{output}");
            }
        }

        public static void CopyServerSources()
        {
            Debug.Log($"{Consts.Log.Tag} Delete sources at: <color=#8CFFD1>{ServerExecutableRootPath}</color>");
            try
            {
                DirectoryUtils.Delete(ServerExecutableRootPath, recursive: true);
            }
            catch (UnauthorizedAccessException) { /* ignore */ }

            var sourceDir = ServerSourcePath;
            Debug.Log($"{Consts.Log.Tag} Copy sources from: <color=#8CFFD1>{sourceDir}</color>");
            try
            {
                DirectoryUtils.Copy(sourceDir, ServerExecutableRootPath, "*/bin~", "*/obj~", "*\\bin~", "*\\obj~", "*.meta");
            }
            catch (DirectoryNotFoundException ex)
            {
                Debug.LogError($"{Consts.Log.Tag} Server source directory not found. Please check the path: <color=#8CFFD1>{PackageCache}</color>");
                Debug.LogError($"{Consts.Log.Tag} It may happen if the package was added into a project using local path reference. Please consider to use a package from the registry instead. Follow official installation instructions at https://github.com/IvanMurzak/Unity-MCP");
                Debug.LogException(ex);
            }
        }
        public static void DeleteSlnFiles()
        {
            var slnFiles = Directory.GetFiles(ServerExecutableRootPath, "*.sln*", SearchOption.TopDirectoryOnly);
            foreach (var slnFile in slnFiles)
            {
                try
                {
                    File.Delete(slnFile);
                }
                catch (UnauthorizedAccessException) { /* ignore */ }
            }
        }
    }
}