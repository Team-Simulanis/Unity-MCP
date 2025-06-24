#if !UNITY_5_3_OR_NEWER
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using com.IvanMurzak.Unity.MCP.Common;
using NLog.Extensions.Logging;
using NLog;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.ReflectorNet;

namespace com.IvanMurzak.Unity.MCP.Server
{
    using Consts = Common.Consts;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.Error.WriteLine("Location: " + Environment.CurrentDirectory);
            // Configure NLog
            var logger = LogManager.Setup().LoadConfigurationFromFile("NLog.config").GetCurrentClassLogger();
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Configure all logs to go to stderr. This is needed for MCP STDIO server to work properly.
                builder.Logging.AddConsole(consoleLogOptions => consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace);

                // Replace default logging with NLog
                // builder.Logging.ClearProviders();
                builder.Logging.AddNLog();

                builder.Services.AddSignalR(configure =>
                {
                    configure.EnableDetailedErrors = true;
                    configure.MaximumReceiveMessageSize = 1024 * 1024 * 256; // 256 MB
                    configure.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                    configure.KeepAliveInterval = TimeSpan.FromSeconds(1);
                    configure.HandshakeTimeout = TimeSpan.FromSeconds(5);
                    configure.JsonSerialize(JsonUtils.JsonSerializerOptions);
                });

                // Setup MCP server ---------------------------------------------------------------
                builder.Services
                    .AddMcpServer(options =>
                    {
                        options.Capabilities ??= new();
                        options.Capabilities.Tools ??= new();
                        options.Capabilities.Tools.ListChanged = true;
                    })
                    .WithStdioServerTransport()
                    //.WithPromptsFromAssembly()
                    .WithToolsFromAssembly()
                    .WithCallToolHandler(ToolRouter.Call)
                    .WithListToolsHandler(ToolRouter.ListAll);
                //.WithReadResourceHandler(ResourceRouter.ReadResource)
                //.WithListResourcesHandler(ResourceRouter.ListResources)
                //.WithListResourceTemplatesHandler(ResourceRouter.ListResourceTemplates);

                // Setup McpApp ----------------------------------------------------------------
                builder.Services.AddMcpPlugin(logger: null, configure =>
                {
                    configure
                        .WithServerFeatures()
                        .AddLogging(logging =>
                        {
                            logging.AddNLog();
                            logging.SetMinimumLevel(LogLevel.Debug);
                        });
                }).Build(new Reflector());

                // builder.WebHost.UseUrls(Consts.Hub.DefaultEndpoint);
                var (port, timeoutSeconds) = ParseArguments(args);
                
                // Set the runtime configurable timeout
                Consts.Hub.TimeoutSeconds = timeoutSeconds;
                
                builder.WebHost.UseKestrel(options =>
                {
                    options.ListenLocalhost(port);
                });

                var app = builder.Build();

                // Middleware ----------------------------------------------------------------
                // ---------------------------------------------------------------------------

                app.UseRouting();
                app.MapHub<RemoteApp>(Consts.Hub.RemoteApp, options =>
                {
                    options.Transports = HttpTransports.All;
                    options.ApplicationMaxBufferSize = 1024 * 1024 * 10; // 10 MB
                    options.TransportMaxBufferSize = 1024 * 1024 * 10; // 10 MB
                });

                if (logger.IsEnabled(NLog.LogLevel.Debug))
                {
                    var endpointDataSource = app.Services.GetRequiredService<Microsoft.AspNetCore.Routing.EndpointDataSource>();
                    foreach (var endpoint in endpointDataSource.Endpoints)
                        logger.Debug($"Configured endpoint: {endpoint.DisplayName}");

                    app.Use(async (context, next) =>
                    {
                        logger.Debug($"Request: {context.Request.Method} {context.Request.Path}");
                        await next.Invoke();
                        logger.Debug($"Response: {context.Response.StatusCode}");
                    });
                }

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Application stopped due to an exception.");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }
        static (int port, float timeoutSeconds) ParseArguments(string[] args)
        {
            var port = Consts.Hub.DefaultPort;
            var timeoutSeconds = Consts.Hub.TimeoutSeconds;

            // Parse command line arguments
            // Format: [port] [timeout] or --port=8090 --timeout=300
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                
                // Handle --port=value format
                if (arg.StartsWith("--port="))
                {
                    if (int.TryParse(arg.Substring(7), out var parsedPort))
                        port = parsedPort;
                }
                // Handle --timeout=value format (value in milliseconds)
                else if (arg.StartsWith("--timeout="))
                {
                    if (float.TryParse(arg.Substring(10), out var parsedTimeoutMs))
                        timeoutSeconds = parsedTimeoutMs / 1000f; // Convert milliseconds to seconds
                }
                // Handle positional arguments (backwards compatibility)
                else if (i == 0 && int.TryParse(arg, out var posPort))
                {
                    port = posPort;
                }
                else if (i == 1 && float.TryParse(arg, out var posTimeoutMs))
                {
                    timeoutSeconds = posTimeoutMs / 1000f; // Convert milliseconds to seconds
                }
            }

            // Check environment variables as fallback
            var envPort = Environment.GetEnvironmentVariable(Consts.Env.Port);
            if (envPort != null && int.TryParse(envPort, out var parsedEnvPort))
                port = parsedEnvPort;

            var envTimeout = Environment.GetEnvironmentVariable(Consts.Env.Timeout);
            if (envTimeout != null && float.TryParse(envTimeout, out var parsedEnvTimeoutMs))
                timeoutSeconds = parsedEnvTimeoutMs / 1000f; // Convert milliseconds to seconds

            return (port, timeoutSeconds);
        }
    }
}
#endif