#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using R3;

namespace com.IvanMurzak.Unity.MCP.Server
{
    public class BaseHub<T> : Hub, IDisposable where T : Hub
    {
        // Thread-safe collection to store connected clients, grouped by hub type
        protected static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, bool>> ConnectedClients = new();

        protected readonly ILogger _logger;
        protected readonly IHubContext<T> _hubContext;
        protected readonly CompositeDisposable _disposables = new();

        protected BaseHub(ILogger logger, IHubContext<T> hubContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogTrace("Ctor.");
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        public override Task OnConnectedAsync()
        {
            var clients = ConnectedClients.GetOrAdd(GetType(), _ => new());
            if (!clients.TryAdd(Context.ConnectionId, true))
                _logger.LogWarning($"Client {Context.ConnectionId} is already connected to {GetType().Name}.");

            _logger.LogInformation($"Client connected: '{Context.ConnectionId}', Total connected clients for {GetType().Name}: {clients.Count}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (!ConnectedClients.TryGetValue(GetType(), out var clients))
            {
                _logger.LogWarning($"No connected clients found for {GetType().Name}.");
                return base.OnDisconnectedAsync(exception);
            }
            if (clients.TryRemove(Context.ConnectionId, out _))
            {
                _logger.LogInformation($"Client disconnected: '{Context.ConnectionId}', Total connected clients for {GetType().Name}: {clients.Count}");
            }
            else
            {
                _logger.LogWarning($"Client '{Context.ConnectionId}' was not found in connected clients for {GetType().Name}.");
            }

            return base.OnDisconnectedAsync(exception);
        }

        public void RemoveCurrentClient(ISingleClientProxy client)
        {
            if (!ConnectedClients.TryGetValue(GetType(), out var clients) || clients.IsEmpty)
            {
                _logger.LogWarning($"No connected clients found for {GetType().Name}.");
                return;
            }

            var connectionId = Context?.ConnectionId;
            if (connectionId == null)
                connectionId = clients.Last().Key;

            if (clients.TryRemove(connectionId, out _))
            {
                _logger.LogInformation($"Client '{connectionId}' removed from connected clients for {GetType().Name}.");
                Context?.Abort();
            }
            else
            {
                _logger.LogWarning($"Client '{connectionId}' was not found in connected clients for {GetType().Name}.");
            }
        }

        protected ISingleClientProxy? GetActiveClient()
        {
            if (!ConnectedClients.TryGetValue(GetType(), out var clients) || clients.IsEmpty)
            {
                _logger.LogWarning($"No connected clients of type {GetType().Name}.");
                return null;
            }

            var connectionId = clients.Keys.LastOrDefault();
            if (connectionId == null)
                return null;

            var client = _hubContext.Clients.Client(connectionId);
            if (client == null)
            {
                _logger.LogDebug($"Client {connectionId} is not available. Removing from connected clients.");
                clients.TryRemove(connectionId, out _);
                return GetActiveClient();
            }

            _logger.LogTrace($"Client {connectionId} is available.");
            return client;
        }
        public new void Dispose()
        {
            base.Dispose();
            _disposables.Dispose();

            if (ConnectedClients.TryRemove(GetType(), out var clients))
                clients.Clear();
        }
    }
}