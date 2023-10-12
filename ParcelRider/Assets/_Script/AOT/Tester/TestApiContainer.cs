using System;
using AOT.Controllers;
using AOT.Core;
using AOT.Test;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using UnityEngine;

public class TestApiContainer : MonoBehaviour
{
    public LoginServiceSo LoginServiceSo;
    public OrderParcelSo OrderParcelSo;
    public HistoryOrderSo HistoryOrderSo;

    public void Init()
    {
        RegLoginService();
        RegOrderService();
    }

    private void RegOrderService()
    {
        var userOrderController = App.GetController<UserOrderController>();

        RegTester(nameof(userOrderController.Do_RequestCancel), args =>
        {
            var orderId = (int)args[0];
            var (isSuccess, status, ordId)  = OrderParcelSo.GetOrderService(orderId);
            return new object[] { isSuccess, status, ordId };
        }, userOrderController);
        RegTester(nameof(userOrderController.Do_Payment), args =>
        {
            var payM = (PaymentMethods)args[0];
            var (isSuccess, message, payMethod) = OrderParcelSo.PaymentOrderService(payM);
            return new object[] { isSuccess, message, payMethod };
        }, userOrderController);
        RegTester(nameof(userOrderController.Do_Create), args =>
        {
            var order = (DeliverOrderModel)args[0];
            var (isSuccess, message) = OrderParcelSo.CreateOrderService(order);
            return new object[] { isSuccess, message };
        },userOrderController);
        RegTester(nameof(userOrderController.Do_UpdateAll), args =>
        {
            return new object[] {1};
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
            var (isSuccess, message) = LoginServiceSo.GetLoginService();
            return new object[] { isSuccess, message };
        }, userLoginController);
        RegTester(nameof(userLoginController.RequestGoogle), _ =>
        {
            var (isSuccess, message) = LoginServiceSo.GetLoginService();
            return new object[] { isSuccess, message };
        },userLoginController);
        RegTester(nameof(userLoginController.RequestFacebook), _ =>
        {
            var (isSuccess, message) = LoginServiceSo.GetLoginService();
            return new object[] { isSuccess, message };
        }, userLoginController);
        RegTester(nameof(userLoginController.RequestRegister), _ =>
        {
            var (isSuccess, message) = LoginServiceSo.GetLoginService();
            return new object[] { isSuccess, message };
        }, userLoginController);
        RegTester(nameof(userLoginController.CheckLoginStatus), _ =>
        {
            var isSuccess = true;
            return new object[] { isSuccess };
        }, userLoginController);
    }

    private void RegTester(string method, Func<object[], object[]> func, ControllerBase controller) => controller.RegTester(func, method);
}