using System;
using System.Threading;
using System.Threading.Tasks;
using Best.SignalR;

namespace AOT.Network
{
    /// <summary>
    /// SignalR连接处理器, 实现连接和重连机制
    /// </summary>
    public class SignalRConnectionHandler
    {
        event Func<HubConnection> NewHubConnection;
        event Action<HubConnection> CloseHub;
        event Action<string, Exception> DebugLog;
        public int MaxRetry = 2;
        public int WaitingSecs = 3;

        public SignalRConnectionHandler(
            Func<HubConnection> newHubConnectionFunc, 
            Action<HubConnection> onCloseHubAction, 
            Action<string, Exception> onDebugAction,
            int maxRetry = 2,
            int waitingSecs = 3)
        {
            MaxRetry = maxRetry;
            WaitingSecs = waitingSecs;
            NewHubConnection += newHubConnectionFunc;
            CloseHub += onCloseHubAction;
            DebugLog += onDebugAction;
        }

        private SemaphoreSlim _connectLock = new(1, 1);

        // 这是一个内置的连接状态，用于判断是否已经连接。
        public bool IsConnected => _conn is { State: ConnectionStates.Connected };
        public bool IsAwait => State != null && IsWaitingState(State.Value);
        private HubConnection _conn;
        public ConnectionStates? State => _conn?.State;

        public async ValueTask<bool> ConnectAsync()
        {
            await AwaitConnectionWithCancellationToken();
            return IsConnected;
        }

        public async ValueTask DisconnectAsync() => await _conn.CloseAsync();

        public async Task<T> InvokeTask<T>(string method,params object[] args)
        {
            await AwaitConnectionWithCancellationToken();
            if (!IsConnected) return default;
            var result = await _conn.InvokeAsync<T>(method, args);
            return result;
        }

        private async ValueTask AwaitConnectionWithCancellationToken()
        {
            if (IsConnected) return; // 如果已经连接，则直接返回。
            await _connectLock.WaitAsync(); // 请求访问。

            try
            {
                if (_conn == null) // 如果连接为空，则创建连接。
                {
                    _conn = NewHubConnection();
                    await _conn.ConnectAsync();
                    if (IsConnected) return; // 检查是否已经连接。
                }

                if (IsWaitingState(_conn.State)) // 如果是等待状态，则等待。
                {
                    // 创建一个超时的 CancellationTokenSource
                    using var cts = new CancellationTokenSource(WaitingSecs * 1000);
                    try
                    {
                        await Task.Delay(WaitingSecs * 1000, cts.Token); // 使用超时
                    }
                    catch (TaskCanceledException)
                    {
                        // 处理超时逻辑，例如记录日志
                        DebugLog?.Invoke("Connection attempt timed out.", null);
                        return;
                    }

                    if (IsConnected) return; // 再次检查是否已经连接。
                }

                // 重连机制
                var retries = 0;
                while (retries < MaxRetry)
                {
                    retries++;
                    try
                    {
                        // 重连机制
                        await Reconnection();
                        if (IsConnected) return; // 连接成功，退出。
                    }
                    catch (Exception e)
                    {
                        DebugLog?.Invoke("Reconnect failed.", e);
                    }
                }
            }
            finally
            {
                _connectLock.Release(); // 释放锁。
            }

            return;

            async Task Reconnection()
            {
                CloseHub?.Invoke(_conn); // 关闭连接。
                _conn = NewHubConnection(); // 创建新连接。
                await _conn.ConnectAsync();
            }
        }

        private async ValueTask AwaitConnection()
        {
            if (IsConnected) return; // 如果已经连接，则直接返回。
            await _connectLock.WaitAsync(); // 请求访问。
            try
            {
                if (_conn == null) // 如果连接为空，则创建连接。
                {
                    _conn = NewHubConnection();
                    await _conn.ConnectAsync();
                    if (IsConnected) return; // 检查是否已经连接。
                }

                if (IsWaitingState(_conn.State)) // 如果是等待状态，则等待。
                {
                    await Task.Delay(WaitingSecs * 1000); // 等待状态的timeout
                    if (IsConnected) return; // 再次检查是否已经连接。
                }


                // 重连机制
                var retries = 0;
                while (retries < MaxRetry)
                {
                    retries++;
                    try
                    {
                        // 重连机制
                        await Reconnection();
                        if (IsConnected) return; // 连接成功，退出。
                    }
                    catch (Exception e)
                    {
                        DebugLog?.Invoke("Reconnect failed.", e);
                    }
                }
            }
            finally
            {
                _connectLock.Release(); // 释放锁。
            }

            return;

            async Task Reconnection()
            {
                CloseHub?.Invoke(_conn); // 关闭连接。
                _conn = NewHubConnection(); // 创建新连接。
                await _conn.ConnectAsync();
            }

        }

        bool IsWaitingState(ConnectionStates state)
        {
            switch (state)
            {
                case ConnectionStates.Authenticating:
                case ConnectionStates.Negotiating:
                case ConnectionStates.Redirected:
                case ConnectionStates.Reconnecting:
                    return true;
                case ConnectionStates.Initial:
                case ConnectionStates.Connected:
                case ConnectionStates.CloseInitiated:
                case ConnectionStates.Closed:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}