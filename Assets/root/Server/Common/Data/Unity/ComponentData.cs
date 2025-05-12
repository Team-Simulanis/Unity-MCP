#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.Collections.Generic;
using com.IvanMurzak.Unity.MCP.Common.Data.Unity;

namespace com.IvanMurzak.Unity.MCP.Common.Data.Unity
{
    [System.Serializable]
    public class ComponentData : ComponentDataLight
    {
        public List<SerializedMember> fields { get; set; } = new();
        public List<SerializedMember> properties { get; set; } = new();

        public ComponentData() { }
    }
}