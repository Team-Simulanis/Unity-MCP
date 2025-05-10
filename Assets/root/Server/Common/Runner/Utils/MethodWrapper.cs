#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;
using com.IvanMurzak.Unity.MCP.Common.Reflection;
using Microsoft.Extensions.Logging;

namespace com.IvanMurzak.Unity.MCP.Common.MCP
{
    public class MethodWrapper
    {
        protected readonly Reflector _reflector;
        protected readonly MethodInfo _methodInfo;
        protected readonly object? _targetInstance;
        protected readonly Type? _classType;

        protected readonly string? _description;
        protected readonly ILogger? _logger;
        protected readonly JsonNode? _inputSchema;

        public JsonNode? InputSchema => _inputSchema;
        public string? Description => _description;

        public MethodWrapper(Reflector reflector, ILogger? logger, MethodInfo methodInfo)
        {
            _reflector = reflector ?? throw new ArgumentNullException(nameof(reflector));
            _logger = logger;
            _methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));

            if (!methodInfo.IsStatic)
                throw new ArgumentException("The provided method must be static.");

            _description = methodInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
            _inputSchema = JsonUtils.GetArgumentsSchema(methodInfo);
        }

        public MethodWrapper(Reflector reflector, ILogger? logger, object targetInstance, MethodInfo methodInfo)
        {
            _reflector = reflector ?? throw new ArgumentNullException(nameof(reflector));
            _logger = logger;
            _targetInstance = targetInstance ?? throw new ArgumentNullException(nameof(targetInstance));
            _methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));

            if (methodInfo.IsStatic)
                throw new ArgumentException("The provided method must be an instance method. Use the other constructor for static methods.");

            _description = methodInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
            _inputSchema = JsonUtils.GetArgumentsSchema(methodInfo);
        }

        public MethodWrapper(Reflector reflector, ILogger? logger, Type classType, MethodInfo methodInfo)
        {
            _reflector = reflector ?? throw new ArgumentNullException(nameof(reflector));
            _logger = logger;
            _classType = classType ?? throw new ArgumentNullException(nameof(classType));
            _methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));

            if (methodInfo.IsStatic)
                throw new ArgumentException("The provided method must be an instance method. Use the other constructor for static methods.");

            _description = methodInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
            _inputSchema = JsonUtils.GetArgumentsSchema(methodInfo);
        }

        public virtual async Task<object?> Invoke(params object?[] parameters)
        {
            // If _targetInstance is null and _targetType is set, create an instance of the target type
            var instance = _targetInstance ?? (_classType != null ? Activator.CreateInstance(_classType) : null);

            // Build the final parameters array, filling in default values where necessary
            var finalParameters = BuildParameters(_reflector, parameters);
            PrintParameters(finalParameters);

            // Invoke the method (static or instance)
            var result = _methodInfo.Invoke(instance, finalParameters);

            // Handle Task, Task<T>, or synchronous return types
            if (result is Task task)
            {
                await task.ConfigureAwait(continueOnCapturedContext: false);

                // If it's a Task<T>, extract the result
                var resultProperty = task.GetType().GetProperty("Result");
                return resultProperty?.GetValue(task);
            }

            // For synchronous methods, return the result directly
            return result;
        }

        public virtual async Task<object?> InvokeDict(IReadOnlyDictionary<string, object?>? namedParameters)
        {
            // If _targetInstance is null and _targetType is set, create an instance of the target type
            var instance = _targetInstance ?? (_classType != null ? Activator.CreateInstance(_classType) : null);

            // Build the final parameters array, filling in default values where necessary
            var finalParameters = BuildParameters(_reflector, namedParameters);
            PrintParameters(finalParameters);

            // Invoke the method (static or instance)
            var result = _methodInfo.Invoke(instance, finalParameters);

            // Handle Task, Task<T>, or synchronous return types
            if (result is Task task)
            {
                await task.ConfigureAwait(continueOnCapturedContext: false);

                // If it's a Task<T>, extract the result
                var resultProperty = task.GetType().GetProperty("Result");
                return resultProperty?.GetValue(task);
            }

            // For synchronous methods, return the result directly
            return result;
        }

        protected object?[]? BuildParameters(Reflector reflector, object?[]? parameters)
        {
            if (parameters == null)
                return null;

            var methodParameters = _methodInfo.GetParameters();

            // Prepare the final arguments array, filling in default values where necessary
            var finalParameters = new object?[methodParameters.Length];
            for (int i = 0; i < methodParameters.Length; i++)
            {
                if (i < parameters.Length)
                {
                    // Handle JsonElement conversion
                    if (parameters[i] is JsonElement jsonElement)
                    {
                        try
                        {
                            // Try #1: Parsing as the parameter type directly
                            finalParameters[i] = JsonUtils.Deserialize(jsonElement, methodParameters[i].ParameterType);
                        }
                        catch
                        {
                            // Try #2: Parsing as SerializedMember
                            var serializedParameter = JsonUtils.Deserialize<SerializedMember>(jsonElement);
                            if (serializedParameter == null)
                                throw new ArgumentException($"Failed to parse {nameof(SerializedMember)} for parameter '{methodParameters[i].Name}'");

                            finalParameters[i] = reflector.Deserialize(serializedParameter);
                        }
                    }
                    else
                    {
                        // Use the provided parameter value
                        finalParameters[i] = parameters[i];
                    }
                }
                else if (methodParameters[i].HasDefaultValue)
                {
                    // Use the default value if no value is provided
                    finalParameters[i] = methodParameters[i].DefaultValue;
                }
                else
                {
                    throw new ArgumentException($"No value provided for parameter '{methodParameters[i].Name}' and no default value is defined.");
                }
            }

            return finalParameters;
        }

        protected object?[]? BuildParameters(Reflector reflector, IReadOnlyDictionary<string, object?>? namedParameters)
        {
            if (namedParameters == null)
                return null;

            var methodParameters = _methodInfo.GetParameters();

            // Prepare the final arguments array
            var finalParameters = new object?[methodParameters.Length];
            for (int i = 0; i < methodParameters.Length; i++)
            {
                var parameter = methodParameters[i];

                if (namedParameters != null && namedParameters.TryGetValue(parameter.Name!, out var value))
                {
                    if (value is JsonElement jsonElement)
                    {
                        try
                        {
                            // Try #1: Parsing as the parameter type directly
                            finalParameters[i] = JsonUtils.Deserialize(jsonElement, methodParameters[i].ParameterType);
                        }
                        catch
                        {
                            // Try #2: Parsing as SerializedMember
                            var serializedParameter = JsonUtils.Deserialize<SerializedMember>(jsonElement);
                            if (serializedParameter == null)
                                throw new ArgumentException($"Failed to parse {nameof(SerializedMember)} for parameter '{methodParameters[i].Name}'");

                            finalParameters[i] = reflector.Deserialize(serializedParameter);
                        }
                    }
                    else
                    {
                        // Use the provided parameter value
                        finalParameters[i] = value;
                    }
                }
                else if (parameter.HasDefaultValue)
                {
                    // Use the default value if no value is provided
                    finalParameters[i] = parameter.DefaultValue;
                }
                else
                {
                    // Use the type's default value if no value is provided
                    finalParameters[i] = parameter.ParameterType.IsValueType
                        ? Activator.CreateInstance(parameter.ParameterType)
                        : null;
                }
            }

            return finalParameters;
        }
        void PrintParameters(object?[]? parameters)
        {
            if (!(_logger?.IsEnabled(LogLevel.Debug) ?? false))
                return;

            _logger?.LogDebug("Invoke method: {0} {1}, Class: {2}", _methodInfo.ReturnType.Name, _methodInfo.Name, _classType?.Name ?? "null");

            var methodParameters = _methodInfo.GetParameters();
            var maxLength = Math.Max(methodParameters.Length, parameters?.Length ?? 0);
            var result = new string[maxLength];

            for (var i = 0; i < maxLength; i++)
            {
                var parameterType = i < methodParameters.Length ? methodParameters[i].ParameterType.ToString() : "N/A";
                var parameterName = i < methodParameters.Length ? methodParameters[i].Name : "N/A";
                var parameterValue = i < (parameters?.Length ?? 0) ? parameters?[i]?.ToString() ?? "null" : "null";

                result[i] = $"{parameterType} {parameterName} = {parameterValue}";
            }

            var parameterLogs = string.Join(Environment.NewLine, result);
            _logger?.LogDebug("Invoke method: Parameters. Input: {0}, Provided: {1}\n{2}", methodParameters.Length, parameters?.Length, parameterLogs);
        }
    }
}