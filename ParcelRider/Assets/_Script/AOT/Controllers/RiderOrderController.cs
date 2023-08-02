using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.DataModel;
using AOT.Utl;
using AOT.Views;

namespace AOT.Controllers
{
    public class RiderOrderController : OrderControllerBase
    {
        private List<(string description, bool justCancel)> ExceptionOps { get; set; } =
            new List<(string description, bool justCancel)>();

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

        private DeliveryOrder GetOrder(string orderId) => Models.OrderCollection.GetOrder(orderId);

        public void PickItem(string orderId)
        {
            var o = GetOrder(orderId);
            // PickItem
            if (TestMode)
            {
                o.Status = (int)DeliveryOrder.States.Delivering;
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

        public void ItemCollection(string orderId)
        {
            // ItemCollection
            if(TestMode)
            {
                var o = GetOrder(orderId);
                o.Status = (int)DeliveryOrder.States.Collection;
                Do_UpdateAll();
            }
        }

        public void Complete(string orderId, Action callbackAction)
        {
            // Complete
            if (TestMode)
            {
                var o = GetOrder(orderId);
                o.Status = (int)DeliveryOrder.States.Complete;
                Do_UpdateAll();
                callbackAction?.Invoke();
            }
        }

        public void SetException(string orderId, int optionIndex)
        {
            var o = GetOrder(orderId);
            o.Status = (int)(ExceptionOps[optionIndex].justCancel
                ? DeliveryOrder.States.None
                : DeliveryOrder.States.Exception);
            Do_UpdateAll();
        }

        public void OrderException(string orderId, Action<string[]> callbackOptions)
        {
            var o = GetOrder(orderId);
            var status = (DeliveryOrder.States)o.Status;
            ExceptionOps.Clear();
            switch (status)
            {
                case DeliveryOrder.States.Wait:
                    ExceptionOps.AddRange(new[]
                    {
                        ("Package info does not match", false),
                        ("Package condition problem", false),
                        ("Address problem", false),
                        ("Customer cancel", true),
                        ("Rider cancel", true),
                    });
                    break;
                case DeliveryOrder.States.Delivering:
                    ExceptionOps.AddRange(new[]
                    {
                        ("Package exception", false),
                        ("Delivery exception", false),
                        ("Customer cancel", false),
                    });
                    break;
                case DeliveryOrder.States.Collection:
                    ExceptionOps.AddRange(new[]
                    {
                        ("Package exception", false),
                        ("Delivery exception", false),
                        ("Collector not found", false),
                    });
                    break;
                case DeliveryOrder.States.Exception:
                case DeliveryOrder.States.Complete:
                case DeliveryOrder.States.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status.ToString());
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

            ApiPanel.Rider_GetDeliveryOrders(50, page, bag =>
            {
                Models.OrderCollection.ClearOrders();
                Models.SetOrderList(bag.Select(o => new DeliveryOrder(o)).ToList());
            }, msg =>
            {
                MessageWindow.Set("Error", msg);
            });
        }

        public void Do_AssignRider(string orderId)
        {
            #region TestMode
            if (TestMode)
            {
                var o = Models.OrderCollection.GetOrder(orderId);
                o.Rider = App.Models.Rider.ToEntity();
                o.Status = (int)DeliveryOrder.States.Wait;
                Models.OrderCollection.UpdateOrder(o);
                return;
            }
            #endregion
            var id = int.Parse(orderId);
            ApiPanel.Rider_AssignRider(id, dto =>
            {
                var o = Models.OrderCollection.GetOrder(orderId);
                o.Status = (int)dto.Status;
                Do_UpdateAll();
            }, msg => MessageWindow.Set("Error", msg));
        }
    }
}