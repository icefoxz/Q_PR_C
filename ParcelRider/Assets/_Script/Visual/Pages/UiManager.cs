using System;
using System.Collections;
using System.Linq;
using Controllers;
using Core;
using UnityEngine;
using UnityEngine.UI;
using Views;
using Visual.Pages.Rider;

public interface IUiManager
{
    void SetPackageConfirm(float point, float kg, float length, float width, float height);
    void DisplayWindows(bool display);
    void ViewOrder(string orderId);
    void PlayCoroutine(IEnumerator co, bool transparentPanel, Action callback);
    void LoginInit();
    void RiderMode();
    void UserMode();
}

public class UiManager : MonoBehaviour, IUiManager
{
    [SerializeField] private Panel _panel;
    [SerializeField] private Transform _overlapPages;

    [SerializeField] private Page _mainPage;
    [SerializeField] private Page _loginPage;
    [SerializeField] private Page _newPackagePage;
    [SerializeField] private Page _orderViewPage;
    [SerializeField] private Page _paymentPage;
    [SerializeField] private Page _riderPage;

    //windows
    [SerializeField] private Page _win_packageConfirm;
    [SerializeField] private Page _win_account;

    [SerializeField] private View _view_accountSect;

    [SerializeField] private Image _windows;

    //覆盖mainpage的页面
    private Transform OverlapPages => _overlapPages;
    private LoginPage LoginPage { get; set; }
    private MainPage MainPage { get; set; }
    private PaymentPage PaymentPage { get; set; }
    private OrderViewPage OrderViewPage { get; set; }
    private RiderPage RiderPage { get; set; }
    private View_AccountSect View_AccountSect { get; set; }

    private NewPackagePage NewPackagePage { get; set; }
    private PackageConfirmWindow PackageConfirmWindow { get; set; }
    private AccountWindow AccountWindow { get; set; }

    private PackageController PackageController => App.GetController<PackageController>();
    private LoginController LoginController => App.GetController<LoginController>();

    public void Init()
    {
        View_AccountSect = new View_AccountSect(_view_accountSect, 
            () => AccountWindow.Show(),
            OnLogoutAction);
        LoginPage = new LoginPage(_loginPage, this);
        MainPage = new MainPage(_mainPage, this);
        PaymentPage = new PaymentPage(_paymentPage, OnPaymentCallback, this);
        PackageConfirmWindow = new PackageConfirmWindow(_win_packageConfirm, this);
        AccountWindow = new AccountWindow(_win_account, this);
        OrderViewPage = new OrderViewPage(_orderViewPage, this);
        RiderPage = new RiderPage(_riderPage, this);
        NewPackagePage = new NewPackagePage(v: _newPackagePage, () =>
        {
            PackageController.CreatePackage(NewPackagePage.GenerateOrder(), success =>
            {
                if (success)
                {

                }
            });
            PaymentPage.Show();
        }, this);
        LoginController.CheckLoginStatus(OnLoginAction);
    }

    private void OnLogoutAction()
    {
        CloseAllPages();
        LoginPage.Show();
    }

    public void CloseAllPages()
    {
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

    public void SetPackageConfirm(float point, float kg, float length, float width, float height) =>
        PackageConfirmWindow.Set(point, kg, length, width, height, () => NewPackagePage.Set(kg, length, width, height));

    public void DisplayWindows(bool display) => _windows.gameObject.SetActive(display);

    public void ViewOrder(string orderId)
    {
        var order = PackageController.GetOrder(orderId);
        OrderViewPage.Set(order);
    }

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

    public void LoginInit() => MainPage.Show();

    public void RiderMode()
    {
        CloseAllPages();
        RiderPage.Show();
    }

    public void UserMode()
    {
        CloseAllPages();
        MainPage.Show();
    }

    private void OnLoginAction(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            MainPage.Show();
            return;
        }
        LoginPage.Show();
    }

    //更新账号信息
    private void UpdateAccountInfo()
    {
        var user = App.Models.User;
        if (user != null)
        {
            var userName = user.Username;
            var userAvatar = user.Avatar;
            View_AccountSect.Set(userName, userAvatar);
            View_AccountSect.Show();
        }
        else View_AccountSect.Hide();
    }

    //注册为骑手
    public void RegisterRider()
    {
        PackageConfirmWindow.Hide();
        RiderPage.RegisterRider();
    }
}