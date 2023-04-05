using System;
using System.Collections;
using System.Linq;
using Controllers;
using Core;
using DataModel;
using UnityEngine;
using UnityEngine.UI;
using Views;

public interface IUiManager
{
    void SetPackageConfirm(float point, float kg, float meter);
    void DisplayWindows(bool display);
    void ViewOrder(int orderId);
    void PlayCoroutine(IEnumerator co, bool transparentPanel, Action callback);
    void LoginInit();
}
public class UiManager : MonoBehaviour,IUiManager
{
    [SerializeField] private Panel _panel;
    [SerializeField] private Transform _overlapPages;

    [SerializeField] private Page _mainPage;
    [SerializeField] private Page _loginPage;
    [SerializeField] private Page _newPackagePage;
    [SerializeField] private Page _orderViewPage;
    [SerializeField] private Page _paymentPage;
    [SerializeField] private Page _win_packageConfirm;
    [SerializeField] private View _view_accountSect;

    [SerializeField] private Image _windows;

    //覆盖mainpage的页面
    private Transform OverlapPages => _overlapPages;
    private LoginPage LoginPage { get; set; }
    private MainPage MainPage { get; set; }
    private PaymentPage PaymentPage { get; set; }
    private OrderViewPage OrderViewPage { get; set; }
    private View_AccountSect View_AccountSect { get; set; }
    
    private NewPackagePage NewPackagePage { get; set; }
    private PackageConfirmWindow PackageConfirmWindow { get; set; }

    private PackageController PackageController => App.GetController<PackageController>();
    private LoginController LoginController => App.GetController<LoginController>();

    public void Init()
    {
        View_AccountSect = new View_AccountSect(_view_accountSect);
        LoginPage = new LoginPage(_loginPage, this);
        MainPage = new MainPage(_mainPage, this);
        PaymentPage = new PaymentPage(_paymentPage, OnPaymentCallback, this);
        PackageConfirmWindow = new PackageConfirmWindow(_win_packageConfirm, this);
        OrderViewPage = new OrderViewPage(_orderViewPage, this);
        NewPackagePage = new NewPackagePage(v: _newPackagePage, () =>
        {
            PackageController.CreatePackage(NewPackagePage.GenerateOrder());
            PaymentPage.Show();
        }, this);
        LoginController.CheckLoginStatus(OnLoginAction);
    }

    public void CloseAllPages(bool includedMainPage)
    {
        if(includedMainPage)
            MainPage.Hide();
        foreach (Transform page in OverlapPages) page.gameObject.SetActive(false);
    }

    private void OnPaymentCallback(bool isSuccess)
    {
        if (isSuccess)
        {
            NewPackagePage.Hide();
            NewPackagePage.ResetUi();
            PackageController.AddCurrentOrder();
            MainPage.SetOrders(PackageController.Orders.ToArray());
        }
    }

    public void SetPackageConfirm(float point, float kg, float meter) => PackageConfirmWindow.Set(point, kg, meter,()=> NewPackagePage.Set(kg, meter));
    public void DisplayWindows(bool display) => _windows.gameObject.SetActive(display);
    public void ViewOrder(int orderId)
    {
        var order = PackageController.GetOrder(orderId);
        OrderViewPage.Set(order);
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

    public void LoginInit()
    {
        MainPage.Show();
        UpdateAccountInfo();
    }

    private void OnLoginAction(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            UpdateAccountInfo();
            MainPage.Show();
            return;
        }
        LoginPage.Show();
    }

    //更新账号信息
     private void UpdateAccountInfo()
    {
        var userName = LoginController.GetAccountName();
        var userAvatar = LoginController.GetUserAvatar();
        View_AccountSect.Set(userName, userAvatar);
        View_AccountSect.Show();
    }
}