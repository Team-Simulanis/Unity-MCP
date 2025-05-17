#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.Diagnostics;
using System.Threading.Tasks;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Editor.Utils;

namespace com.IvanMurzak.Unity.MCP.Editor
{
    static partial class Startup
    {
        const string AcceptableDotNetVersionSubstring = "9.";
        const string DefaultDotNetVersion = "9.0.300";
        public static async Task InstallDotNetIfNeeded(string version = DefaultDotNetVersion, bool force = false)
        {
            // Check if .NET SDK is installed
            if (force)
            {
                UnityEngine.Debug.Log($"{Consts.Log.Tag} Force installing .NET SDK...");
            }
            else
            {
                var isDotnetInstalled = await IsDotNetInstalled();
                if (isDotnetInstalled)
                    return;
                UnityEngine.Debug.Log($"{Consts.Log.Tag} .NET SDK is not installed. Installing...");
            }

            // Install .NET SDK if not installed
#if UNITY_EDITOR_WIN
            await InstallDotnet_Windows(version);
#elif UNITY_EDITOR_OSX
            await InstallDotnet_MacOS(version);
#elif UNITY_EDITOR_LINUX
            await InstallDotnet_MacOS(version);
            // await InstallDotnet_Linux(version);
#else
            Debug.LogError($"{Consts.Log.Tag} Unsupported platform for .NET SDK installation.");
            return;
#endif
        }

        public static async Task<bool> IsDotNetInstalled()
        {
            var (output, error) = await ProcessUtils.Run(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--version"
            }, suppressError: true);

            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.Log($"{Consts.Log.Tag} .NET SDK is not installed.");
                // UnityEngine.Debug.LogError($"{Consts.Log.Tag} Error checking .NET SDK version: {error}");
                return false;
            }

            if (string.IsNullOrEmpty(output))
            {
                UnityEngine.Debug.Log($"{Consts.Log.Tag} .NET SDK is not installed.");
                // UnityEngine.Debug.LogError($"{Consts.Log.Tag} .NET SDK is not installed.");
                return false;
            }

            UnityEngine.Debug.Log($"{Consts.Log.Tag} .NET SDK version: {output}");
            return output.StartsWith(AcceptableDotNetVersionSubstring);
        }

        static async Task InstallDotnet_Windows(string version)
        {
            var tempScript = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dotnet-install.ps1");

            UnityEngine.Debug.Log($"{Consts.Log.Tag} Downloading .NET SDK installer script...");
            var (downloadOutput, downloadError) = await ProcessUtils.Run(new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-Command \"Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile '{tempScript}'\""
            });

            if (!string.IsNullOrEmpty(downloadError))
            {
                UnityEngine.Debug.LogError($"{Consts.Log.Tag} Error downloading installer: {downloadError}");
                return;
            }

            UnityEngine.Debug.Log($"{Consts.Log.Tag} Installing .NET SDK version {version}...");
            var (output, error) = await ProcessUtils.Run(new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-ExecutionPolicy Bypass -File \"{tempScript}\" -Version {version}"
            });

            if (!string.IsNullOrEmpty(error))
                UnityEngine.Debug.LogError($"{Consts.Log.Tag} Installation error: {error}");
            else
                UnityEngine.Debug.Log($"{Consts.Log.Tag} .NET SDK {version} installed successfully");
        }
        static async Task InstallDotnet_MacOS(string version)
        {
            var (output, error) = await ProcessUtils.Run(new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-c \"curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version {version} --install-dir $HOME/.dotnet\""
            });

            if (!string.IsNullOrEmpty(output))
                UnityEngine.Debug.Log(output);

            if (!string.IsNullOrEmpty(error))
                UnityEngine.Debug.LogError(error);
        }
        static async Task InstallDotnet_Linux(string version)
        {
            var (output, error) = await ProcessUtils.Run(new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-c \"wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && chmod +x dotnet-install.sh && ./dotnet-install.sh --version {version}\""
            });

            if (!string.IsNullOrEmpty(output))
                UnityEngine.Debug.Log(output);

            if (!string.IsNullOrEmpty(error))
                UnityEngine.Debug.LogError(error);
        }
    }
}