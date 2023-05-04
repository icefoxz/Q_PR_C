using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DataModel;
using Utl;

namespace Controllers
{
    public class RiderController : IController
    {
        private PackageController PackageController => App.GetController<PackageController>();
        private List<(string description, bool justCancel)> ExceptionOps { get; set; } = new List<(string description, bool justCancel)>();

        public void RiderApplication(Action<bool> callbackAction)
        {
            // RiderApplication
            callbackAction(true);

            //ApiPanel.RegisterRider();
        }

        public void TakeOrder(string orderId, Action callbackAction)
        {
            // TakeOrder
            var o = PackageController.GetOrder(orderId);
            o.Status = (int)DeliveryOrder.States.Wait;
            o.DeliveryManId = Auth.RiderId;
            UpdateOrderEvent();
            callbackAction();
        }

        private static void UpdateOrderEvent() => App.MessagingManager.Send(EventString.Orders_Update, string.Empty);

        public void PickItem(string orderId, Action callbackAction)
        {
            // PickItem
            var o = PackageController.GetOrder(orderId);
            o.Status = (int)DeliveryOrder.States.Delivering;
            UpdateOrderEvent();
            callbackAction();
        }

        public void ItemCollection(string orderId, Action callbackAction)
        {
            // ItemCollection
            var o = PackageController.GetOrder(orderId);
            o.Status = (int)DeliveryOrder.States.Collection;
            UpdateOrderEvent();
            callbackAction();
        }

        public void Complete(string orderId, Action callbackAction)
        {
            // Complete
            var o = PackageController.GetOrder(orderId);
            o.Status = (int)DeliveryOrder.States.Complete;
            UpdateOrderEvent();
            callbackAction();
        }

        public void SetException(string orderId, int optionIndex, Action callbackAction)
        {
            var o = PackageController.GetOrder(orderId);
            o.Status = (int)(ExceptionOps[optionIndex].justCancel
                ? DeliveryOrder.States.None
                : DeliveryOrder.States.Exception);
            UpdateOrderEvent();
            callbackAction();
        }

        public void OrderException(string orderId, Action<string[]> callbackOptions)
        {
            var o = PackageController.GetOrder(orderId);
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
    }
}