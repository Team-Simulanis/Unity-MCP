#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace com.IvanMurzak.Unity.MCP.Common
{
    public class McpPluginBuilder : IMcpPluginBuilder
    {
        readonly IServiceCollection _services;
        readonly IDictionary<string, IRunTool> _tools = new Dictionary<string, IRunTool>();
        readonly IDictionary<string, IRunResource> _resources = new Dictionary<string, IRunResource>();

        public IServiceCollection Services => _services;
        public ServiceProvider? ServiceProvider { get; private set; }

        public McpPluginBuilder(IServiceCollection? services = null)
        {
            _services = services ?? new ServiceCollection();

            _services.AddTransient<IConnectionManager, ConnectionManager>();
            _services.AddSingleton<IMcpPlugin, McpPlugin>();

            _services.AddSingleton(_tools);
            _services.AddSingleton(_resources);

            Func<string, Task<HubConnection>> hubConnectionBuilder = (string endpoint) =>
            {
                var connectionConfig = ServiceProvider!.GetRequiredService<IOptions<ConnectionConfig>>().Value;
                var hubConnection = new HubConnectionBuilder()
                    .WithUrl(connectionConfig.Endpoint + endpoint)
                    .WithAutomaticReconnect(new FixedRetryPolicy(TimeSpan.FromSeconds(1)))
                    .WithServerTimeout(TimeSpan.FromSeconds(3))
                    .AddJsonProtocol(options =>
                    {
                        // options.PayloadSerializerOptions.PropertyNamingPolicy = null;
                        // options.PayloadSerializerOptions.DictionaryKeyPolicy = null;
                    })
                    .ConfigureLogging(logging =>
                    {
                        // logging.AddNLog();
                        logging.SetMinimumLevel(LogLevel.Trace);
                    })
                    .Build();

                return Task.FromResult(hubConnection);
            };
            _services.AddSingleton(hubConnectionBuilder);
        }

        public IMcpPluginBuilder AddTool(string name, IRunTool runner)
        {
            if (_tools.ContainsKey(name))
                throw new ArgumentException($"Tool with name '{name}' already exists.");

            _tools.Add(name, runner);
            return this;
        }

        public IMcpPluginBuilder AddResource(IRunResource resourceParams)
        {
            if (_resources == null)
                throw new ArgumentNullException(nameof(_resources));
            if (resourceParams == null)
                throw new ArgumentNullException(nameof(resourceParams));

            if (_resources.ContainsKey(resourceParams.Route))
                throw new ArgumentException($"Resource with routing '{resourceParams.Route}' already exists.");

            _resources.Add(resourceParams.Route, resourceParams);
            return this;
        }

        public IMcpPluginBuilder AddLogging(Action<ILoggingBuilder> loggingBuilder)
        {
            _services.AddLogging(loggingBuilder);
            return this;
        }

        public IMcpPluginBuilder WithConfig(Action<ConnectionConfig> config)
        {
            _services.Configure(config);
            return this;
        }

        public IMcpPlugin Build()
        {
            ServiceProvider = _services.BuildServiceProvider();
            return ServiceProvider.GetRequiredService<IMcpPlugin>();
        }
    }
}