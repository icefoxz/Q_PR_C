using Core;
using UnityEngine;
using Utl;

public class AppLunch : MonoBehaviour
{
    [SerializeField] private Res _res;
    [SerializeField] private Canvas _mainCanvas;
    [SerializeField] private UiManagerBase _uiManager;
    [SerializeField] private MonoService _monoService;
    [SerializeField] private string _serverUrl;
    [SerializeField] private ApiPanel _apiPanel;
    [SerializeField] private bool _startUi;
    private void Start()
    {
        App.Run(_res, _mainCanvas, _uiManager, _monoService, _startUi);
        _apiPanel.Init(_serverUrl);
    }
}