using System;
using System.Collections;
using UnityEngine;

public interface IUiManager
{
    void ShowPanel(bool transparent, bool displayLoadingImage = true);
    void HidePanel();
    void DisplayWindows(bool display);
    void PlayCoroutine(IEnumerator co, bool transparentPanel, Action callback);
}
public abstract class UiManagerBase : MonoBehaviour, IUiManager
{
    [SerializeField] private GameObject _windows;
    [SerializeField] private Panel _panel;
    protected GameObject Windows => _windows;
    protected Panel Panel => _panel;

    public abstract void Init(bool startUi);

    public void DisplayWindows(bool display) => _windows.SetActive(display);

    public void ShowPanel(bool transparent, bool displayLoadingImage = true) =>
        _panel.Show(transparent, displayLoadingImage);

    public void HidePanel() => _panel.Hide();

    public void PlayCoroutine(IEnumerator co, bool transparentPanel, Action callback)
    {
        StartCoroutine(WaitForCo());

        IEnumerator WaitForCo()
        {
            ShowPanel(transparentPanel);
            yield return co;
            callback?.Invoke();
            HidePanel();
        }
    }
}