using System;
using AOT.Core;
using AOT.Utl;
using AOT.Views;
using UnityEngine;

namespace AOT
{
    public class AppLaunch : MonoBehaviour
    {
        [SerializeField] private Res _res;
        [SerializeField] private Canvas _mainCanvas;
        [SerializeField] private UiManagerField _uiManager;
        [SerializeField] private MonoService _monoService;
        [SerializeField] private string _serverUrl;
        [SerializeField] private ApiPanel _apiPanel;
        [SerializeField] private bool _autoStartUi;
        [SerializeField] private bool _testMode;
        public static bool TestMode { get; private set; }
        public static bool IsUserMode { get; private set; }

        private void Start()
        {
            TestMode = _testMode;
            IsUserMode = _uiManager.IsUserMode;
            App.Run(res: _res, monoService: _monoService);
            _apiPanel.Init(_serverUrl);
            App.UiInit(_mainCanvas, _uiManager.Get(), _autoStartUi);
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
            public bool IsUserMode => _mode == AppMode.User;

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
}