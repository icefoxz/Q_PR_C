using System;
using System.Linq;
using AOT.Core;
using AOT.DataModel;
using AOT.Model;
using AOT.Test;
using AOT.Utl;
using AOT.Views;
using OrderHelperLib.Contracts;

namespace AOT.Controllers
{
    public class OrderControllerBase : ControllerBase
    {
        public const float KgToPounds = 2.2046226218f;
        public const float MeterToFeet = 3.280839895f;
        protected AppModels Models => App.Models;
        
        public void List_Set(params DeliveryOrder[] orders) => Models.OrderCollection.SetOrders(orders);
        public void List_Remove(DeliveryOrder order) => Models.OrderCollection.RemoveOrder(order);
        public void List_Clear() => Models.OrderCollection.ClearOrders();
        public void SetCurrent(DeliveryOrder order) => Models.SetCurrentOrder(order);
    }

    public class UserOrderController : OrderControllerBase
    {
        private DeliveryOrder Current => Models.OrderCollection.Current;

        public void Do_Create(DeliveryOrder order, Action<bool, string> callbackAction)
        {

            #region TestMode

            if (TestMode)
            {
                SetCurrent(order);
                var list = Models.OrderCollection.Orders.ToList();
                list.Add(order);
                List_Clear();
                List_Set(list.ToArray());
                Do_UpdateAll();
                callbackAction(true, string.Empty);
                return;
            }

            #endregion

            var dto = order.ToDto();
            ApiPanel.CreateDeliveryOrder(dto, bag =>
            {
                SetCurrent(new DeliveryOrder(bag));
                callbackAction?.Invoke(true, string.Empty);
            }, msg =>
            {
                SetCurrent(null);
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

            ApiPanel.User_GetDeliveryOrders(50, page, bag =>
            {
                var orders = bag.Select(o => new DeliveryOrder(o)).ToList();
                List_Clear();
                List_Set(orders.ToArray());
            }, msg => MessageWindow.Set("Error", msg));
        }
        
        public void Do_RequestCancel(string orderId, Action<bool> callbackAction)
        {
            #region TestMode
            if (TestMode)
            {
                var o = Models.OrderCollection.GetOrder(orderId);
                o.Status = (int)DeliveryOrder.States.Exception;
                SetCurrent(o);
                callbackAction(true); //todo
                return;
            }
            #endregion
            //todo : request cancel Api
        }
    }
}