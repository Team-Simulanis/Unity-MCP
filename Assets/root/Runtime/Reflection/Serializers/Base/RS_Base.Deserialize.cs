#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;
using com.IvanMurzak.Unity.MCP.Common.Utils;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public abstract partial class RS_Base<T> : IReflectionSerializer
    {
        public virtual object? Deserialize(SerializedMember data)
        {
            var type = TypeUtils.GetType(data.type);
            if (type == null)
                return null;

            var result = data.valueJsonElement != null
                ? JsonUtils.Deserialize(data.valueJsonElement.Value, type)
                : null;

            return result;
        }        
    }
}