#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;

namespace com.IvanMurzak.Unity.MCP.Common.Data
{
    public class ResponseMenuItem
    {
        public string MenuPath { get; set; }
        public string DisplayName { get; set; }
        public bool IsEnabled { get; set; }
        public string Category { get; set; }

        public ResponseMenuItem(string menuPath, string displayName, bool isEnabled, string category)
        {
            MenuPath = menuPath;
            DisplayName = displayName;
            IsEnabled = isEnabled;
            Category = category;
        }
    }

    public class ResponseExecuteMenuItem
    {
        public string MenuPath { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }

        public ResponseExecuteMenuItem(string menuPath, bool success, string message)
        {
            MenuPath = menuPath;
            Success = success;
            Message = message;
        }
    }
} 