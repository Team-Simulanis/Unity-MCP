#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data.Unity;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Reflection
    {
        [McpPluginTool
        (
            "Reflection_MethodFind",
            Title = "Find method using reflection"
        )]
        [Description("Find method in the project using C# Reflection.")]
        public string MethodFind
        (
            [Description("Filter.")]
            MethodRef filter,
            bool knownNamespace = false,
            int classNameMatchLevel = 1,
            int methodNameMatchLevel = 1,
            int parametersMatchLevel = 10
        )
        {
            // Prepare Namespace
            filter.Namespace = filter.Namespace.Trim().Replace("null", string.Empty);
            if (string.IsNullOrEmpty(filter.ClassName))
                filter.ClassName = null;

            var typesEnumerable = AllTypes;

            if (knownNamespace)
                typesEnumerable = typesEnumerable.Where(type => type.Namespace == filter.Namespace);

            if (classNameMatchLevel > 0 && !string.IsNullOrEmpty(filter.ClassName))
                typesEnumerable = typesEnumerable
                    .Select(type => new
                    {
                        Type = type,
                        MatchLevel = Compare(type.Name, filter.ClassName)
                    })
                    .Where(entry => entry.MatchLevel >= classNameMatchLevel)
                    .OrderByDescending(entry => entry.MatchLevel)
                    .Select(entry => entry.Type);

            var types = typesEnumerable.ToList();

            var methodEnumerable = types
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                .Where(method => method.DeclaringType != null && !method.DeclaringType.IsAbstract);

            if (methodNameMatchLevel > 0 && !string.IsNullOrEmpty(filter.MethodName))
                methodEnumerable = methodEnumerable
                    .Select(method => new
                    {
                        Method = method,
                        MatchLevel = Compare(method.Name, filter.MethodName)
                    })
                    .Where(entry => entry.MatchLevel >= methodNameMatchLevel)
                    .OrderByDescending(entry => entry.MatchLevel)
                    .Select(entry => entry.Method);

            if (parametersMatchLevel > 0)
                methodEnumerable = methodEnumerable
                    .Select(method => new
                    {
                        Method = method,
                        MatchLevel = Compare(method.GetParameters(), filter.Parameters)
                    })
                    .Where(entry => entry.MatchLevel >= parametersMatchLevel)
                    .OrderByDescending(entry => entry.MatchLevel)
                    .Select(entry => entry.Method);

            var methods = methodEnumerable.ToList();
            if (methods.Count == 0)
                return $"[Success] Method not found.\n{filter}";

            var methodRefs = methods
                .Select(method => new MethodRef(method))
                .ToList();

            return $@"[Success] Found {methods.Count} method(s):
```json
{JsonUtils.Serialize(methodRefs)}
```";
        }
    }
}