using System;
using System.Collections;
using AOT.Controllers;
using AOT.Core;
using AOT.DataModel;
using AOT.Test;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using UnityEngine;

public class TestApiContainer : MonoBehaviour
{
    public LoginServiceSo LoginServiceSo;
    public OrderParcelSo OrderParcelSo;
    public HistoryOrderSo HistoryOrderSo;
    public ActiveOrderSo ActiveOrderSo;
    public RiderLoginServiceSo RiderLoginServiceSo;

#if UNITY_EDITOR
    void Start()
    {
        StartCoroutine(InitCo());
    }
#endif

    private IEnumerator InitCo()
    {
        yield return new WaitForSeconds(3);
        RegLoginService();
        RegOrderService();
        RegRiderLoginService();
        RegRiderOrderService();
    }

    private void RegOrderService()
    {
        var userOrderController = App.GetController<UserOrderController>();

        RegTester(nameof(userOrderController.Do_RequestCancel), args =>
        {
            var orderId = (long)args[0];
            var (isSuccess, status, ordId)  = OrderParcelSo.GetOrderService(orderId);
            ActiveOrderSo.CancelOrder(orderId);
            return new object[] { isSuccess, status, ordId };
        }, userOrderController);
        RegTester(nameof(userOrderController.Do_Payment), args =>
        {
            var payM = (PaymentMethods)args[0];
            var (isSuccess, message, payMethod) = OrderParcelSo.PaymentOrderService(payM);
            ActiveOrderSo.SetPayment(payM);
            return new object[] { isSuccess, message, payMethod };
        }, userOrderController);
        RegTester(nameof(userOrderController.Do_Create), args =>
        {
            var order = (DeliverOrderModel)args[0];
            var (isSuccess, message) = OrderParcelSo.CreateOrderService(order);
            var data = DataBag.Deserialize(message);
            var newOrder = data.Get<DeliverOrderModel>(0);
            ActiveOrderSo.SetNewOrder(newOrder);
            return new object[] { isSuccess, message };
        },userOrderController);
        RegTester(nameof(userOrderController.Do_UpdateAll), args =>
        {
            var message = ActiveOrderSo.GetActiveOrderList();
            return new object[] { message };
        }, userOrderController);
        RegTester(nameof(userOrderController.Do_UpdateHistory), args =>
        {
            var message = HistoryOrderSo.GetHistoryList();
            return new object[] { message };
        }, userOrderController);
    }

    private void RegLoginService()
    {
        var userLoginController = App.GetController<LoginController>();

        RegTester(nameof(userLoginController.RequestLogin), args =>
        {
            var username = args[0].ToString();
            var password = args[1].ToString();
            var (isSuccess, message) = LoginServiceSo.UserLoginService(username);
            return new object[] { isSuccess, message };
        }, userLoginController);
        RegTester(nameof(userLoginController.RequestGoogle), _ =>
        {
            var (isSuccess, message) = LoginServiceSo.ThirdPartyLoginService();
            return new object[] { isSuccess, message };
        },userLoginController);
        RegTester(nameof(userLoginController.RequestFacebook), _ =>
        {
            var (isSuccess, message) = LoginServiceSo.ThirdPartyLoginService();
            return new object[] { isSuccess, message };
        }, userLoginController);
        RegTester(nameof(userLoginController.RequestRegister), _ =>
        {
            var (isSuccess, message) = LoginServiceSo.ThirdPartyLoginService();
            return new object[] { isSuccess, message };
        }, userLoginController);
        RegTester(nameof(userLoginController.CheckLoginStatus), _ =>
        {
            var isSuccess = true;
            return new object[] { isSuccess };
        }, userLoginController);
    }

    private void RegRiderLoginService()
    {
        var riderLoginController = App.GetController<RiderLoginController>();

        RegTester(nameof(riderLoginController.Rider_RequestLogin), args=>
        {
            var username = args[0];
            var (isSuccess, message) = RiderLoginServiceSo.GetLoginService((string)username);
            return new object[] { isSuccess, message};
        }, riderLoginController);
    }
    private void RegRiderOrderService()
    {
        var riderOrderService = App.GetController<RiderOrderController>();
        RegTester(nameof(riderOrderService.PickItem), args=>
        {
            var orderId = (long)args[0];
            var (isSuccess, status, ordId) = ActiveOrderSo.ItemPicked(orderId);
            return new object[] { isSuccess, status, ordId };
        },riderOrderService);
        RegTester(nameof(riderOrderService.ItemCollection), args =>
        {
            var orderId = (long)args[0];
            var (isSuccess, status, oId) = ActiveOrderSo.ItemCollected(orderId);
            return new object[] { isSuccess, status, oId };
        }, riderOrderService);
        RegTester(nameof(riderOrderService.Complete), args =>
        {
            var orderId = (long)args[0];
            var (isSuccess, status, ordId) = ActiveOrderSo.DeliveryComplete(orderId);

            return new object[] {isSuccess, status, ordId };
        }, riderOrderService);
        RegTester(nameof(riderOrderService.Do_UpdateAll), _ =>
        {
            var message = ActiveOrderSo.GetActiveOrderList();
            return new object[] { message };
        }, riderOrderService);
        RegTester(nameof(riderOrderService.Do_AssignRider), args=>
        {
            var order = (DeliverOrderModel)args[0];
            var newOrder = RiderLoginServiceSo.SetRiderInfo(order);
            var data = DataBag.Deserialize(newOrder);
            var newData = data.Get<DeliverOrderModel>(0);
            var (isSuccess, status, ordId) = ActiveOrderSo.OrderAssigned(newData);
            return new object[] { isSuccess, status, ordId };
        }, riderOrderService);
    }

    private void RegTester(string method, Func<object[], object[]> func, ControllerBase controller) => controller.RegTester(func, method);
}