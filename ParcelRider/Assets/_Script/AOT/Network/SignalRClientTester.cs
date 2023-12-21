using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AOT.Core;
using Cysharp.Threading.Tasks;
using OrderHelperLib.Contracts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AOT.Network
{
    public class SignalRClientTester : MonoBehaviour
    {
        private enum RetryMode
        {
            None,
            Waiting,
            Yes,
            No
        }
        [SerializeField] private SignalRClient _client;
        private SignalRCaller _caller;
        [SerializeField] private RetryMode _retryMode = RetryMode.Waiting;

        void Start()
        {
            _client.OnServerCall += s => Debug.Log($"Server call:\n{s}");
            _caller = new SignalRCaller(_client);
        }

        [Button]
        public void Connect() =>
            _client.Connect(connected => Debug.Log(connected ? "Connected!" : "Failed to connect!"));
        [Button] public void Disconnect() => _client.Disconnect();
        [Button] public async void TestCall(string method, string arg)
        {
            var result = await _client.InvokeAsync(method, arg);
            Debug.Log($"Server response:\n{result}");
        }

        [Button]
        public async void TestQueryVersion(string ids)
        {
            await _caller.Req_Do_Version(ids.Split(',').Select(long.Parse).ToArray(),
                b => Debug.Log(string.Join(',', b.Get<Dictionary<long, int>>(0).Select(a => $"{a.Key}:{a.Value}"))),
                RetryTask);
        }

        private async Task<bool> RetryTask(string errorMsg)
        {
            Debug.Log($"Retry: {errorMsg}");
            _retryMode = RetryMode.Waiting;
            await UniTask.WaitUntil(() => _retryMode != RetryMode.Waiting);
            var isRetry = _retryMode == RetryMode.Yes;
            await UniTask.Delay(100);
            _retryMode = isRetry ? RetryMode.Waiting : RetryMode.None;
            return isRetry;
        }
    }
}