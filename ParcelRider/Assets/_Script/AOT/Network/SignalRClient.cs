using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AOT.Core;
using AOT.Utl;
using Best.SignalR;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AOT.Network
{
    public interface ISignalRClient
    {
        SignalRClient.States State { get; }
        SignalRCaller Caller { get; }
        void Connect(Action<bool> connectAction = null);
        void Disconnect();
        Task<string> InvokeAsync(string methodName, params object[] args);
    }

    /// <summary>
    /// 客户端实例, 提供唯一状态<see cref="States"/>, Unity机制里实现app切换的时候自动重连,但不能保证重连成功
    /// </summary>
    public class SignalRClient : MonoBehaviour, ISignalRClient
    {
        public enum States
        {
            Disconnected,
            Connected,
            Await,
        }
        private SignalRConnection? _connection;

        public States State => _connection switch
        {
            null or { State: ConnectionStates.CloseInitiated or ConnectionStates.Closed or ConnectionStates.Initial } =>
                States.Disconnected,
            { State: ConnectionStates.Connected } => States.Connected,
            _ => States.Await
        };
        [SerializeField] private Image _blockingPanel;
        [SerializeField] private string _serverUrl;
        [SerializeField] private float debounceTime = 1.0f; // 防抖动时间（秒）
        private DateTime lastConnectionAttempt = DateTime.MinValue;
        private SignalRCaller _caller;
        private bool IsDebouncing() => (DateTime.UtcNow - lastConnectionAttempt).TotalSeconds < debounceTime;
        private void UpdateDebounce() => lastConnectionAttempt = DateTime.UtcNow;

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

        //抖动时间内不允许重复连接
        public void Init()
        {
            _caller = new SignalRCaller(this);
        }

        public void Connect(Action<bool> connectAction) => ConnectWithDebouncing(connectAction);

        private async void ConnectWithDebouncing(Action<bool> connectAction =null)
        {
            if (IsDebouncing()) return;
            if (_connection is { State: not (ConnectionStates.Closed 
                    or ConnectionStates.Initial
                    or ConnectionStates.CloseInitiated) })
                return;
            var isConnected = await ConnectAsync();
            connectAction?.Invoke(isConnected);
            UpdateDebounce();
        }

        public async Task<bool> ConnectAsync()
        {
            Blocking(true);
            _connection = new SignalRConnection(_serverUrl, OnDisconnectOrError);
            _connection.OnServerCall += ServerCall;
            var isConnected = await _connection.ConnectAsync();
            Debug.Log(isConnected ? "Connected to server!" : "Connection error!");
            Blocking(false);
            return isConnected;

            void OnDisconnectOrError(SignalRConnection conn)
            {
                if (_connection != conn) return;
                conn.OnServerCall -= ServerCall;
                Debug.Log("Remove ServerCall!");
                _connection = null;
            }
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
            if (_connection is
                {
                    State: not (ConnectionStates.Closed
                    or ConnectionStates.CloseInitiated)
                })
            {
                await _connection.DisconnectAsync();
            }
            UpdateDebounce();
        }

        public async Task<string> InvokeAsync(string methodName, params object[] args)
        {
            if (_connection is not
                {
                    State: not (ConnectionStates.Closed
                    or ConnectionStates.CloseInitiated)
                }) return null;
            var bag = DataBag.SerializeWithName(methodName, args);
            try
            {
                return await _connection.InvokeAsync("SignalRCall", bag);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return null;
            }
        }

#if !UNITY_EDITOR
        void OnApplicationFocus(bool focus)
        {
            if (!Application.isPlaying)return;
            if (!App.IsLoggedIn) return;
            switch (focus)
            {
                case true when State == States.Disconnected:
                {
                    Connect(null);
                    break;
                }
                case false when State == States.Connected:
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