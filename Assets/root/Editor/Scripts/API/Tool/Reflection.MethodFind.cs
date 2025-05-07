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
            var methodEnumerable = FindMethods(
                filter: filter,
                knownNamespace: knownNamespace,
                classNameMatchLevel: classNameMatchLevel,
                methodNameMatchLevel: methodNameMatchLevel,
                parametersMatchLevel: parametersMatchLevel);

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