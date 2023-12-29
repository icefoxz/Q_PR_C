using System;
using System.Reflection;
using System.Threading.Tasks;
using AOT.Utl;
using Best.SignalR;
using Best.SignalR.Encoders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OrderHelperLib;
using UnityEngine;

namespace AOT.Network
{
    //SignalR连接实例
    public class SignalRConnection
    {
        public const string ServerCall = "ServerCall";
        private HubConnection _hub;
        public ConnectionStates? State => _hub?.State;
        public event Action<string> OnServerCall;
        public SignalRConnection(string url,
            Action<SignalRConnection> onDisconnectOrError)
        {
            var uri = new Uri(url + "?access_token=" + ApiCaller.AccessToken);
            _hub = new HubConnection(uri, new JsonProtocol(new JsonDotNetEncoder(Json.Settings)));
            _hub.On<string>(ServerCall, msg => OnServerCall?.Invoke(msg));
            _hub.OnError += OnError;
            _hub.OnClosed += OnClosed;
            //_hub.OnConnected += OnConnected;
            return;

            //void OnConnected(HubConnection obj) => onConnected?.Invoke();

            void OnClosed(HubConnection obj)
            {
                RemoveEvents();
                onDisconnectOrError?.Invoke(this);
            }

            void RemoveEvents()
            {
                _hub.OnError -= OnError;
                _hub.OnClosed -= OnClosed;
            }

            void OnError(HubConnection hub, string message)
            {
#if UNITY_EDITOR
                Debug.Log(message);
#endif
                RemoveEvents();
                onDisconnectOrError?.Invoke(this);
            }
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                await _hub.ConnectAsync();
                return _hub.State == ConnectionStates.Connected;
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.LogWarning(e);
#endif
                return false;
            }
        }

        public void StartConnect() => _hub.StartConnect();
        public async Task DisconnectAsync() => await _hub.CloseAsync();
        public Task<string> InvokeAsync(string method, params object[] args) => _hub.InvokeAsync<string>(method, args);
    }
}
