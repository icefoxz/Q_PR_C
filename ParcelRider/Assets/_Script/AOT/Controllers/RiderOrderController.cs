using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.DataModel;
using AOT.Utl;
using AOT.Views;
using OrderHelperLib.Contracts;

namespace AOT.Controllers
{
    public class RiderOrderController : OrderControllerBase
    {
        private List<(string description, bool resetOrder)> ExceptionOps { get; set; } =
            new List<(string description, bool resetOrder)>();

        public void RiderApplication(Action<bool> callbackAction)
        {
            #region TestMode

            if (TestMode)
            {
                callbackAction(true);
                return;
            }

            #endregion

            // RiderApplication

            //ApiPanel.RegisterRider();
        }

        private DeliveryOrder GetOrder(int orderId) => Models.OrderCollection.GetOrder(orderId);

        public void PickItem(int orderId)
        {
            var o = GetOrder(orderId);
            // PickItem
            if (TestMode)
            {
                o.Status = (int)DeliveryOrderStatus.Delivering;
                SetCurrent(o);
                Do_UpdateAll();
                return;
            }

            ApiPanel.Rider_PickItem(o, dto =>
            {
                SetCurrent(new DeliveryOrder(dto));
                Do_UpdateAll();
            }, msg => MessageWindow.Set("order", msg));
        }

        public void ItemCollection(int orderId)
        {
            // ItemCollection
            if(TestMode)
            {
                var o = GetOrder(orderId);
                o.Status = (int)DeliveryOrderStatus.Delivering;
                Do_UpdateAll();
            }
        }

        public void Complete(int orderId, Action callbackAction)
        {
            // Complete
            if (TestMode)
            {
                var o = GetOrder(orderId);
                o.Status = (int)DeliveryOrderStatus.Completed;
                Do_UpdateAll();
                callbackAction?.Invoke();
            }
        }

        public void SetException(int orderId, int optionIndex)
        {
            var o = GetOrder(orderId);
            o.Status = (int)(ExceptionOps[optionIndex].resetOrder
                ? DeliveryOrderStatus.Created
                : DeliveryOrderStatus.Exception);
            Do_UpdateAll();
        }

        public void OrderException(int orderId, Action<string[]> callbackOptions)
        {
            var o = GetOrder(orderId);
            var status = (DeliveryOrderStatus)o.Status;
            ExceptionOps.Clear();
            switch (status)
            {
                case DeliveryOrderStatus.Created:
                    ExceptionOps.AddRange(new[]
                    {
                        ("Package info does not match", false),
                        ("Package condition problem", false),
                        ("Address problem", false),
                        ("Customer cancel", true),
                        ("Rider cancel", true),
                    });
                    break;
                case DeliveryOrderStatus.Assigned:
                    ExceptionOps.AddRange(new[]
                    {
                        ("Package exception", false),
                        ("Delivery exception", false),
                        ("Customer cancel", false),
                    });
                    break;
                case DeliveryOrderStatus.Delivering:
                    ExceptionOps.AddRange(new[]
                    {
                        ("Package exception", false),
                        ("Delivery exception", false),
                        ("Collector not found", false),
                    });
                    break;
                case DeliveryOrderStatus.Exception:
                case DeliveryOrderStatus.Canceled:
                case DeliveryOrderStatus.Completed:
                case DeliveryOrderStatus.Close:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            callbackOptions(ExceptionOps.Select(e => e.description).ToArray());
        }

        public void Do_UpdateAll(int page = 1)
        {
            #region TestMode
            if (TestMode)
            {
                Models.OrderCollection.UpdateOrder(Models.OrderCollection.Orders.First());
                return;
            }
            #endregion

            ApiPanel.Rider_GetDeliveryOrders(50, page, dtos =>
            {
                Models.OrderCollection.ClearOrders();
                Models.SetOrderList(dtos.Select(o => new DeliveryOrder(o)).ToList());
            }, msg =>
            {
                MessageWindow.Set("Error", msg);
            });
        }

        public void Do_AssignRider(int orderId)
        {
            #region TestMode
            if (TestMode)
            {
                var o = Models.OrderCollection.GetOrder(orderId);
                o.Rider = App.Models.Rider;
                o.Status = (int)DeliveryOrderStatus.Created;
                Models.OrderCollection.UpdateOrder(o);
                return;
            }
            #endregion
            var id = orderId;
            ApiPanel.Rider_AssignRider(id, dto =>
            {
                var o = Models.OrderCollection.GetOrder(orderId);
                o.Status = (int)dto.Status;
                Do_UpdateAll();
            }, msg => MessageWindow.Set("Error", msg));
        }

        public void ViewOrder(int orderId)
        {
            var o = Models.OrderCollection.GetOrder(orderId);
            SetCurrent(o);
        }
    }
}