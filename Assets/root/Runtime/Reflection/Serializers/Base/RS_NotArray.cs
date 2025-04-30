#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using com.IvanMurzak.Unity.MCP.Common.Utils;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public abstract class RS_NotArray<T> : RS_Base<T>
    {
        public override int SerializationPriority(Type type)
        {
            var distance = TypeUtils.GetInheritanceDistance(baseType: typeof(T), targetType: type);
            if (distance >= 0)
                return MAX_DEPTH - distance;

            var isArray = type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
            return isArray
                ? 0
                : base.SerializationPriority(type);
        }
    }
}