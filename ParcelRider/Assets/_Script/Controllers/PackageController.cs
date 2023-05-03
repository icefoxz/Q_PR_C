using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DataModel;
using OrderHelperLib.DtoModels.DeliveryOrders;
using Utl;

namespace Controllers
{
    public class PackageController : IController
    {
        public const float KgToPounds = 2.2046226218f;
        public const float MeterToFeet = 3.280839895f;
        public DeliveryOrder Current { get; private set; }
        private List<DeliveryOrder> OrderList { get; set; } = new List<DeliveryOrder>();
        public IReadOnlyList<DeliveryOrder> Orders => OrderList;

        public void CreatePackage(DeliveryOrder order, Action<bool> callbackAction)
        {
            Current = order;
            var p = order.Package;
            var item = new ItemInfoDto
            {
                Weight = p.Weight,
                Width = p.Width,
                Height = p.Height,
                Length = p.Length,
                Quantity = 1
            };
            var from = GetCoordinate(order.From);
            var to = GetCoordinate(order.To);
            var deliveryInfo = new DeliveryInfoDto
            {
                Distance = p.Distance,
                Weight = p.Weight,
                Price = p.Price
            };
            var receiverInfo = new ReceiverInfoDto
            {
                Name = order.To.Name,
                PhoneNumber = order.To.Phone
            };
            var deliveryOrder = new DeliveryOrderDto
            {
                DeliveryInfo = deliveryInfo,
                StartCoordinates = from,
                EndCoordinates = to,
                ItemInfo = item,
                ReceiverInfo = receiverInfo,
            };
            ApiPanel.CreateDeliveryOrder(deliveryOrder, bag =>
            {
                Current = new DeliveryOrder(bag);
                callbackAction?.Invoke(true);
            }, msg =>
            {
                Current = null;
                callbackAction?.Invoke(false);
            });

            CoordinatesDto GetCoordinate(IdentityInfo info)
            {
                var dto = new CoordinatesDto();
                dto.Address = info.Address;
                return dto;
            }
        }


        public void AddCurrentOrder()
        {
            if (Current == null)
                throw new NullReferenceException("Current order is null!");
            OrderList.Add(Current);
            Current = null;
        }

        public void AddOrder(params DeliveryOrder[] orders) => OrderList.AddRange(orders);
        public void RemoveOrder(DeliveryOrder order) => OrderList.Remove(order);
        public void ClearOrders() => OrderList.Clear();

        public DeliveryOrder GetOrder(string orderId) => OrderList.FirstOrDefault(o => o.Id == orderId);
    }
}