#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using com.IvanMurzak.Unity.MCP.Common.Data;
using Microsoft.Extensions.Logging;

namespace com.IvanMurzak.Unity.MCP.Common
{
    public class McpRunner : IMcpRunner
    {
        protected readonly ILogger<McpRunner> _logger;
        readonly IDictionary<string, IRunTool> _tools;
        readonly IDictionary<string, IRunResource> _resources;

        public McpRunner(ILogger<McpRunner> logger, IDictionary<string, IRunTool> tools, IDictionary<string, IRunResource> resources)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogTrace("Ctor.");
            _tools = tools ?? throw new ArgumentNullException(nameof(tools));
            _resources = resources ?? throw new ArgumentNullException(nameof(resources));

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Registered tools [{0}]:", tools.Count);
                foreach (var kvp in tools)
                    _logger.LogTrace("Tool: {0}", kvp.Key);
            }

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Registered resources [{0}]:", resources.Count);
                foreach (var kvp in resources)
                    _logger.LogTrace("Resource: {0}", kvp.Key);
            }
        }

        public bool HasTool(string name) => _tools.ContainsKey(name);
        public bool HasResource(string name) => _resources.ContainsKey(name);

        public async Task<IResponseData<ResponseCallTool>> RunCallTool(IRequestCallTool data, CancellationToken cancellationToken = default)
        {
            var name = data?.Name ?? string.Empty;
            var requestId = data?.RequestID ?? Consts.Guid.Zero;

            try
            {
                if (data == null)
                    throw new ArgumentNullException(nameof(data));

                var toolKey = name;

                if (!_tools.TryGetValue(toolKey, out var toolRunner))
                {
                    _logger.LogWarning("Tool not found: {0}", name);
                    return ResponseData<ResponseCallTool>.Error(requestId, $"Tool not found: {name}");
                }

                var result = await toolRunner.Run(data.Arguments);
                if (result == null)
                    return ResponseData<ResponseCallTool>.Error(requestId, $"Error running tool '{name}': returned null result.");

                return ResponseData<ResponseCallTool>.Success(requestId, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running tool '{0}': {1}\n{2}", name, ex.Message, ex.StackTrace);
                return ResponseData<ResponseCallTool>.Error(requestId, $"Error running tool '{name}': {ex.Message}");
            }
        }

        public Task<IResponseData<ResponseListTool[]>> RunListTool(IRequestListTool data, CancellationToken cancellationToken = default)
        {
            var requestId = data?.RequestID ?? Consts.Guid.Zero;

            try
            {
                var response = ResponseData<ResponseListTool[]>.Success(requestId,
                    _tools.Select(kvp => new ResponseListTool(kvp.Key, kvp.Value.Title, kvp.Value.Description, kvp.Value.InputSchema)).ToArray());
                return Task.FromResult((IResponseData<ResponseListTool[]>)response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting list of tools: {0}", ex.Message);
                return Task.FromResult((IResponseData<ResponseListTool[]>)ResponseData<ResponseListTool[]>.Error(requestId, $"Error getting list of tools: {ex.Message}"));
            }
        }

        public async Task<IResponseData<ResponseListResource[]>> RunListResources(IRequestListResources data, CancellationToken cancellationToken = default)
        {
            var requestId = data?.RequestID ?? Consts.Guid.Zero;

            try
            {
                var results = new List<ResponseListResource>();

                foreach (var (resourceType, resource) in _resources)
                {
                    if (!string.IsNullOrWhiteSpace(data?.Filter))
                    {
                        var uriFilter = data.Filter;
                        // Handle both glob and regex patterns
                        var isMatch = false;
                        if (uriFilter.Contains('*'))  // Glob pattern
                        {
                            // Convert glob to regex
                            // * matches any sequence of characters
                            // ? matches a single character
                            string regexPattern = "^" + Regex.Escape(uriFilter)
                                .Replace("\\*", ".*")
                                .Replace("\\?", ".") + "$";
                            isMatch = Regex.IsMatch(resourceType, regexPattern, RegexOptions.IgnoreCase);
                        }
                        else  // Direct comparison
                        {
                            isMatch = resourceType.Contains(uriFilter, StringComparison.OrdinalIgnoreCase);
                        }

                        if (!isMatch)
                            continue;
                    }

                    try
                    {
                        var content = await resource.RunResourceList();
                        // Convert the IList<string> to a comma-separated string for compatibility
                        string contentStr = string.Join(", ", content);
                        results.Add(new ResponseListResource(resourceType, contentStr));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error getting resource type '{0}': {1}", resourceType, ex.Message);
                    }
                }

                return ResponseData<ResponseListResource[]>.Success(requestId, results.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting list of resources: {0}", ex.Message);
                return ResponseData<ResponseListResource[]>.Error(requestId, $"Error getting list of resources: {ex.Message}");
            }
        }

        public async Task<IResponseData<ResponseResourceContent[]>> RunResourceContent(IRequestResourceContent data, CancellationToken cancellationToken = default)
        {
            var requestId = data?.RequestID ?? Consts.Guid.Zero;
            var uri = data?.Uri ?? string.Empty;

            try
            {
                if (data == null)
                    throw new ArgumentNullException(nameof(data));

                if (string.IsNullOrWhiteSpace(uri))
                    throw new ArgumentException("Resource uri is empty or whitespace.", nameof(data.Uri));

                var parts = uri.Split(':', 2);
                if (parts.Length != 2)
                    throw new ArgumentException($"Invalid uri format: {uri}. Expected format: <resource-type>:<resource-id>");

                var resourceType = parts[0];
                var resourceId = parts[1];

                if (!_resources.TryGetValue(resourceType, out var resource))
                    throw new ArgumentException($"Resource type not found: {resourceType}");

                _logger.LogDebug("Loading resource {0}: {1}", resourceType, resourceId);

                var result = await resource.Run(resourceId);
                return ResponseData<ResponseResourceContent[]>.Success(requestId, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading resource '{0}': {1}", uri, ex.Message);
                return ResponseData<ResponseResourceContent[]>.Error(requestId, $"Error loading resource '{uri}': {ex.Message}");
            }
        }

        public async Task<IResponseData<ResponseResourceTemplate[]>> RunResourceTemplates(IRequestListResourceTemplates data, CancellationToken cancellationToken = default)
        {
            var requestId = data?.RequestID ?? Consts.Guid.Zero;

            try
            {
                var response = new List<ResponseResourceTemplate>();
                foreach (var (resourceType, resource) in _resources)
                {
                    try
                    {
                        var templates = await resource.RunResourceTemplates();
                        foreach (var template in templates)
                        {
                            if (!string.IsNullOrWhiteSpace(data?.Filter))
                            {
                                if (!template.Uri.Contains(data.Filter, StringComparison.OrdinalIgnoreCase))
                                    continue;
                            }
                            response.Add(template);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error getting resource templates for '{0}': {1}", resourceType, ex.Message);
                    }
                }

                return ResponseData<ResponseResourceTemplate[]>.Success(requestId, response.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resource templates: {0}", ex.Message);
                return ResponseData<ResponseResourceTemplate[]>.Error(requestId, $"Error getting resource templates: {ex.Message}");
            }
        }
        
        public Task<IResponseData<ResponseMenuItem[]>> RunListMenuItems(IRequestListMenuItems data, CancellationToken cancellationToken = default)
        {
            var requestId = data?.RequestID ?? Consts.Guid.Zero;

            try
            {
                // The actual menu item listing will be handled by the RpcRouter since it requires Unity editor classes
                // This just provides the interface method
                return Task.FromResult((IResponseData<ResponseMenuItem[]>)ResponseData<ResponseMenuItem[]>.Error(requestId, "Menu item listing requires Unity Editor"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting list of menu items: {0}", ex.Message);
                return Task.FromResult((IResponseData<ResponseMenuItem[]>)ResponseData<ResponseMenuItem[]>.Error(requestId, $"Error getting list of menu items: {ex.Message}"));
            }
        }

        public Task<IResponseData<ResponseExecuteMenuItem>> RunExecuteMenuItem(IRequestExecuteMenuItem data, CancellationToken cancellationToken = default)
        {
            var requestId = data?.RequestID ?? Consts.Guid.Zero;

            try
            {
                // The actual menu item execution will be handled by the RpcRouter since it requires Unity editor classes
                // This just provides the interface method
                return Task.FromResult((IResponseData<ResponseExecuteMenuItem>)ResponseData<ResponseExecuteMenuItem>.Error(requestId, "Menu item execution requires Unity Editor"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing menu item: {0}", ex.Message);
                return Task.FromResult((IResponseData<ResponseExecuteMenuItem>)ResponseData<ResponseExecuteMenuItem>.Error(requestId, $"Error executing menu item: {ex.Message}"));
            }
        }

        public void Dispose()
        {
            _resources.Clear();
            _tools.Clear();
        }
    }
}