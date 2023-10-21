using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.DataModel;
using AOT.Utl;
using AOT.Views;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;

namespace AOT.Controllers
{
    public class RiderOrderController : OrderControllerBase
    {
        private List<(string description, bool resetOrder)> ExceptionOps { get; set; } =
            new List<(string description, bool resetOrder)>();

        public void RiderApplication(Action<bool> callbackAction)
        {
            //Call(args => (bool)args[0], arg =>
            //{
            //    var success = arg;
            //    callbackAction(success);
            //    return;
            //}, () =>
            //{
            //    // RiderApplication
            //
            //    //ApiPanel.RegisterRider();
            //});
        }

        private DeliveryOrder GetOrder(int orderId) => Models.ActiveOrders.GetCurrent();

        public void PickItem(int orderId)
        {
            var oo = GetOrder(orderId);
            // PickItem
            //if (TestMode)
            //{
            //    o.Status = (int)DeliveryOrderStatus.Delivering;
            //    SetActiveCurrent(o);
            //    Do_UpdateAll();
            //    return;
            //}
            Call(new object[] { oo }, args => ((bool)args[0], (string)args[1]), arg =>
            {
                var (success, message) = arg;
                if (success)
                {
                    var bag = DataBag.Deserialize(message);
                    var order = bag.Get<DeliveryOrder>(0);
                    oo.Status = order.Status;
                    SetActiveCurrent(oo);
                    Do_UpdateAll();
                }
                return;
            }, () =>
            {
                ApiPanel.Rider_PickItem(oo, dto =>
                {
                    SetActiveCurrent(new DeliveryOrder(dto));
                    Do_UpdateAll();
                }, msg => MessageWindow.Set("order", msg));
            });
        }

        public void ItemCollection(int orderId)
        {
            // ItemCollection
            //if(TestMode)
            //{
            //    var o = GetOrder(orderId);
            //    o.Status = (int)DeliveryOrderStatus.Delivering;
            //    Do_UpdateAll();
            //}
            Call(new object[] { orderId }, args => ((bool)args[0], (int)args[1], (int)args[2]), arg =>
            {
                var (success, status, oId) = arg;
                UpdateOrder(status, oId);
                return;
            }, () =>
            {

            });
        }

        private void UpdateOrder(int status, int oId)
        {
            var o = GetOrder(oId);
                //Models.ActiveOrders.GetCurrent;
            o.Status = status;
            SetActiveCurrent(o);
            Do_UpdateAll();
        }

        public void Complete(int orderId, Action callbackAction)
        {
            // Complete
            Call(new object[] { orderId }, args => ((int)args[0], (int)args[1]), arg =>
            {
                var (status, ordId) = arg;
                var o = Models.ActiveOrders.GetOrder(ordId);
                o.Status = status;
                SetActiveCurrent(o);
                var h = Models.History.Orders.ToList();
                h.Add(o);
                List_HistoryOrderSet(h.ToArray());
                callbackAction?.Invoke();
            }, () =>
            {

            });
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
            // #region TestMode
            // if (TestMode)
            // {
            //     Models.ActiveOrders.UpdateOrder(Models.ActiveOrders.Orders.First());
            //     return;
            // }
            // #endregion
            Call(args => args[0], arg =>
            {
                //var message = arg;
                //var bag = DataBag.Deserialize(message);
                //var model = bag.Get<DeliveryOrder>(0);
                //var list = new List<DeliveryOrder>();
                //list.Add(model);
                //Models.SetOrderList(list);
                var o = Models.ActiveOrders.Orders;
                if (o.Count != 0)
                    Models.ActiveOrders.UpdateOrder(o.First());
                return;
            }, () =>
            {
                ApiPanel.Rider_GetDeliveryOrders(50, page, dtos =>
                {
                    Models.SetOrderList(dtos.Select(o => new DeliveryOrder(o)).ToList());
                }, msg =>
                {
                    MessageWindow.Set("Error", msg);
                });
            });
        }

        public void Do_AssignRider(int orderId)
        {
            Call(new object[] { orderId },args => ((bool)args[0], (int)args[1], (int)args[2]), arg =>
            {
                var (success, status, oId) = arg;
                UpdateOrder(status, oId);
                return;
            }, () =>
            {
                var id = orderId;
                ApiPanel.Rider_AssignRider(id, dto =>
                {
                    var o = Models.ActiveOrders.GetCurrent();
                    o.Status = (int)dto.Status;
                    Do_UpdateAll();
                }, msg => MessageWindow.Set("Error", msg));
            });
        }

        public void ViewOrder(int orderId)
        {
            var o = Models.ActiveOrders.GetOrder(orderId);
            SetActiveCurrent(o);
        }
    }
}