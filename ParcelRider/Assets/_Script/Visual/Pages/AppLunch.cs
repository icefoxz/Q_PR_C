using Core;
using UnityEngine;

public class AppLunch : MonoBehaviour
{
    [SerializeField] private Res _res;
    [SerializeField] private Canvas _mainCanvas;
    [SerializeField] private UiManager _uiManager;
    [SerializeField] private MonoService _monoService;
    private void Start()
    {
        App.Run(_res, _mainCanvas, _uiManager, _monoService);
    }
}