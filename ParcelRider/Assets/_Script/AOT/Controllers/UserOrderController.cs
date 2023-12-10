using AOT.Core;
using AOT.DataModel;
using AOT.Model;
using AOT.Test;
using AOT.Utl;
using AOT.Views;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using OrderHelperLib.Dtos.Lingaus;
using UnityEngine;

namespace AOT.Controllers
{
    public class OrderControllerBase : ControllerBase
    {
        public const float KgToPounds = 2.2046226218f;
        public const float MeterToFeet = 3.280839895f;
        protected AppModels AppModel => App.Models;

        public void List_ActiveOrder_Set(ICollection<DeliverOrderModel> orders) =>
            AppModel.AssignedOrders.SetOrders(orders.Select(o => new DeliveryOrder(o)).ToArray());
        public void List_HistoryOrderSet(ICollection<DeliverOrderModel> orders) =>
            AppModel.History.SetOrders(orders.Select(o => new DeliveryOrder(o)).ToArray());

#if UNITY_EDITOR
        protected bool IsTestMode([CallerMemberName] string methodName = null)
        {
            if (!App.IsTestMode) return false;
            Debug.LogWarning($"记得写[{methodName}]的So");
            return true;
        }
#endif
    }

    public class UserOrderController : OrderControllerBase
    {
        public void Do_Create(DeliverOrderModel order, Action<bool, string> callbackAction)
        {
            Call(new object[] { order }, args => ((bool)args[0], (string)args[1]),
                arg =>
                {
                    var (isSuccess, message) = arg;
                    if (isSuccess)
                    {
                        var bag = DataBag.Deserialize(message);
                        var dOrder = bag.Get<DeliveryOrder>(0);
                        var model = new DeliveryOrder(dOrder);
                        Resolve_Orders(model);
                        Do_SetCurrent(model.Id);
                        message = string.Empty;
                    }

                    callbackAction(isSuccess, message);
                },
                () =>
                {
                    ApiPanel.CreateDeliveryOrder(order, doModel =>
                    {
                        var m = new DeliveryOrder(doModel);
                        Resolve_Orders(m);
                        Do_SetCurrent(m.Id);
                        callbackAction?.Invoke(true, string.Empty);
                    }, msg =>
                    {
                        callbackAction?.Invoke(false, msg);
                    });
                });
        }

        private void Resolve_Orders(DeliveryOrder model) => AppModel.Resolve_Order(model, isRider: false);

        public void Do_UpdateAll(int pageIndex = 0)
        {
            Call(args => args[0], arg =>
                {
                    var bag = DataBag.Deserialize(arg);
                    var list = bag.Get<List<DeliverOrderModel>>(0);
                    List_ActiveOrder_Set(list.ToArray());
                    return;
                },
                () => ApiPanel.User_GetDeliveryOrders(50, pageIndex, pg => List_ActiveOrder_Set(pg.List),
                    msg => MessageWindow.Set("Error", "Error in updating data!")));
        }

        public void Do_UpdateHistory(int pageIndex = 0)
        {
            Call(args => (string)args[0], arg =>
            {
                var bag = DataBag.Deserialize(arg);
                List<DeliverOrderModel> list = bag.Get<List<DeliverOrderModel>>(0);
                List_HistoryOrderSet(list);
            }, () =>
            {
                ApiPanel.User_GetHistories(50, pageIndex, pg => List_HistoryOrderSet(pg.List),
                    msg => MessageWindow.Set("Error", "Error in updating data!"));
            });
        }

        public void Do_RequestCancel(long orderId)
        {
            Call(new object[] { orderId }, args => ((bool)args[0], (DeliveryOrderStatus)args[1], (long)args[2]), arg =>
            {
                var (success, status, ordId) = arg;
                if (success)
                {
                    var o = AppModel.AssignedOrders.GetOrder(ordId);
                    o.Status = ((int)status);
                    o.SubState = DoSubState.SenderCancelState;
                    Resolve_Orders(o);
                    Do_SetCurrent(o.Id);
                }
            }, () =>
            {
                var o = AppModel.AssignedOrders.GetOrder(orderId);
                ApiPanel.CancelDeliveryOrder(orderId, o.SubState, (success, bag, message) =>
                {
                    if (success)
                    {
                        var order = bag.Get<DeliverOrderModel>(0);
                        Resolve_Orders(new DeliveryOrder(order));
                        Do_SetCurrent(orderId);
                        return;
                    }
                    MessageWindow.Set("Error", message);
                });
            });
        }

        public void Get_SubStates()
        {
            if (IsTestMode()) return;
            ApiPanel.User_GetSubStates(b =>
            {
                var subStates = b.Get<DoSubState[]>(0);
                AppModel.SetSubStates(subStates);
            }, msg => MessageWindow.Set("Error", "Error in updating data!"));
        }

        public void Logout()
        {
            AppModel.Reset();
            App.SendEvent(EventString.User_Logout);
        }

        public void Do_SetCurrent(long orderId) => App.Models.SetCurrentOrder(orderId);

        public void DoPay_RiderCollect(Action<bool, string> callbackAction)
        {
            if (IsTestMode()) return;
            var orderId = AppModel.CurrentOrder.Id;
            ApiPanel.User_DoPay_Rider(orderId, b =>
                {
                    var order = b.Get<DeliverOrderModel>(0);
                    Resolve_Orders(new DeliveryOrder(order));
                    callbackAction(true, string.Empty);
                },
                message => callbackAction(false, message));
        }

        public void DoPay_DeductFromCredit(Action<bool, string> callbackAction)
        {
            if (IsTestMode()) return;
            var orderId = AppModel.CurrentOrder.Id;
            ApiPanel.User_DoPay_Credit(orderId, b =>
            {
                var lingau = b.Get<LingauModel>(0);
                AppModel.SetUserLingau(lingau);
            }, message => callbackAction(false, message));
        }
    }
}