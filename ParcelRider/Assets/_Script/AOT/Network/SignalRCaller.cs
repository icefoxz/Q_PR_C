using System;
using System.Threading.Tasks;
using AOT.Core;
using AOT.Utl;
using AOT.Views;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using UnityEngine;

namespace AOT.Network
{
    /// <summary>
    /// SignalR请求器, 服务器的请求都在这里实现
    /// </summary>
    public class SignalRCaller
    {
        private SignalRClient _signalRClient;

        public SignalRCaller(SignalRClient signalRClient) => _signalRClient = signalRClient;

        public void OnServerCall(string dataBag)
        {
            var bag = DataBag.Deserialize(dataBag);
            var callEvent = bag.DataName;
            App.SendEvent(callEvent, bag);
        }

        public void Req_Do_Version(long[] orderIds,
            Action<DataBag> successAction,
            string errorMessage = null) =>
            Invoke(SignalREvents.Req_Do_Vers, successAction, errorMessage, orderIds);

        //重新登录和请求机制
        private async void Invoke(string method,
            Action<DataBag> successAction,
            string errorMessage,
            params object[] args)
        {
            if (_signalRClient.State == SignalRClient.States.Disconnected)
            {
                var retrySecs = 2;
                do
                {
                    var isConnected = await _signalRClient.ConnectAsync();
                    if (isConnected) break;
                    var retry = await ReqRetryWindow.Await("Connection lost.");
                    if (!retry) return;
                    await Task.Delay(TimeSpan.FromSeconds(retrySecs));
                    retrySecs++;

                } while (_signalRClient.State != SignalRClient.States.Connected);
            }
            RetryCaller.Start(call: 
                () => _signalRClient.InvokeAsync(method, args),
                successAction: result =>
                {
                    var bag = DataBag.Deserialize(result);
                    successAction(bag);
                },
                a =>
                {
#if UNITY_EDITOR
                    Debug.LogError($"<color=cyan>{a.arg}<-color>\n<color=red>Server response error! {a.err}</color>");
#endif
                    return ReqRetryWindow.Await(errorMessage ?? a.arg);
                });
        }
    }
}