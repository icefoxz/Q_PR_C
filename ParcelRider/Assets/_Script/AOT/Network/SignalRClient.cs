using System;
using System.Threading.Tasks;
using AOT.Core;
using AOT.Utl;
using OrderHelperLib.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AOT.Network
{
    public interface ISignalRClient
    {
        SignalRConnectionHandler.States State { get; }
        SignalRCaller Caller { get; }
        void Connect();
    }

    /// <summary>
    /// 客户端实例, 提供唯一状态<see cref="SignalRConnectionHandler.States"/>, Unity机制里实现app切换的时候自动重连,但不能保证重连成功
    /// </summary>
    public class SignalRClient : MonoBehaviour, ISignalRClient
    {
        private SignalRConnection? _connection;

        public SignalRConnectionHandler.States State=> _connection?.Status ?? SignalRConnectionHandler.States.Disconnected;

        [SerializeField] private Image _blockingPanel;
        [SerializeField] private string _serverUrl;
        [SerializeField] private float debounceTime = 1.0f; // 防抖动时间（秒）
        private DateTime lastConnectionAttempt = DateTime.MinValue;
        private SignalRCaller _caller;
        private bool IsDebouncing() => (DateTime.UtcNow - lastConnectionAttempt).TotalSeconds < debounceTime;
        private void UpdateDebounce() => lastConnectionAttempt = DateTime.UtcNow;
        public string ServerUrl => _serverUrl;

        public SignalRCaller Caller
        {
            get
            {
                if (_caller == null)
                {
                    throw new Exception("Caller is null! Please Init before call!");
                }
                return _caller;
            }
        }

        /// <summary>
        /// This is for testing purpose, use <see cref="Caller"/>.OnServerCall instead.<br/>
        /// This event will broadcast in <see cref="App.MessagingManager"/> through <see cref="SignalREvents"/><br/>
        /// </summary>
        public event Action<string> OnServerCall;

        public void Init()
        {
            _caller = new SignalRCaller(this);
            _connection = new SignalRConnection(this);
            _connection.OnServerCall += ServerCall;
            Blocking(false);
        }

        public void Connect(Action<bool> connectAction) => ConnectWithDebouncing(connectAction);

        //抖动时间内不允许重复连接
        private async void ConnectWithDebouncing(Action<bool> connectAction =null)
        {
            if (IsDebouncing()) return;
            if (_connection is { Status: SignalRConnectionHandler.States.Await }) return;
            var isConnected = await ConnectAsync();
            connectAction?.Invoke(isConnected);
            UpdateDebounce();
        }

        public async Task<bool> ConnectAsync()
        {
            Blocking(true);
            var isConnected = await _connection.ConnectAsync();
            Debug.Log(isConnected ? "Connected to server!" : "Connection error!");
            Blocking(false);
            return isConnected;

        }

        private void ServerCall(string data)
        {
            OnServerCall?.Invoke(data);
            Caller.OnServerCall(data);
        }

        private void Blocking(bool display) => _blockingPanel.gameObject.SetActive(display);

        public async void Disconnect()
        {
            if (IsDebouncing()) return;
            if (_connection is { Status: not SignalRConnectionHandler.States.Disconnected }) 
                await _connection.DisconnectAsync();
            UpdateDebounce();
        }

        public async Task<string> InvokeAsync(string methodName, params object[] args)
        {
            try
            {
                var bag = DataBag.SerializeWithName(methodName, args);
                return await _connection.InvokeAsync("SignalRCall", bag);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return null;
            }
        }

        public void Connect() => ConnectWithDebouncing();

#if UNITY_EDITOR
        void OnApplicationFocus(bool focus)
        {
            if (!Application.isPlaying)return;
            if (!App.IsLoggedIn) return;
            if (App.IsTestMode) return;
            switch (focus)
            {
                case true when State == SignalRConnectionHandler.States.Disconnected:
                {
                    Connect(null);
                    break;
                }
                case false when State == SignalRConnectionHandler.States.Connected:
                {
                    Disconnect();
                    break;
                }
            }
        }
#endif
        public void SetServerUrl(string url) => _serverUrl = url;
    }
}