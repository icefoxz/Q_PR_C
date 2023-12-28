using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AOT.Network
{
    public class SignalRClientTester : MonoBehaviour
    {
        [SerializeField] private SignalRClient _client;
        private SignalRCaller _caller;

        void Start()
        {
            _client.OnServerCall += s => Debug.Log($"<color=orange>Server call:\n{s}</color>");
        }

        [Button] public void Connect() => _client.Connect(connected => Debug.Log(connected ? "Connected!" : "Failed to connect!"));
        [Button] public void Disconnect() => _client.Disconnect();
        [Button] public async void TestCall(string method, string arg)
        {
            var result = await _client.InvokeAsync(method, arg);
            Debug.Log($"Server response:\n{result}");
        }

        [Button] public void TestQueryVersion(string ids)
        {
            _caller.Req_Do_Version(ids.Split(',').Select(long.Parse).ToArray(),
                b => Debug.Log(string.Join(',', b.Get<Dictionary<long, int>>(0).Select(a => $"{a.Key}:{a.Value}"))));
        }
    }
}