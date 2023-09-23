using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.DataModel;
using AOT.Model;
using AOT.Test;
using AOT.Utl;
using AOT.Views;
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
    }

    public class UserOrderController : OrderControllerBase
    {
        private DeliveryOrder Current => Models.OrderCollection.Current;

        public void Do_Create(DeliverOrderModel order, Action<bool, string> callbackAction)
        {

            #region TestMode

            if (TestMode)
            {
                var model = new DeliveryOrder(order);
                SetCurrent(model);
                var list = Models.OrderCollection.Orders.ToList();
                list.Add(model);
                List_Clear();
                List_Set(list.ToArray());
                Do_UpdateAll();
                callbackAction(true, string.Empty);
                return;
            }

            #endregion

            var dto = order;
            ApiPanel.CreateDeliveryOrder(dto, doModel =>
            {
                SetCurrent(new DeliveryOrder(doModel));
                callbackAction?.Invoke(true, string.Empty);
            }, msg =>
            {
                callbackAction?.Invoke(false, msg);
            });
        }

        public void Do_Payment(PaymentMethods payment, Action<bool, string> callbackAction)
        {
            #region TestMode
            if(TestMode)
            {
                Current.SetPaymentMethod(payment);
                callbackAction(true, string.Empty);
                return;
            }
            #endregion
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
            #region TestMode
            if (TestMode)
            {
                var o = Models.OrderCollection.GetOrder(orderId);
                o.Status = (int)DeliveryOrderStatus.Exception;
                SetCurrent(o);
                callbackAction(true); //todo
                return;
            }
            #endregion
            //todo : request cancel Api
        }

        public void ViewOrder(int orderId)
        {
            var o = Models.OrderCollection.GetOrder(orderId);
            SetCurrent(o);
        }
    }
}