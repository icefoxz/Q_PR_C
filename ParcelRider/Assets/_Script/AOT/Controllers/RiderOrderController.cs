﻿using System;
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

        private DeliveryOrder GetOrder(long orderId) => Models.AssignedOrders.GetCurrent();

        public void Get_SubStates() => ApiPanel.Rider_GetSubStates(b =>
        {
            var subStates = b.Get<DoSubState[]>(0);
            Models.SetSubStates(subStates);
        }, msg => MessageWindow.Set("Error", "Error in updating data!"));

        public void PickItem(long orderId)
        {
            var oo = GetOrder(orderId);
            Call(new object[] { orderId }, args => ((bool)args[0], (int)args[1], (long)args[2]), arg =>
            {
                var (success, status, oId) = arg;
                if (success)
                {
                    UpdateOrder(status, oId);
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

        public void ItemCollection(long orderId)
        {
            Call(new object[] { orderId }, args => ((bool)args[0], (int)args[1], (long)args[2]), arg =>
            {
                var (success, status, oId) = arg;
                UpdateOrder(status, oId);
                return;
            }, () =>
            {

            });
        }

        private void UpdateOrder(int status, long oId)
        {
            var o = GetOrder(oId);
            o.Status = status;
            SetActiveCurrent(o);
        }

        public void Complete(long orderId, Action callbackAction)
        {
            // Complete
            Call(new object[] { orderId }, args => ((bool)args[0], (int)args[1], (long)args[2]), arg =>
            {
                var (success, status, ordId) = arg;
                UpdateOrder(status, ordId);
                callbackAction?.Invoke();
            }, () =>
            {

            });
        }

        public void SetException(long orderId, int optionIndex)
        {
            var o = GetOrder(orderId);
            o.Status = (int)(ExceptionOps[optionIndex].resetOrder
                ? DeliveryOrderStatus.Created
                : DeliveryOrderStatus.Exception);
            Do_UpdateAll();
        }

        public void OrderException(long orderId, Action<string[]> callbackOptions)
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
                case DeliveryOrderStatus.Closed:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            callbackOptions(ExceptionOps.Select(e => e.description).ToArray());
        }

        public void Do_UpdateAll(int page = 1)
        {
            Call(args => args[0], arg =>
            {
                var message = arg;
                var bag = DataBag.Deserialize(message);
                var list = bag.Get<List<DeliverOrderModel>>(0);
                var toList = new List<DeliveryOrder>();
                List_ActiveOrder_Set(list);
                return;
            }, () =>
            {
                //ApiPanel.Rider_GetDeliveryOrders(50, page, pg =>
                //{
                //    Models.ActiveOrders.SetOrders(pg.List.Select(o => new DeliveryOrder(o)).ToList());
                //}, msg =>
                //{
                //    MessageWindow.Set("Error", msg);
                //}));
            });
        }

        public void Do_AssignRider(long orderId)
        {
            var order = GetOrder(orderId);
            Call(new object[] { order },args => ((bool)args[0], (int)args[1], (long)args[2]), arg =>
            {
                var (success, status, oId) = arg;
                UpdateOrder(status, oId);
                Do_UpdateAll();
                return;
            }, () =>
            {
                var id = orderId;
                ApiPanel.Rider_AssignRider(id, dto =>
                {
                    var o = Models.AssignedOrders.GetCurrent();
                    o.Status = dto.Status;
                    Do_UpdateAll();
                }, msg => MessageWindow.Set("Error", msg));
            });
        }

        public void ViewOrder(long orderId)
        {
            var o = Models.AssignedOrders.GetOrder(orderId);
            SetActiveCurrent(o);
        }
    }
}