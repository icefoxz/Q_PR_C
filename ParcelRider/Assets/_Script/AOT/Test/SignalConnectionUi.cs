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
    private Dictionary<Image, SignalRConnectionHandler.States> _stateMap;

    private void Start()
    {
        _stateMap = new Dictionary<Image, SignalRConnectionHandler.States>
        {
            { img_disconnected, SignalRConnectionHandler.States.Disconnected },
            { img_connected, SignalRConnectionHandler.States.Connected },
            { img_await, SignalRConnectionHandler.States.Await },
        };
    }

    private void UpdateStatus()
    {
        var state = _client.State;
        foreach (var pair in _stateMap) pair.Key.gameObject.SetActive(pair.Value == state);
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
