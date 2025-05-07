#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data.Unity;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    [McpPluginToolType]
    public partial class Tool_Reflection
    {
        static IEnumerable<Type> AllTypes => AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes());

        static IEnumerable<MethodInfo> AllMethods => AllTypes
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            .Where(method => method.DeclaringType != null && !method.DeclaringType.IsAbstract);

        static int Compare(string original, string value)
        {
            if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(value))
                return 0;

            if (original.Equals(value, StringComparison.OrdinalIgnoreCase))
                return original.Equals(value)
                    ? 6
                    : 5;

            if (original.StartsWith(value, StringComparison.OrdinalIgnoreCase))
                return original.StartsWith(value)
                    ? 4
                    : 3;

            if (original.Contains(value, StringComparison.OrdinalIgnoreCase))
                return original.Contains(value)
                    ? 2
                    : 1;

            return 0;
        }

        static int Compare(ParameterInfo[] original, List<MethodRef.Parameter> value)
        {
            if (original == null && value == null)
                return 2;
            
            if (original == null || value == null)
                return 0;

            if (original.Length != value.Count)
                return 0;

            for (int i = 0; i < original.Length; i++)
            {
                var parameter = original[i];
                var methodRefParameter = value[i];

                if (parameter.Name != methodRefParameter.name)
                    return 1;

                if (parameter.ParameterType.Name != methodRefParameter.type)
                    return 1;
            }

            return 2;
        }

        public static class Error
        {

        }
    }
}