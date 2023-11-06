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

namespace AOT.Controllers
{
    public class OrderControllerBase : ControllerBase
    {
        public const float KgToPounds = 2.2046226218f;
        public const float MeterToFeet = 3.280839895f;
        protected AppModels Models => App.Models;

        public void List_ActiveOrder_Set(ICollection<DeliverOrderModel> orders) =>
            Models.ActiveOrders.SetOrders(orders.Select(o => new DeliveryOrder(o)).ToArray());
        public void List_HistoryOrderSet(ICollection<DeliverOrderModel> orders) =>
            Models.History.SetOrders(orders.Select(o => new DeliveryOrder(o)).ToArray());
        public void List_Remove(DeliveryOrder order) => Models.ActiveOrders.RemoveOrder(order.Id);
        protected void SetActiveCurrent(DeliveryOrder order) => Models.ActiveOrders.SetCurrent(order.Id);

        public void List_Update()
        {
            Call(null, args => (string)args[0], arg =>
            {
                var bag = DataBag.Deserialize(arg);
                List<DeliverOrderModel> list = bag.Get<List<DeliverOrderModel>>(0);
                List_ActiveOrder_Set(list);
            }, () =>
            {

            });
        }
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
                    var list = Models.ActiveOrders.Orders.ToList();
                    list.Add(model);
                    List_ActiveOrder_Set(list.ToArray());
                    SetActiveCurrent(model);
                    //Do_UpdateAll();
                    message = string.Empty;
                }
                callbackAction(isSuccess, message);
            },
                () =>
            {
                var dto = order;
                ApiPanel.CreateDeliveryOrder(dto, doModel =>
                {
                    SetActiveCurrent(new DeliveryOrder(doModel));
                    callbackAction?.Invoke(true, string.Empty);
                }, msg =>
                {
                    callbackAction?.Invoke(false, msg);
                });
            });
        }

        public void Do_Payment(PaymentMethods payment, Action<bool, string> callbackAction)
        {
            Call(new object[] { payment },args => ((bool)args[0], (string)args[1], (PaymentMethods)args[2]), arg =>
            {
                var (success, message, payMethod) = arg;
                if (success)
                {
                    var current = App.Models.ActiveOrders.GetCurrent();
                    current.SetPaymentMethod(payMethod);
                    message = string.Empty;
                }
                callbackAction(success, message);
            }, () =>
            {
                //todo : request payment Api
            });
        }

        public void Do_UpdateAll(int page = 1)
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
                ApiPanel.User_GetDeliveryOrders(50, page, dtos =>
                {
                    List_ActiveOrder_Set(dtos);
                }, msg => MessageWindow.Set("Error", msg));
            });
        }
        public void Do_UpdateHistory(int page = 1)
        {
            Call(args => (string)args[0], arg =>
            {
                var bag = DataBag.Deserialize(arg);
                List<DeliverOrderModel> list = bag.Get<List<DeliverOrderModel>>(0);
                List_HistoryOrderSet(list);
            }, () =>
            {
                //todo : request history api
            });
        }

        public void Do_RequestCancel(string orderId, Action<bool> callbackAction)
        {
            Call(new object[] {orderId},args => ((bool)args[0], (DeliveryOrderStatus)args[1], args[2].ToString()), arg =>
            {
                var (success, status, ordId) = arg;
                if (success)
                {
                    var o = Models.ActiveOrders.GetOrder(ordId);
                    o.Status = ((int)status);
                    SetActiveCurrent(o);
                    var h = App.Models.History.Orders.ToList();
                    h.Add(o);
                    List_HistoryOrderSet(h.ToArray());
                }
                callbackAction(success);
            }, () =>
            {
                //todo : request cancel Api
            });
        }

        public void ViewOrder(string orderId)
        {
            var o = Models.ActiveOrders.GetOrder(orderId);
            SetActiveCurrent(o);
        }

        public void ViewHistory(string orderId)
        {
            Models.History.SetCurrent(orderId);
        }
    }
}