using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DataModel;

namespace Controllers
{
    internal class PackageController : IController
    {
        public const float KgToPounds = 2.2046226218f;
        public const float MeterToFeet = 3.280839895f;
        public DeliveryOrder Current { get; private set; }
        private List<DeliveryOrder> OrderList { get; set; } = new List<DeliveryOrder>();
        public IReadOnlyList<DeliveryOrder> Orders => OrderList;

        public void CreatePackage(DeliveryOrder order) => Current = order;
        public void AddCurrentOrder()
        {
            if (Current == null)
                throw new NullReferenceException($"Current order is null!");
            OrderList.Add(Current);
            Current = null;
        }

        public void AddOrder(params DeliveryOrder[] orders) => OrderList.AddRange(orders);
        public void RemoveOrder(DeliveryOrder order) => OrderList.Remove(order);
        public void ClearOrders() => OrderList.Clear();

        public DeliveryOrder GetOrder(int orderId) => OrderList.FirstOrDefault(o => o.Id == orderId);
    }
}