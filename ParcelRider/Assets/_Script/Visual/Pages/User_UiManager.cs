using System;
using System.Collections;
using AOT.Controllers;
using AOT.Core;
using AOT.Views;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using UnityEngine;
using Visual.Pages.Rider;
using Visual.Pages.Visual.Pages.Rider;
using Visual.Sects;

namespace Visual.Pages
{
    public class User_UiManager : UiManagerBase
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
        [SerializeField] private Page _win_image;

        [SerializeField] private View _view_accountSect;

        //覆盖mainpage的页面
        private Transform OverlapPages => _overlapPages;
        private User_LoginPage User_LoginPage { get; set; }
        private User_MainPage User_MainPage { get; set; }
        private PaymentPage PaymentPage { get; set; }
        private User_OrderViewPage User_OrderViewPage { get; set; }
    
        private View_AccountSect View_AccountSect { get; set; }

        private User_NewPackagePage User_NewPackagePage { get; set; }
        private PackageConfirmWindow PackageConfirmWindow { get; set; }
        private AccountWindow AccountWindow { get; set; }
        private ConfirmWindow ConfirmWindow { get; set; }
        private MessageWindow MessageWindow { get; set; }
        private ImageWindow ImageWindow { get; set; }

        private UserOrderController UserOrderController => App.GetController<UserOrderController>();
        private LoginController LoginController => App.GetController<LoginController>();

        public override void Init(bool startUi)
        {
            View_AccountSect = new View_AccountSect(v: _view_accountSect, 
                onAccountAction: () => AccountWindow.Show(),
                logoutAction: 
                Logout_To_LoginPage);
            User_LoginPage = new User_LoginPage(v: _loginPage, onLoggedInAction: Login_Init, uiManager: this);
            User_MainPage =
                new User_MainPage(v: _mainPage, id => UserOrderController.Do_SetCurrent(id), uiManager: this);
            PaymentPage = new PaymentPage(v: _paymentPage, uiManager: this);

            PackageConfirmWindow = new PackageConfirmWindow(v: _win_packageConfirm, uiManager: this);
            AccountWindow = new AccountWindow(v: _win_account, uiManager: this);
            ConfirmWindow = new ConfirmWindow(v: _win_confirm, uiManager: this);
            MessageWindow = new MessageWindow(v: _win_message, uiManager: this);
            ImageWindow = new ImageWindow(v: _win_image, uiManager: this);

            User_OrderViewPage = new User_OrderViewPage(v: _orderViewPage, OnRequestCancel,
                uiManager: this);
            User_NewPackagePage = new User_NewPackagePage(v: _newPackagePage, onSubmit: NewDo_Submit, uiManager: this);
            if (startUi) StartUi();
            Windows.SetActive(value: false);
        }

        private void OnRequestCancel()
        {
            var order = App.Models.CurrentOrder;
            if (order == null ||
                !DoStateMap.IsAssignableSubState(TransitionRoles.User, (int)order.State, DoSubState.SenderCancelState))
            {
                MessageWindow.Set(title: "Cancel Order", content: "Order is closed");
                return;
            }

            ConfirmWindow.Set(() => UserOrderController.Do_RequestCancel(order.Id));
        }

        private void StartUi()
        {
            StartCoroutine(InitCheckLogin());

            IEnumerator InitCheckLogin()
            {
                yield return new WaitForSeconds(3);
                LoginController.CheckLoginStatus(onLoginAction: OnLoginAction);
            }
        }


        //当新的包裹提交
        private void NewDo_Submit()
        {
            var order = User_NewPackagePage.GenerateOrder();
            var item = order.ItemInfo;
            var payment = order.PaymentInfo;
            UserOrderController.Do_Create(order, (success, message) =>
            {
                if(!success)
                {
                    MessageWindow.Set(title: "Create Order Failed", content: message);
                    return;
                }
                PackageConfirmWindow.Set(point: payment.Charge, 
                    kg: item.Weight, 
                    length: item.Length, 
                    width: item.Width, 
                    height: item.Height,
                    onRiderCollectAction: RiderCollectPayment,
                    onDeductFromPoint: DeductFromCredit,
                    onPaymentGateway: PaymentGateway);
            });

            void RiderCollectPayment() => CreateNewDeliveryOrder(method: PaymentMethods.RiderCollection);

            void DeductFromCredit() => CreateNewDeliveryOrder(method: PaymentMethods.UserCredit);

            void PaymentGateway()
            {
                PaymentPage.Set(onPaymentAction: success =>
                {
                    if (success) CreateNewDeliveryOrder(method: PaymentMethods.OnlinePayment);
                });
            }

            void CreateNewDeliveryOrder(PaymentMethods method)
            {
                UserOrderController.Do_Payment(payment: method, callbackAction: (success, message) =>
                {
                    if (success)
                    {
                        User_NewPackagePage.Hide();
                        User_NewPackagePage.ResetUi();
                        UserOrderController.Do_UpdateAll();
                        MessageWindow.Set(title: "Create Order", content: "Success!");
                        return;
                    }
                    MessageWindow.Set(title: "Payment Failed", content: message);
                });
            }
        }

        private void Logout_To_LoginPage()
        {
            CloseAllPages();
            UserOrderController.Logout();
        }

        private void CloseAllPages()
        {
            foreach (Transform page in OverlapPages) page.gameObject.SetActive(value: false);
        }

        public void NewPackage(float point, float kg, float length, float width, float height)
        {
            User_NewPackagePage.Set(kg: kg, length: length, width: width, height: height);
        }

        private void Login_Init()
        {
            UserOrderController.Get_SubStates();
            UserOrderController.Do_UpdateAll();
            UserOrderController.Do_UpdateHistory();
            User_MainPage.Show();
        }

        public void UserMode()
        {
            CloseAllPages();
            User_MainPage.Show();
        }

        private void OnLoginAction(bool isLoggedIn)
        {
            if (isLoggedIn)
            {
                User_MainPage.Show();
                return;
            }
            User_LoginPage.Show();
        }

        public void ViewHistory(long orderId)
        {
            UserOrderController.Do_SetCurrent(orderId);
        }
    }
}