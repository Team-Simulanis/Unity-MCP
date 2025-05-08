#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using System.Linq;
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
            MethodPointerRef filter,

            [Description("Set to true if 'Namespace' is known and full namespace name is specified in the 'filter.Namespace' property. Otherwise, set to false.")]
            bool knownNamespace = false,

            [Description(@"Minimal match level for 'ClassName'.
0 - ignore 'filter.ClassName',
1 - contains ignoring case,
2 - contains case sensitive,
3 - starts with ignoring case,
4 - starts with case sensitive,
5 - equals ignoring case,
6 - equals case sensitive.")]
            int classNameMatchLevel = 1,

            [Description(@"Minimal match level for 'MethodName'.
0 - ignore 'filter.MethodName',
1 - contains ignoring case,
2 - contains case sensitive,
3 - starts with ignoring case,
4 - starts with case sensitive,
5 - equals ignoring case,
6 - equals case sensitive.")]
            int methodNameMatchLevel = 1,

            [Description(@"Minimal match level for 'Parameters'.
0 - ignore 'filter.Parameters',
1 - parameters count is the same,
2 - equals.")]
            int parametersMatchLevel = 2
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
                .Select(method => new MethodDataRef(method))
                .ToList();

            return $@"[Success] Found {methods.Count} method(s):
```json
{JsonUtils.Serialize(methodRefs)}
```";
        }
    }
}