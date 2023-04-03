using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Views;

public interface IUiManager
{
    void SetPackageConfirm(float point, float kg, float meter);
    void DisplayWindows(bool display);
    void ViewOrder(int orderId);
    void PlayCoroutine(IEnumerator co, bool transparentPanel, Action callback);
}
public class UiManager : MonoBehaviour,IUiManager
{
    [SerializeField] private Page _loginPage;
    [SerializeField] private Page _mainPage;
    [SerializeField] private Panel _panel;
    [SerializeField] private Page _win_packageConfirm;
    [SerializeField] private Image _windows;
    private LoginPage LoginPage { get; set; }
    private MainPage MainPage { get; set; }
    private PackageConfirmWindow PackageConfirmWindow { get; set; }
    public void Init()
    {
        LoginPage = new LoginPage(_loginPage, this);
        MainPage = new MainPage(_mainPage, this);
        PackageConfirmWindow = new PackageConfirmWindow(_win_packageConfirm,SetNewPackage, this);
    }

    private void SetNewPackage()
    {
        throw new System.NotImplementedException();
    }

    public void SetPackageConfirm(float point, float kg, float meter) => PackageConfirmWindow.Set(point, kg, meter);
    public void DisplayWindows(bool display)=> _windows.gameObject.SetActive(display);
    public void ViewOrder(int orderId)
    {
        throw new System.NotImplementedException($"No DO implement!");
    }

    public void ShowPanel(bool transparent,bool displayLoadingImage = true) => _panel.Show(transparent,displayLoadingImage);
    public void HidePanel() => _panel.Hide();

    public void PlayCoroutine(IEnumerator co, bool transparentPanel ,Action callback)
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