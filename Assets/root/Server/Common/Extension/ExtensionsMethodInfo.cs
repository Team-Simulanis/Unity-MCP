#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.IvanMurzak.Unity.MCP.Common.Data.Unity;
using com.IvanMurzak.Unity.MCP.Common.Utils;

namespace com.IvanMurzak.Unity.MCP.Common
{
    public static class ExtensionsMethodInfo
    {
        public static MethodInfo? FilterByParameters(this IEnumerable<MethodInfo> methods, SerializedMemberList? serializedParameters = null)
        {
            if (serializedParameters == null || serializedParameters.Count == 0)
                return methods.FirstOrDefault(m => m.GetParameters().Length == 0);

            return methods.FirstOrDefault(method =>
            {
                var methodParameters = method.GetParameters();
                for (int i = 0; i < methodParameters.Length; i++)
                {
                    var methodParameter = methodParameters[i];
                    if (i >= serializedParameters.Count)
                    {
                        if (methodParameter.IsOptional)
                            break;
                        return false;
                    }
                    var serializedParam = serializedParameters[i];

                    if (methodParameter.Name != serializedParam.name || methodParameter.ParameterType != TypeUtils.GetType(serializedParam.className))
                        return false;
                }
                return true;
            });
        }
    }
}