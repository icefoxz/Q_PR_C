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

    public void Init()
    {
        RegLoginService();
        RegOrderService();
    }

    private void RegOrderService()
    {
        var userOrderController = App.GetController<UserOrderController>();

        RegGen(nameof(userOrderController.Do_RequestCancel), args =>
        {
            var orderId = (int)args[0];
            var (isSuccess, status, ordId)  = OrderParcelSo.GetOrderService(orderId);
            return new object[] { isSuccess, status, ordId };
        }, userOrderController);
        RegGen(nameof(userOrderController.Do_Payment), args =>
        {
            var payM = (PaymentMethods)args[0];
            var (isSuccess, message, payMethod) = OrderParcelSo.PaymentOrderService(payM);
            return new object[] { isSuccess, message, payMethod };
        }, userOrderController);
        RegGen(nameof(userOrderController.Do_Create), args =>
        {
            var order = (DeliverOrderModel)args[0];
            var (isSuccess, message) = OrderParcelSo.CreateOrderService(order);
            return new object[] { isSuccess, message };
        },userOrderController);
    }

    private void RegLoginService()
    {
        var userLoginController = App.GetController<LoginController>();

        RegGen(nameof(userLoginController.RequestLogin), args =>
        {
            var username = args[0].ToString();
            var password = args[1].ToString();
            var (isSuccess, message) = LoginServiceSo.GetLoginService();
            return new object[] { isSuccess, message };
        }, userLoginController);
        RegGen(nameof(userLoginController.RequestGoogle), _ =>
        {
            var (isSuccess, message) = LoginServiceSo.GetLoginService();
            return new object[] { isSuccess, message };
        },userLoginController);
        RegGen(nameof(userLoginController.RequestFacebook), _ =>
        {
            var (isSuccess, message) = LoginServiceSo.GetLoginService();
            return new object[] { isSuccess, message };
        }, userLoginController);
    }

    private void RegGen(string method, Func<object[], object[]> func, ControllerBase controller) => controller.RegTester(func, method);
}