#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;

namespace com.IvanMurzak.Unity.MCP.Common.Data
{
    public class RequestExecuteMenuItem : IRequestExecuteMenuItem
    {
        public string RequestID { get; set; } = Guid.NewGuid().ToString();
        public string MenuPath { get; set; }

        public RequestExecuteMenuItem() { }
        public RequestExecuteMenuItem(string menuPath)
        {
            MenuPath = menuPath;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
} 