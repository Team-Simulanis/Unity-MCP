using System;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Utils;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP
{
    public partial class McpPluginUnity
    {
        [Serializable]
        public class Data
        {
            public const int DefaultPort = 60606;
            public const string DefaultHost = "http://localhost:60606";

            [SerializeField] public string host = DefaultHost;
            [SerializeField] public int port = com.IvanMurzak.Unity.MCP.Common.Utils.Consts.Hub.DefaultPort;
            [SerializeField] public bool keepConnected = true;
            [SerializeField] public LogLevel logLevel = LogLevel.Warning;

            public Data SetDefault()
            {
                host = DefaultHost;
                port = com.IvanMurzak.Unity.MCP.Common.Utils.Consts.Hub.DefaultPort;
                keepConnected = true;
                logLevel = LogLevel.Warning;
                return this;
            }
        }
    }
}