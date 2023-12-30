using System;
using System.Threading.Tasks;
using AOT.Utl;
using Best.SignalR;
using Best.SignalR.Encoders;
using UnityEngine;
using Color = System.Drawing.Color;

namespace AOT.Network
{
    //SignalR连接实例
    public class SignalRConnection
    {
        public const string ServerCall = "ServerCall";

        public SignalRConnectionHandler.States Status => _connHandler.Status;
        public event Action<string> OnServerCall;
        private SignalRConnectionHandler _connHandler;

        public SignalRConnection(SignalRClient client)
        {
            _connHandler = new SignalRConnectionHandler(InstanceHub, h => h.StartClose(), OnDebug);
            return;

            HubConnection InstanceHub()
            {
                var hub = new HubConnection(new Uri(client.ServerUrl + "?access_token=" + ApiCaller.AccessToken), new JsonProtocol(new JsonDotNetEncoder(Json.Settings)));
                hub.On<string>(ServerCall, msg => OnServerCall?.Invoke(msg));
                hub.OnError += OnError;
                return hub;
            }
        }

        private void OnError(HubConnection hub, string message)
        {
            Debug.LogError($"Hub State:{hub.State}----> {message}".Color(Color.Tomato));
        }

        private void OnDebug(string errMessage, Exception e)
        {
            if (!string.IsNullOrEmpty(errMessage)) Debug.Log(errMessage);
            if(e!= null) Debug.LogException(e);
        }

        public async Task<bool> ConnectAsync() => await _connHandler.ConnectAsync();

        public async Task DisconnectAsync() => await _connHandler.DisconnectAsync();

        public Task<string> InvokeAsync(string method, params object[] args) => _connHandler.InvokeTask<string>(method, args);
    }
}
