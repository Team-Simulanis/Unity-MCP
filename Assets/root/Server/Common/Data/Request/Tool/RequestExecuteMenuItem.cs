#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;

namespace com.IvanMurzak.Unity.MCP.Common.Data
{
    public interface IRequestExecuteMenuItem : IRequestID, IDisposable
    {
        string MenuPath { get; }
    }

    public class RequestExecuteMenuItem : IRequestExecuteMenuItem
    {
        public string RequestID { get; set; }
        public string MenuPath { get; }

        public RequestExecuteMenuItem(string menuPath)
            : this(Guid.NewGuid().ToString(), menuPath)
        {
        }

        public RequestExecuteMenuItem(string requestID, string menuPath)
        {
            RequestID = requestID;
            MenuPath = menuPath;
        }
        
        public virtual void Dispose()
        {
        }
        
        ~RequestExecuteMenuItem() => Dispose();
    }
} 