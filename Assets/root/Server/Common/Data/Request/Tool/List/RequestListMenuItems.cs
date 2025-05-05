#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;

namespace com.IvanMurzak.Unity.MCP.Common.Data
{
    public class RequestListMenuItems : IRequestListMenuItems
    {
        public string RequestID { get; set; } = Guid.NewGuid().ToString();
        public string? Filter { get; set; }
        public string? ParentPath { get; set; }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
} 