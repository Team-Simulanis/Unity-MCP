using com.IvanMurzak.Unity.MCP.Common.Data.Unity;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace com.IvanMurzak.Unity.MCP.Server.API
{
    public partial class Tool_Scene
    {
        [McpServerTool
        (
            Name = "Reflection_MethodCall",
            Title = "Call method using C# reflection"
        )]
        [Description(@"Call specific C# method using C# reflection. Receives input parameters and returns result.
To browse available method use 'Reflection_MethodFind'.")]
        public Task<CallToolResponse> MethodCall
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

            [Description(@"Specify target object to call method on. It could be null if the method is static or if the is no target object.
New instance of the object will be created if the method is instance method and the target object is null.")]
            SerializedMember? targetObject = null,

            [Description(@"Method input parameters.")]
            List<SerializedMember>? inputParameters = null,

            [Description(@"Set to true if the method should be executed in the main thread. Otherwise, set to false.")]
            bool executeInMainThread = true
        )
        {
            return ToolRouter.Call("Reflection_MethodCall", arguments =>
            {
                arguments[nameof(filter)] = filter;
                arguments[nameof(knownNamespace)] = knownNamespace;
                arguments[nameof(classNameMatchLevel)] = classNameMatchLevel;
                arguments[nameof(methodNameMatchLevel)] = methodNameMatchLevel;
                arguments[nameof(parametersMatchLevel)] = parametersMatchLevel;

                if (targetObject != null)
                    arguments[nameof(targetObject)] = targetObject;

                if (inputParameters != null)
                    arguments[nameof(inputParameters)] = inputParameters;

                arguments[nameof(executeInMainThread)] = executeInMainThread;
            });
        }
    }
}