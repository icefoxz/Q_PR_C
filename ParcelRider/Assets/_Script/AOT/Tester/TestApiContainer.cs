using System;
using AOT.Controllers;
using AOT.Core;
using AOT.Test;
using UnityEngine;

public class TestApiContainer : MonoBehaviour
{
    public LoginServiceSo LoginServiceSo;

    public void Init()
    {
        RegLoginService();
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
    }

    private void RegGen(string method, Func<object[]> func, ControllerBase controller) => controller.RegTester(func, method);
}