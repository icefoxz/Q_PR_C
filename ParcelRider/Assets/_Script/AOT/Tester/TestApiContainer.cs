using System;
using AOT.Controllers;
using AOT.Core;
using AOT.Test;
using OrderHelperLib.Contracts;
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

        RegGen(nameof(userOrderController.Do_RequestCancel), () =>
        {
            var (isSuccess, status)  = OrderParcelSo.GetOrderService();
            return new object[] { isSuccess, status };
        }, userOrderController);
        RegGen(nameof(userOrderController.Do_Payment), () =>
        {
            var (isSuccess, message) = OrderParcelSo.PaymentOrderService();
            return new object[] { isSuccess, message };
        }, userOrderController);
        RegGen(nameof(userOrderController.Do_Create), () =>
        {
            var (isSuccess, message) = OrderParcelSo.CreateOrderService();
            return new object[] { isSuccess, message };
        },userOrderController);
    }

    private void RegLoginService()
    {
        var userLoginController = App.GetController<LoginController>();

        RegGen(nameof(userLoginController.RequestLogin), () =>
        {
            var (isSuccess, message) = LoginServiceSo.GetLoginService();
            return new object[] { isSuccess, message };
        }, userLoginController);
        RegGen(nameof(userLoginController.RequestGoogle), () =>
        {
            var (isSuccess, message) = LoginServiceSo.GetLoginService();
            return new object[] { isSuccess, message };
        },userLoginController);
        RegGen(nameof(userLoginController.RequestFacebook), () =>
        {
            var (isSuccess, message) = LoginServiceSo.GetLoginService();
            return new object[] { isSuccess, message };
        }, userLoginController);
    }

    private void RegGen(string method, Func<object[]> func, ControllerBase controller) => controller.RegTester(func, method);
}