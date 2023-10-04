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

        public void List_Set(ICollection<DeliverOrderModel> orders) =>
            Models.OrderCollection.SetOrders(orders.Select(o => new DeliveryOrder(o)).ToArray());
        public void List_Remove(DeliveryOrder order) => Models.OrderCollection.RemoveOrder(order);
        public void List_Clear() => Models.OrderCollection.ClearOrders();
        protected void SetCurrent(DeliveryOrder order) => Models.SetCurrentOrder(order);

        public void List_Update()
        {
            Call(args => (string)args[0], arg =>
            {
                var bag = DataBag.Deserialize(arg);
                List<DeliverOrderModel> list = bag.Get<List<DeliverOrderModel>>(0);
                List_Set(list);
            }, () =>
            {

            });
        }
    }

    public class UserOrderController : OrderControllerBase
    {
        private DeliveryOrder Current => Models.OrderCollection.Current;

        public void Do_Create(DeliverOrderModel order, Action<bool, string> callbackAction)
        {
            Call(args => ((bool)args[0], (string)args[1]),
                arg =>
            {
                var (isSuccess, message) = arg;
                if (isSuccess)
                {
                    DeserializeDeliveryOrderModel(message);
                    var model = new DeliveryOrder(order);
                    SetCurrent(model);
                    var list = Models.OrderCollection.Orders.ToList();
                    list.Add(model);
                    List_Clear();
                    List_Set(list.ToArray());
                    Do_UpdateAll();
                    message = string.Empty;
                }
                callbackAction(isSuccess, message);
            },
                () =>
            {
                var dto = order;
                ApiPanel.CreateDeliveryOrder(dto, doModel =>
                {
                    SetCurrent(new DeliveryOrder(doModel));
                    callbackAction?.Invoke(true, string.Empty);
                }, msg =>
                {
                    callbackAction?.Invoke(false, msg);
                });
            });
        }

        private void DeserializeDeliveryOrderModel(string message)
        {
            var bag = DataBag.Deserialize(message);
            var order = bag.Get<DeliverOrderModel>(0);
            ReadOnlySetOrder(order);
        }

        private void ReadOnlySetOrder(DeliverOrderModel order)
        {
            Models.SetCurrentOrder(order);
        }

        public void Do_Payment(PaymentMethods payment, Action<bool, string> callbackAction)
        {
            Call(args => ((bool)args[0], (string)args[1]), arg =>
            {
                var (success, message) = arg;
                if (success)
                {
                    Current.SetPaymentMethod(payment);
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
            #region TestMode
            if (TestMode)
            {
                return;
            }
            #endregion

            ApiPanel.User_GetDeliveryOrders(50, page, dtos =>
            {
                List_Clear();
                List_Set(dtos);
            }, msg => MessageWindow.Set("Error", msg));
        }
        
        public void Do_RequestCancel(int orderId, Action<bool> callbackAction)
        {
            Call(args => ((bool)args[0], (DeliveryOrderStatus)args[1]), arg =>
            {
                var (success, status) = arg;
                if (success)
                {
                    var o = Models.OrderCollection.GetOrder(orderId);
                    o.Status = (int)status;
                    SetCurrent(o);
                }
                callbackAction(success);
            }, () =>
            {
                //todo : request cancel Api
            });
        }

        public void ViewOrder(int orderId)
        {
            var o = Models.OrderCollection.GetOrder(orderId);
            SetCurrent(o);
        }
    }
}