using System;
using System.Threading;
using System.Threading.Tasks;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data;
using com.IvanMurzak.Unity.MCP.Common.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using R3;

namespace com.IvanMurzak.Unity.MCP.Server
{
    public class RemoteToolRunner : IToolRunner, IDisposable
    {
        readonly ILogger<RemoteToolRunner> _logger;
        readonly IHubContext<RemoteApp> _remoteAppContext;
        readonly CancellationTokenSource cts = new();
        readonly CompositeDisposable _disposables = new();

        public RemoteToolRunner(ILogger<RemoteToolRunner> logger, IHubContext<RemoteApp> remoteAppContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogTrace("{0} Ctor.", typeof(RemoteToolRunner).Name);
            _remoteAppContext = remoteAppContext ?? throw new ArgumentNullException(nameof(remoteAppContext));
        }

        public Task<IResponseData<ResponseCallTool>> RunCallTool(IRequestCallTool requestData, string? connectionId, CancellationToken cancellationToken = default)
            => ClientUtils.InvokeAsync<IRequestCallTool, ResponseCallTool, RemoteApp>(
                logger: _logger,
                hubContext: _remoteAppContext,
                methodName: com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunCallTool,
                connectionId: connectionId,
                requestData: requestData,
                cancellationToken: CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken).Token)
                .ContinueWith(task =>
            {
                var response = task.Result;
                if (response.IsError)
                    return ResponseData<ResponseCallTool>.Error(requestData.RequestID, response.Message ?? "[Error] Got an error during invoking tool");

                return response;
            }, cancellationToken: CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken).Token);

        public Task<IResponseData<ResponseListTool[]>> RunListTool(IRequestListTool requestData, string? connectionId, CancellationToken cancellationToken = default)
            => ClientUtils.InvokeAsync<IRequestListTool, ResponseListTool[], RemoteApp>(
                logger: _logger,
                hubContext: _remoteAppContext,
                methodName: com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunListTool,
                connectionId: connectionId,
                requestData: requestData,
                cancellationToken: CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken).Token)
                .ContinueWith(task =>
            {
                var response = task.Result;
                if (response.IsError)
                    return ResponseData<ResponseListTool[]>.Error(requestData.RequestID, response.Message ?? "[Error] Got an error during listing tools");

                return response;
            }, cancellationToken: CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken).Token);

        public Task<IResponseData<ResponseMenuItem[]>> RunListMenuItems(IRequestListMenuItems requestData, string? connectionId = null, CancellationToken cancellationToken = default)
            => ClientUtils.InvokeAsync<IRequestListMenuItems, ResponseMenuItem[], RemoteApp>(
                logger: _logger,
                hubContext: _remoteAppContext,
                methodName: com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunListMenuItems,
                connectionId: connectionId,
                requestData: requestData,
                cancellationToken: CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken).Token)
                .ContinueWith(task =>
            {
                var response = task.Result;
                if (response.IsError)
                    return ResponseData<ResponseMenuItem[]>.Error(requestData.RequestID, response.Message ?? "[Error] Got an error during listing menu items");

                return response;
            }, cancellationToken: CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken).Token);

        public Task<IResponseData<ResponseExecuteMenuItem>> RunExecuteMenuItem(IRequestExecuteMenuItem requestData, string? connectionId = null, CancellationToken cancellationToken = default)
            => ClientUtils.InvokeAsync<IRequestExecuteMenuItem, ResponseExecuteMenuItem, RemoteApp>(
                logger: _logger,
                hubContext: _remoteAppContext,
                methodName: com.IvanMurzak.Unity.MCP.Common.Utils.Consts.RPC.Client.RunExecuteMenuItem,
                connectionId: connectionId,
                requestData: requestData,
                cancellationToken: CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken).Token)
                .ContinueWith(task =>
            {
                var response = task.Result;
                if (response.IsError)
                    return ResponseData<ResponseExecuteMenuItem>.Error(requestData.RequestID, response.Message ?? "[Error] Got an error during executing menu item");

                return response;
            }, cancellationToken: CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken).Token);

        public void Dispose()
        {
            _logger.LogTrace("{0} Dispose.", typeof(RemoteToolRunner).Name);
            _disposables.Dispose();

            if (!cts.IsCancellationRequested)
                cts.Cancel();

            cts.Dispose();
        }
    }
}