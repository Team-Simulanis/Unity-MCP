#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace com.IvanMurzak.Unity.MCP.Common.Data.Unity
{
    [System.Serializable]
    [Description("Reference to UnityEngine.Object instance. It could be GameObject, Component, Asset, etc.")]
    public class ObjectRef
    {
        [JsonInclude]
        [JsonPropertyName("instanceID")]
        public int instanceID;

        [JsonInclude]
        [JsonPropertyName("assetPath")]
        public string? assetPath;

        public ObjectRef() { }
        public ObjectRef(int id) => instanceID = id;
        public ObjectRef(string assetPath) => this.assetPath = assetPath;

        public override string ToString()
            => string.IsNullOrEmpty(assetPath)
                ? $"instanceID={instanceID}"
                : instanceID == 0
                    ? $"assetPath={assetPath}"
                    : $"instanceID={instanceID}, assetPath={assetPath}";
    }
}