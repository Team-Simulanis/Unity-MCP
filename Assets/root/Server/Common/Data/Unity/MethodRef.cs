#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace com.IvanMurzak.Unity.MCP.Common.Data.Unity
{
    [Description(@"Method reference. Used to find method in codebase of the project.
'Namespace' (string) - namespace of the class. It may be empty if the class is in the global namespace.
'ClassName' (string) - class name.
'MethodName' (string) - method name.")]
    public class MethodRef
    {
        public string? Namespace { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public List<Parameter>? Parameters { get; set; }

        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                if (string.IsNullOrEmpty(ClassName))
                    return false;
                if (string.IsNullOrEmpty(MethodName))
                    return false;

                if (Parameters != null && Parameters.Count > 0)
                {
                    foreach (var parameter in Parameters)
                    {
                        if (parameter == null)
                            return false;
                        if (string.IsNullOrEmpty(parameter.type))
                            return false;
                        if (string.IsNullOrEmpty(parameter.name))
                            return false;
                    }
                }
                return true;
            }
        }

        public MethodRef() { }
        public MethodRef(MethodInfo methodInfo)
        {
            Namespace = methodInfo.DeclaringType?.Namespace;
            ClassName = methodInfo.DeclaringType?.Name ?? string.Empty;
            MethodName = methodInfo.Name;
            Parameters = methodInfo.GetParameters()
                ?.Select(parameter => new Parameter
                {
                    type = parameter.ParameterType.FullName,
                    name = parameter.Name
                })
                ?.ToList();
        }

        public override string ToString() => Parameters == null
            ? string.IsNullOrEmpty(Namespace)
                ? $"MethodRef: {ClassName}.{MethodName}()"
                : $"MethodRef: {Namespace}.{ClassName}.{MethodName}()"
            : string.IsNullOrEmpty(Namespace)
                ? $"MethodRef: {ClassName}.{MethodName}({string.Join(", ", Parameters)})"
                : $"MethodRef: {Namespace}.{ClassName}.{MethodName}({string.Join(", ", Parameters)})";

        public class Parameter
        {
            public string? type;
            public string? name;

            public Parameter() { }
            public Parameter(string type, string? name)
            {
                this.type = type;
                this.name = name;
            }
            public override string ToString()
            {
                return $"{type} {name}";
            }
        }
    }
}