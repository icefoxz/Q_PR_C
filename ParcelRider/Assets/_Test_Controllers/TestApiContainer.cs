#if UNITY_EDITOR
using System;
using System.Collections;
using AOT.Controllers;
using AOT.Core;
using AOT.Test;
using AOT.Utl;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using UnityEngine;

public class TestApiContainer : MonoBehaviour
{
    public LoginServiceSo LoginServiceSo;
    public OrderParcelSo OrderParcelSo;
    public OrderSimSo ActiveOrderSo;
    public OrderSimSo HistorySo;
    public RiderLoginServiceSo RiderLoginServiceSo;

    void Start()
    {
        StartCoroutine(InitCo());
    }

    private IEnumerator InitCo()
    {
        yield return new WaitForSeconds(1);
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
            var order = ActiveOrderSo.GetOrder(orderId);
            order.Status = (int)status;
            order.SubState = DoSubState.SenderCancelState;
            ActiveOrderSo.CancelOrder(orderId);
            HistorySo.SetNewOrder(order);
            return new object[] { isSuccess, status, ordId };
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
        RegTester(nameof(userOrderController.DoPay_RiderCollect), args =>
        {
            var (isSuccess, message) = ActiveOrderSo.PaymentRiderCollect();
            return new object[] { isSuccess, message };
        }, userOrderController);
        RegTester(nameof(userOrderController.DoPay_DeductFromCredit), args =>
        {
            var (isSuccess, message) = ActiveOrderSo.PaymentCreditDeduction();
            return new object[] { isSuccess, message };
        }, userOrderController);
        RegTester(nameof(userOrderController.Do_UpdateActives), _ =>
        {
            var message = ActiveOrderSo.GetOrders();
            return new object[] { message };
        }, userOrderController);        
        RegTester(nameof(userOrderController.Do_UpdateHistory), _ =>
        {
            var message = HistorySo.GetOrders();
            return new object[] { message };
        }, userOrderController);
    }

    private void RegLoginService()
    {
        var userLoginController = App.GetController<UserLoginController>();

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
        RegTester(nameof(riderOrderService.Do_State_Update), args =>
        {
            var order = (DeliverOrderModel)args[0];
            var stateId = (int)args[1];
            var (bag, statedID) = ActiveOrderSo.DoStateUpdate(order, stateId);
            return new object[] { bag, statedID };
        },riderOrderService);
        RegTester(nameof(riderOrderService.Get_SubStates), _ =>
        {
            var obj = string.Empty;
            return new object[] { obj };
        }, riderOrderService);
        RegTester(nameof(riderOrderService.Do_AssignRider), args =>
        {
            var order = (DeliverOrderModel)args[0];
            var orderWithRider = RiderLoginServiceSo.SetRiderInfo(order);
            var data = DataBag.Deserialize(orderWithRider);
            var newData = data.Get<DeliverOrderModel>(0);
            var (isSuccess, bag) = ActiveOrderSo.OrderAssigned(newData);
            return new object[] { isSuccess, bag};
        }, riderOrderService);
        RegTester(nameof(riderOrderService.Do_Get_Unassigned),  _=>
        {
            var message = ActiveOrderSo.GetOrders();
            return new object[] { message };
        }, riderOrderService);
        RegTester(nameof(riderOrderService.Do_Get_Assigned), _ =>
        {
            var message = ActiveOrderSo.GetOrders();
            return new object[] { message };
        }, riderOrderService);
        RegTester(nameof(riderOrderService.Do_Get_History), _ =>
        {
            var message = HistorySo.GetOrders();
            return new object[] { message };
        }, riderOrderService);
    }

    private void RegTester(string method, Func<object[], object[]> func, ControllerBase controller) => controller.RegTester(func, method);
}
#endif