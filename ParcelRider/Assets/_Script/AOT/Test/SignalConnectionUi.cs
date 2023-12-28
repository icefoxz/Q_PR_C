using System;
using System.Collections;
using System.Collections.Generic;
using AOT.Network;
using UnityEngine;
using UnityEngine.UI;

public class SignalConnectionUi : MonoBehaviour
{
    [SerializeField]private SignalRClient _client;
    [SerializeField]private Image img_disconnected;
    [SerializeField]private Image img_connected;
    [SerializeField]private Image img_await;
    [SerializeField]private Text text_status;
    private Dictionary<Image, SignalRClient.States> _stateMap;

    private void Start()
    {
        _stateMap = new Dictionary<Image, SignalRClient.States>
        {
            { img_disconnected, SignalRClient.States.Disconnected },
            { img_connected, SignalRClient.States.Connected },
            { img_await, SignalRClient.States.Await },
        };
    }

    private void UpdateStatus()
    {
        var state = _client.State;
        foreach (var pair in _stateMap)
        {
            pair.Key.gameObject.SetActive(pair.Value == state);
        }
        text_status.text = state.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (_stateMap == null) return;
        if (_client == null) return;
        UpdateStatus();
    }
}
