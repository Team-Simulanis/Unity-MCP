#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.ComponentModel;
using System.Linq;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data.Unity;
using com.IvanMurzak.Unity.MCP.Utils;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Reflection
    {
        [McpPluginTool
        (
            "Reflection_MethodCall",
            Title = "Find method using reflection"
        )]
        [Description("Find method in the project using C# Reflection.")]
        public string MethodCall
        (
            [Description("Filter.")]
            MethodRef filter,
            bool executeInMainThread = true
        )
        {

            

            return $"[Success] Scene created at";
        }
    }
}