#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;

namespace com.IvanMurzak.Unity.MCP.Common.Data
{
    public interface IRequestListMenuItems : IRequestID, IDisposable
    {
        string? Filter { get; set; }
        string? ParentPath { get; set; }
    }

    public interface IRequestExecuteMenuItem : IRequestID, IDisposable
    {
        string MenuPath { get; set; }
    }
} 