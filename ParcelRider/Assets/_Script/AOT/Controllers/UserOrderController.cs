using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.DataModel;
using AOT.Model;
using AOT.Test;
using AOT.Utl;
using AOT.Views;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using WebUtlLib;

namespace AOT.Controllers
{
    public class OrderControllerBase : ControllerBase
    {
        public const float KgToPounds = 2.2046226218f;
        public const float MeterToFeet = 3.280839895f;
        protected AppModels Models => App.Models;

        public void List_ActiveOrder_Set(ICollection<DeliverOrderModel> orders) =>
            Models.AssignedOrders.SetOrders(orders.Select(o => new DeliveryOrder(o)).ToArray());
        public void List_HistoryOrderSet(ICollection<DeliverOrderModel> orders) =>
            Models.History.SetOrders(orders.Select(o => new DeliveryOrder(o)).ToArray());
        public void List_Remove(DeliveryOrder order) => Models.AssignedOrders.RemoveOrder(order.Id);
        protected void SetActiveCurrent(DeliveryOrder order) => Models.AssignedOrders.SetCurrent(order.Id);
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
                        AddToActiveList(model);
                        SetActiveCurrent(model);
                        //Do_UpdateAll();
                        message = string.Empty;
                    }

                    callbackAction(isSuccess, message);
                },
                () =>
                {
                    ApiPanel.CreateDeliveryOrder(order, doModel =>
                    {
                        var m = new DeliveryOrder(doModel);
                        AddToActiveList(m);
                        SetActiveCurrent(m);
                        callbackAction?.Invoke(true, string.Empty);
                    }, msg =>
                    {
                        callbackAction?.Invoke(false, msg);
                    });
                });

            void AddToActiveList(DeliveryOrder model)
            {
                var list = Models.AssignedOrders.Orders.ToList();
                list.Add(model);
                List_ActiveOrder_Set(list.ToArray());
            }
        }

        public void Do_Payment(PaymentMethods payment, Action<bool, string> callbackAction)
        {
            Call(new object[] { payment },args => ((bool)args[0], (string)args[1], (PaymentMethods)args[2]), arg =>
            {
                var (success, message, payMethod) = arg;
                if (success)
                {
                    var current = App.Models.AssignedOrders.GetCurrent();
                    current.SetPaymentMethod(payMethod);
                    message = string.Empty;
                }
                callbackAction(success, message);
            }, () =>
            {

            });
        }

        public void Do_UpdateAll(int pageIndex = 0)
        {
            Call(args => args[0], arg =>
                {
                    var bag = DataBag.Deserialize(arg);
                    var list = bag.Get<List<DeliverOrderModel>>(0);
                    List_ActiveOrder_Set(list.ToArray());
                    return;
                },
                () =>
                {
                    ApiPanel.User_GetDeliveryOrders(50, pageIndex, pg => List_ActiveOrder_Set(pg.List),
                        msg => MessageWindow.Set("Error", "Error in updating data!"));
                });
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

        public void Do_RequestCancel(long orderId, Action<bool> callbackAction)
        {
            Call(new object[] {orderId},args => ((bool)args[0], (DeliveryOrderStatus)args[1], (long)args[2]), arg =>
            {
                var (success, status, ordId) = arg;
                if (success)
                {
                    var o = Models.AssignedOrders.GetOrder(ordId);
                    o.Status = ((int)status);
                    SetActiveCurrent(o);
                    var h = App.Models.History.Orders.ToList();
                    h.Add(o);
                    List_HistoryOrderSet(h.ToArray());
                }
                callbackAction(success);
            }, () =>
            {
                var o = Models.AssignedOrders.GetOrder(orderId);
                ApiPanel.CancelDeliveryOrder(orderId, o.SubState ,(success, bag, message) =>
                {
                    if (success)
                    {
                        var orderPl = bag.Get<PageList<DeliverOrderModel>>(0);
                        var historyPl = bag.Get<PageList<DeliverOrderModel>>(1);
                        List_ActiveOrder_Set(orderPl.List);
                        List_HistoryOrderSet(historyPl.List);
                        var o = Models.AssignedOrders.GetOrder(orderId);
                        SetActiveCurrent(o);
                    }
                    callbackAction(success);
                });
            });
        }

        public void ViewOrder(long orderId)
        {
            var o = Models.AssignedOrders.GetOrder(orderId);
            SetActiveCurrent(o);
        }

        public void ViewHistory(long orderId)
        {
            Models.History.SetCurrent(orderId);
        }
    }
}