using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace com.IvanMurzak.Unity.MCP.Server
{
    public class HubTools : BaseHub<HubTools>, IToolRunner
    {
        private static readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingRequests = new();

        readonly IMcpRunner _localApp;
        readonly ILocalServer? _localServer;
        readonly IRemoteServer? _remoteServer;

        public HubTools(ILogger<HubTools> logger, IHubContext<HubTools> hubContext, IMcpRunner localApp, IRemoteServer? remoteServer = null, ILocalServer? localServer = null)
            : base(logger, hubContext)
        {
            _localApp = localApp ?? throw new ArgumentNullException(nameof(localApp));
            _remoteServer = remoteServer;
            _localServer = localServer;
            if (localServer == null && remoteServer == null)
                throw new ArgumentNullException(nameof(remoteServer), "Either local or remote server must be set.");
        }

        public async Task<string> RequestClientData(string clientId, string payload)
        {
            var requestId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingRequests[requestId] = tcs;

            await Clients.Client(clientId).SendAsync("HandleRpcCommand", requestId, payload);

            var result = await tcs.Task; // await client response
            _pendingRequests.TryRemove(requestId, out _);
            return result;
        }

        public Task<IResponseData<ResponseCallTool>> RunCallTool(IRequestCallTool data, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IResponseData<ResponseListTool[]>> RunListTool(IRequestListTool data, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        // Client calls this to respond
        public Task SendRpcResponse(string requestId, string result)
        {
            if (_pendingRequests.TryGetValue(requestId, out var tcs))
            {
                tcs.SetResult(result);
            }

            return Task.CompletedTask;
        }
    }
}