using Core;
using UnityEngine;

public class AppLunch : MonoBehaviour
{
    [SerializeField] private Res _res;
    [SerializeField] private Canvas _mainCanvas;
    [SerializeField] private UiManager _uiManager;
    private void Start()
    {
        App.Run(_res, _mainCanvas, _uiManager);
    }
}