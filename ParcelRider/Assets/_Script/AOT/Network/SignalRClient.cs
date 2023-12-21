using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Best.SignalR;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AOT.Network
{
    public class SignalRClient : MonoBehaviour
    {
        private SignalRConnection? _connection;
        [SerializeField] private Image _blockingPanel;
        [SerializeField] private string _serverUrl;
        [SerializeField] private float debounceTime = 1.0f; // 防抖动时间（秒）
        private DateTime lastConnectionAttempt = DateTime.MinValue;
        public event Action<string> OnServerCall;
        //抖动时间内不允许重复连接
        private bool IsDebouncing() => (DateTime.UtcNow - lastConnectionAttempt).TotalSeconds < debounceTime;
        private void UpdateDebounce() => lastConnectionAttempt = DateTime.UtcNow;

        public async void Connect(Action<bool> connectAction)
        {
            if (IsDebouncing()) return;
            Blocking(true);
            if (_connection is { State: not (ConnectionStates.Closed 
                    or ConnectionStates.Initial
                    or ConnectionStates.CloseInitiated) })
                return;
            _connection = new SignalRConnection(_serverUrl,
                () => connectAction(true),
                OnDisconnectOrError);
            _connection.OnServerCall += ServerCall;
            var isConnected = await _connection.ConnectAsync();
            if (isConnected) Debug.Log("Connected to server!");
            else Debug.Log("Connection error!");
            UpdateDebounce();
            Blocking(false);
            return;

            void OnDisconnectOrError(SignalRConnection conn)
            {
                if (_connection != conn) return;
                conn.OnServerCall -= ServerCall;
                Debug.Log("Remove ServerCall!");
                connectAction?.Invoke(false);
                _connection = null;
            }
        }

        private void ServerCall(string message) => OnServerCall?.Invoke(message);

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
            if (!App.IsLoggedIn) return;
            switch (focus)
            {
                case true when _connection == null:
                {
                    Connect();
                    break;
                }
                case false when _connection != null:
                {
                    Disconnect();
                    break;
                }
            }
        }
#endif
    }

    public class SignalRCaller
    {
        private SignalRClient _signalRClient;
        private RetryCaller _retryCaller;

        public SignalRCaller(SignalRClient signalRClient)
        {
            _signalRClient = signalRClient;
            _retryCaller = new RetryCaller();
        }

        public async Task Req_Do_Version(long[] orderIds,
            Action<DataBag> successAction,
            Func<string, Task<bool>> onErrRetry) =>
            await InvokeAsync(SignalREvents.Req_Do_Vers, successAction, onErrRetry, orderIds);

        private async Task InvokeAsync(string method,
            Action<DataBag> successAction, 
            Func<string,Task<bool>> onErrRetry, 
            params object[] args)
        {
            await _retryCaller.RetryAwait(call: 
                () => _signalRClient.InvokeAsync(method, args),
                successAction: result =>
                {
                    var bag = DataBag.Deserialize(result);
                    successAction(bag);
                },
                onErrRetry);
        }
    }
}