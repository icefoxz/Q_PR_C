﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DataModel;
using OrderHelperLib.Contracts;

namespace Controllers
{
    public class OrderController : IController
    {
        public const float KgToPounds = 2.2046226218f;
        public const float MeterToFeet = 3.280839895f;
        public DeliveryOrder Current { get; private set; }
        private List<DeliveryOrder> OrderList { get; set; } = new List<DeliveryOrder>();
        public IReadOnlyList<DeliveryOrder> Orders => OrderList;

        public void SetCurrent(DeliveryOrder order) => Current = order;
        public void CreatePackage(PaymentMethods payment,Action<bool> callbackAction)
        {
            Current.SetPaymentMethod(payment);
            var dto = Current.ToDto();
            AddOrder(Current);
            callbackAction(true);//todo
            //ApiPanel.CreateDeliveryOrder(dto, bag =>
            //{
            //    Current = new DeliveryOrder(bag);
            //    callbackAction?.Invoke(true);
            //}, msg =>
            //{
            //    Current = null;
            //    callbackAction?.Invoke(false);
            //});
        }

        public void AddOrder(params DeliveryOrder[] orders)
        {
            OrderList.AddRange(orders);
            OrderList = OrderList.OrderBy(o => o.Status).ToList();
            UpdateEvent();
        }

        public void RemoveOrder(DeliveryOrder order)
        {
            OrderList.Remove(order);
            UpdateEvent();
        }

        public void ClearOrders()
        {
            OrderList.Clear();
            UpdateEvent();
        }

        public DeliveryOrder GetOrder(string orderId) => OrderList.FirstOrDefault(o => o.Id == orderId);

        public void UpdateOrders(Action callbackAction,int page = 1)
        {
            callbackAction();//todo
            UpdateEvent();
            //ApiPanel.GetDeliveryOrders(50, page, bag =>
            //{
            //    OrderList = bag.Select(o => new DeliveryOrder(o)).ToList();
            //    callbackAction?.Invoke();
            //}, msg =>
            //{
            //    OrderList.Clear();
            //    callbackAction?.Invoke();
            //});
        }

        private static void UpdateEvent() => App.MessagingManager.Send(EventString.Orders_Update, string.Empty);

        public void AssignRider(string orderId, Action<bool> callbackAction)
        {
            var o = OrderList.First(o => o.Id == orderId);
            o.Status = (int)DeliveryOrder.States.Wait;
            UpdateEvent();
            callbackAction(true);//todo
            //var id = int.Parse(orderId);
            //ApiPanel.AssignRider(id, dto =>
            //{
            //    var o = OrderList.First(o => o.Id == orderId);
            //    o.Status = (int)dto.Status;
            //    callbackAction?.Invoke(true);
            //}, msg => callbackAction?.Invoke(false));
        }

        public void RequestCancelOrder(string orderId, Action<bool> callbackAction)
        {
            var o = OrderList.First(o => o.Id == orderId);
            o.Status = (int)DeliveryOrder.States.Exception;
            UpdateEvent();
            callbackAction(true);//todo
        }
    }
}