#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data.Unity;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;
using com.IvanMurzak.Unity.MCP.Common.MCP;
using com.IvanMurzak.Unity.MCP.Common.Reflection;
using com.IvanMurzak.Unity.MCP.Utils;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Reflection
    {
        [McpPluginTool
        (
            "Reflection_MethodCall",
            Title = "Call method using C# reflection"
        )]
        [Description(@"Call C# method. It requires to receive proper method schema.
Use 'Reflection_MethodFind' to find available method before using it.
Receives input parameters and returns result.")]
        public string MethodCall
        (
            MethodPointerRef filter,

            [Description("Set to true if 'Namespace' is known and full namespace name is specified in the 'filter.Namespace' property. Otherwise, set to false.")]
            bool knownNamespace = false,

            [Description(@"Minimal match level for 'ClassName'.
0 - ignore 'filter.ClassName',
1 - contains ignoring case (default value),
2 - contains case sensitive,
3 - starts with ignoring case,
4 - starts with case sensitive,
5 - equals ignoring case,
6 - equals case sensitive.")]
            int classNameMatchLevel = 1,

            [Description(@"Minimal match level for 'MethodName'.
0 - ignore 'filter.MethodName',
1 - contains ignoring case (default value),
2 - contains case sensitive,
3 - starts with ignoring case,
4 - starts with case sensitive,
5 - equals ignoring case,
6 - equals case sensitive.")]
            int methodNameMatchLevel = 1,

            [Description(@"Minimal match level for 'Parameters'.
0 - ignore 'filter.Parameters',
1 - parameters count is the same,
2 - equals (default value).")]
            int parametersMatchLevel = 2,

            [Description(@"Specify target object to call method on. Should be null if the method is static or if the is no specific target instance.
New instance of the specified class will be created if the method is instance method and the targetObject is null.
Required:
- type - full type name of the object to call method on.
- value - serialized object value. It will be deserialized to the specified type.")]
            SerializedMember? targetObject = null,

            [Description(@"Method input parameters. Per each parameter specify:
- type - full type name of the object to call method on.
- name - parameter name.
- value - serialized object value. It will be deserialized to the specified type.")]
            List<SerializedMember>? inputParameters = null,

            [Description(@"Set to true if the method should be executed in the main thread. Otherwise, set to false.")]
            bool executeInMainThread = true
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
                return $"[Error] Method not found.\n{filter}";

            if (methods.Count > 1)
                return @$"[Error] Found more then one method. Only single method should be targeted. Please specify the method name more precisely.
Found {methods.Count} method(s):
```json
{JsonUtils.Serialize(methods.Select(method => new MethodDataRef(method)))}
```";

            var method = methods.First();

            Func<string> action = () =>
            {
                var convertors = Reflector.Instance.Convertors.GetAllSerializers();
                var dictInputParameters = inputParameters
                    ?.Select(p => (p.name, Reflector.Instance.Deserialize(p)))
                    ?.ToImmutableDictionary(
                        keySelector: kvp => kvp.Item1,
                        elementSelector: kvp => kvp.Item2);

                var methodWrapper = default(MethodWrapper);

                if (string.IsNullOrEmpty(targetObject?.type))
                {
                    // No object instance needed. Probably static method.
                    methodWrapper = new MethodWrapper(Reflector.Instance, logger: null, method.DeclaringType, method);
                }
                else
                {
                    // Object instance needed. Probably instance method.
                    var obj = Reflector.Instance.Deserialize(targetObject);
                    if (obj == null)
                        return $"[Error] '{nameof(targetObject)}' deserialized instance is null. Please specify the '{nameof(targetObject)}' properly.";

                    methodWrapper = new MethodWrapper(Reflector.Instance, logger: null, targetInstance: obj, method);
                }

                var result = dictInputParameters != null
                    ? methodWrapper.InvokeDict(dictInputParameters)
                    : methodWrapper.Invoke();

                return $"[Success] Execution result:\n```json\n{JsonUtils.Serialize(result)}\n```";
            };

            if (executeInMainThread)
                return MainThread.Run(action);

            return action();
        }
    }
}