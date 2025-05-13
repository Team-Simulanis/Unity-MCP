#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.Reflection;
using System.Text;
using com.IvanMurzak.Unity.MCP.Common.Data.Unity;
using com.IvanMurzak.Unity.MCP.Common.Utils;

namespace com.IvanMurzak.Unity.MCP.Common
{
    public static class ExtensionsSerializedMemberList
    {
        public static bool IsValidClassNames(this SerializedMemberList? parameters, string fieldName, out string? error)
        {
            if (parameters == null || parameters.Count == 0)
            {
                error = null;
                return true;
            }

            var result = true;
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                if (string.IsNullOrEmpty(parameter.className))
                {
                    stringBuilder.AppendLine($"[Error] {fieldName}[{i}].{nameof(parameter.className)} is empty. Please specify the '{nameof(parameter.name)}' properly.");
                    result = false;
                    continue;
                }

                var parameterType = TypeUtils.GetType(parameter.className);
                if (parameterType == null)
                {
                    stringBuilder.AppendLine($"[Error] {fieldName}[{i}].{nameof(parameter.className)} type '{parameter.className}' not found. Please specify the '{nameof(parameter.name)}' properly.");
                    result = false;
                    continue;
                }
            }

            error = stringBuilder.ToString();

            if (string.IsNullOrEmpty(error))
                error = null;

            return result;
        }

        public static void EnhanceNames(this SerializedMemberList? parameters, MethodInfo method)
        {
            if (parameters == null || parameters.Count == 0)
                return;

            var methodParameters = method.GetParameters();

            for (int i = 0; i < parameters.Count && i < methodParameters.Length; i++)
            {
                var parameter = parameters[i];
                if (string.IsNullOrEmpty(parameter.name))
                {
                    var methodParameter = methodParameters[i];
                    parameter.name = methodParameter.Name;
                }
            }
        }

        public static void EnhanceTypes(this SerializedMemberList? parameters, MethodInfo method)
        {
            if (parameters == null || parameters.Count == 0)
                return;

            var methodParameters = method.GetParameters();

            for (int i = 0; i < parameters.Count && i < methodParameters.Length; i++)
            {
                var parameter = parameters[i];
                if (string.IsNullOrEmpty(parameter.className))
                {
                    var methodParameter = methodParameters[i];
                    var typeName = methodParameter?.ParameterType?.FullName;
                    if (typeName == null)
                        continue;
                    parameter.className = typeName;
                }
            }
        }
    }
}