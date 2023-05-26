using System;
using System.Collections;
using Controllers;
using Core;
using OrderHelperLib.Contracts;
using UnityEngine;
using UnityEngine.UI;
using Views;
using Visual.Pages.Rider;

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

public class UiManager : UiManagerBase
{
    [SerializeField] private Transform _overlapPages;

    [SerializeField] private Page _mainPage;
    [SerializeField] private Page _loginPage;
    [SerializeField] private Page _newPackagePage;
    [SerializeField] private Page _orderViewPage;
    [SerializeField] private Page _paymentPage;

    //windows
    [SerializeField] private Page _win_packageConfirm;
    [SerializeField] private Page _win_account;
    [SerializeField] private Page _win_confirm;
    [SerializeField] private Page _win_message;

    [SerializeField] private View _view_accountSect;

    //覆盖mainpage的页面
    private Transform OverlapPages => _overlapPages;
    private LoginPage LoginPage { get; set; }
    private MainPage MainPage { get; set; }
    private PaymentPage PaymentPage { get; set; }
    private OrderViewPage OrderViewPage { get; set; }
    
    private View_AccountSect View_AccountSect { get; set; }

    private NewPackagePage NewPackagePage { get; set; }
    private PackageConfirmWindow PackageConfirmWindow { get; set; }
    private AccountWindow AccountWindow { get; set; }
    private ConfirmWindow ConfirmWindow { get; set; }
    private MessageWindow MessageWindow { get; set; }

    private OrderController OrderController => App.GetController<OrderController>();
    private LoginController LoginController => App.GetController<LoginController>();

    public override void Init(bool startUi)
    {
        View_AccountSect = new View_AccountSect(_view_accountSect, 
            () => AccountWindow.Show(),
            OnLogoutAction);
        LoginPage = new LoginPage(_loginPage, LoginInit, this);
        MainPage = new MainPage(_mainPage, this);
        PaymentPage = new PaymentPage(_paymentPage, this);

        PackageConfirmWindow = new PackageConfirmWindow(_win_packageConfirm, this);
        AccountWindow = new AccountWindow(_win_account, this);
        ConfirmWindow = new ConfirmWindow(_win_confirm, this);
        MessageWindow = new MessageWindow(_win_message, this);

        OrderViewPage = new OrderViewPage(_orderViewPage, this);
        NewPackagePage = new NewPackagePage(v: _newPackagePage, OnNewPackageSubmit, () => NewPackagePage.Hide(), this);
        if (startUi) StartUi();
        Windows.SetActive(false);
    }

    private void StartUi()
    {
        LoginController.CheckLoginStatus(OnLoginAction);
    }

    //当新的包裹提交
    private void OnNewPackageSubmit()
    {
        var order = NewPackagePage.GenerateOrder();
        var p = order.Package;
        PackageConfirmWindow.Set(p.Price, p.Weight, p.Length, p.Width, p.Height, 
            RiderCollectPayment, 
            DeductFromCredit,
            PaymentGateway);
        OrderController.SetCurrent(order);

        void RiderCollectPayment()
        {
            OrderController.SetCurrent(order);
            CreateNewDeliveryOrder(PaymentMethods.RiderCollection);
        }

        void DeductFromCredit()
        {
            OrderController.SetCurrent(order);
            CreateNewDeliveryOrder(PaymentMethods.UserCreditDeduction);
        }

        void PaymentGateway()
        {
            OrderController.SetCurrent(order);
            PaymentPage.Set(success =>
            {
                if (success) CreateNewDeliveryOrder(PaymentMethods.OnlinePayment);
            });
        }

        void CreateNewDeliveryOrder(PaymentMethods method)
        {
            NewPackagePage.Hide();
            NewPackagePage.ResetUi();
            OrderController.CreatePackage(method, success =>
            {
                if (success)
                {
                    //telling message
                }
            });
        }
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

    public void NewPackage(float point, float kg, float length, float width, float height)
    {
        NewPackagePage.Set(kg, length, width, height);
    }


    public void ViewOrder(string orderId)
    {
        var order = OrderController.GetOrder(orderId);
        OrderViewPage.Set(order, () => ConfirmWindow.Set("Cancel Order?", () =>
        {
            OrderController.RequestCancelOrder(orderId, success =>
                {
                    if (!success)
                    {
                        MessageWindow.Set("Order", "Failed to cancel order");
                    }
                }
            );
        }));
    }

    private void LoginInit() => MainPage.Show();

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
}