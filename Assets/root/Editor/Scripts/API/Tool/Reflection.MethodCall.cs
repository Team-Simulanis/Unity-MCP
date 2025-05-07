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
using com.IvanMurzak.Unity.MCP.Utils;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Reflection
    {
        [McpPluginTool
        (
            "Reflection_MethodCall",
            Title = "Find method using reflection"
        )]
        [Description("Find method in the project using C# Reflection.")]
        public string MethodCall
        (
            [Description("Filter.")]
            MethodRef filter,
            bool knownNamespace = false,
            int classNameMatchLevel = 1,
            int methodNameMatchLevel = 1,
            int parametersMatchLevel = 10,

            SerializedMember? targetObject = null,
            List<SerializedMember>? parameters = null,
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
{JsonUtils.Serialize(methods.Select(method => new MethodRef(method)))}
```";

            var method = methods.First();

            Func<string> action = () =>
            {
                var inputParameters = parameters
                    .Select(p => (p.name, Serializer.Deserialize(p)))
                    .ToImmutableDictionary(
                        keySelector: kvp => kvp.Item1,
                        elementSelector: kvp => kvp.Item2);
                        
                var methodWrapper = default(MethodWrapper);

                if (string.IsNullOrEmpty(targetObject?.type))
                {
                    // No object instance needed. Probably static method.
                    methodWrapper = new MethodWrapper(logger: null, method.DeclaringType, method);
                } 
                else
                {
                    // Object instance needed. Probably instance method.
                    var obj = Serializer.Deserialize(targetObject);
                    if (obj == null)
                        return $"[Error] '{nameof(targetObject)}' deserialized instance is null. Please specify the '{nameof(targetObject)}' properly.";
                    
                    methodWrapper = new MethodWrapper(logger: null, targetInstance: obj, method);
                }

                var result = methodWrapper.Invoke(inputParameters);                

                return $"[Success] Execution result:\n```json\n{JsonUtils.Serialize(result)}\n```";
            };

            if (executeInMainThread)
                return MainThread.Run(action);
            
            return action();
        }
    }
}