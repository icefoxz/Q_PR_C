using System;
using Core;
using UnityEngine;
using Utl;

public class AppLaunch : MonoBehaviour
{
    [SerializeField] private Res _res;
    [SerializeField] private Canvas _mainCanvas;
    [SerializeField] private UiManagerField _uiManager;
    [SerializeField] private MonoService _monoService;
    [SerializeField] private string _serverUrl;
    [SerializeField] private ApiPanel _apiPanel;
    [SerializeField] private bool _startUi;
    [SerializeField] private bool _testMode;
    public static bool TestMode { get; private set; }

    private void Start()
    {
        TestMode = _testMode;
        App.Run(res: _res, canvas: _mainCanvas, uiManager: _uiManager.Get(), monoService: _monoService, startUi: _startUi);
        _apiPanel.Init(_serverUrl);
    }

    [Serializable]private class UiManagerField
    {
        private enum AppMode
        {
            User,
            Rider
        }
        [SerializeField] private AppMode _mode;
        [SerializeField] private UiManagerBase _user;
        [SerializeField] private UiManagerBase _rider;

        public UiManagerBase Get()
        {
            _rider.gameObject.SetActive(_mode == AppMode.Rider);
            _user.gameObject.SetActive(_mode == AppMode.User);
            switch (_mode)
            {
                case AppMode.User:
                    return _user;
                case AppMode.Rider:
                    return _rider;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}