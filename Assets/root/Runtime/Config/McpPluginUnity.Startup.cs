using System;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Json.Converters;
using com.IvanMurzak.Unity.MCP.Utils;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP
{
    using LogLevelMicrosoft = Microsoft.Extensions.Logging.LogLevel;
    using LogLevel = Utils.LogLevel;

    public partial class McpPluginUnity
    {
        public static void BuildAndStart()
        {
            McpPlugin.StaticDisposeAsync();

            var mcpPlugin = new McpPluginBuilder()
                .WithAppFeatures()
                .WithConfig(config =>
                {
                    if (McpPluginUnity.LogLevel.IsActive(LogLevel.Log))
                        Debug.Log($"{Consts.Log.Tag} MCP server address: {McpPluginUnity.Host}");

                    config.Endpoint = McpPluginUnity.Host;
                })
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders(); // 👈 Clears the default providers
                    loggingBuilder.AddProvider(new UnityLoggerProvider());
                    loggingBuilder.SetMinimumLevel(McpPluginUnity.LogLevel switch
                    {
                        LogLevel.Trace => LogLevelMicrosoft.Trace,
                        LogLevel.Log => LogLevelMicrosoft.Information,
                        LogLevel.Warning => LogLevelMicrosoft.Warning,
                        LogLevel.Error => LogLevelMicrosoft.Error,
                        LogLevel.Exception => LogLevelMicrosoft.Critical,
                        _ => LogLevelMicrosoft.Warning
                    });
                })
                .WithToolsFromAssembly(AppDomain.CurrentDomain.GetAssemblies())
                .WithPromptsFromAssembly(AppDomain.CurrentDomain.GetAssemblies())
                .WithResourcesFromAssembly(AppDomain.CurrentDomain.GetAssemblies())
                .Build();

            if (McpPluginUnity.KeepConnected)
            {
                if (McpPluginUnity.LogLevel.IsActive(LogLevel.Log))
                {
                    var message = "<b><color=yellow>Connecting</color></b>";
                    Debug.Log($"{Consts.Log.Tag} {message} <color=orange>ಠ‿ಠ</color>");
                }
                mcpPlugin.Connect();
            }
        }

        public static void RegisterJsonConverters()
        {
            JsonUtils.AddConverter(new Color32Converter());
            JsonUtils.AddConverter(new ColorConverter());
            JsonUtils.AddConverter(new Matrix4x4Converter());
            JsonUtils.AddConverter(new QuaternionConverter());
            JsonUtils.AddConverter(new Vector2Converter());
            JsonUtils.AddConverter(new Vector2IntConverter());
            JsonUtils.AddConverter(new Vector3Converter());
            JsonUtils.AddConverter(new Vector3IntConverter());
            JsonUtils.AddConverter(new Vector4Converter());
        }
    }
}