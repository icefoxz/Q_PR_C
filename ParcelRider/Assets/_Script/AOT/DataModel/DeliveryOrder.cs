using System;
using OrderHelperLib.Contracts;
using OrderHelperLib.DtoModels.DeliveryOrders;

namespace AOT.DataModel
{
    /// <summary>
    /// 运算服务订单
    /// </summary>
    public class DeliveryOrder : Order
    {
        private static int IdSeed { get; set; } = 123000;
        public enum States
        {
            None,
            Wait,
            Delivering,
            Collection,
            Complete = -1,
            Exception = -2,
        }

        public IdentityInfo From { get; set; }
        public IdentityInfo To { get; set; }
        public PackageInfo Package { get; set; }
        public Rider Rider { get; set; } 
        public int Status { get; set; }
        public int PaymentMethod { get; set; }

        public DeliveryOrder()
        {
            Id = IdSeed++.ToString();
        }

        public DeliveryOrder(DeliveryOrderDto dto)
        {
            Id = dto.Id;
            From = new IdentityInfo(dto.User.Phone, dto.User.Name, dto.StartCoordinates.Address);
            To = new IdentityInfo(dto.ReceiverInfo.PhoneNumber, dto.ReceiverInfo.Name, dto.EndCoordinates.Address);
            Package = new PackageInfo(dto.ItemInfo.Weight, dto.DeliveryInfo.Price, dto.DeliveryInfo.Distance,
                dto.ItemInfo.Length, dto.ItemInfo.Width, dto.ItemInfo.Height);
            Rider = dto.Rider == null ? null : new Rider(dto.Rider);
            Status = (int)dto.Status;
        }

        public DeliveryOrderDto ToDto()
        {
            var p = Package;
            var item = new ItemInfoDto
            {
                Weight = p.Weight,
                Width = p.Width,
                Height = p.Height,
                Length = p.Length,
                Quantity = 1
            };
            var from = GetCoordinate(From);
            var to = GetCoordinate(To);
            var deliveryInfo = new DeliveryInfoDto
            {
                Distance = p.Distance,
                Price = p.Price
            };
            var receiverInfo = new ReceiverInfoDto
            {
                Name = To.Name,
                PhoneNumber = To.Phone
            };
            return new DeliveryOrderDto
            {
                DeliveryInfo = deliveryInfo,
                StartCoordinates = from,
                EndCoordinates = to,
                ItemInfo = item,
                ReceiverInfo = receiverInfo,
            };

            CoordinatesDto GetCoordinate(IdentityInfo info)
            {
                var dto = new CoordinatesDto();
                dto.Address = info.Address;
                return dto;
            }
        }

        public void SetPaymentMethod(PaymentMethods payment) => PaymentMethod = (int)payment;
    }

    public class IdentityInfo
    {
        public string Phone { get; }
        public string Name { get; }
        public string Address { get; }

        public IdentityInfo(string phone, string name, string address)
        {
            Phone = phone;
            Name = name;
            Address = address;
        }
    }

    public class PackageInfo
    {
        public float Weight { get; }
        public float Price { get; }
        public float Distance { get; }
        public float Size => MathF.Pow(Length * Width * Height, 1f / 3f);
        public float Length { get; }
        public float Width { get; }
        public float Height { get; }

        public PackageInfo(float weight, float price, float distance, float length, float width, float height)
        {
            Weight = weight;
            Price = price;
            Distance = distance;
            Length = length;
            Width = width;
            Height = height;
        }
    }
}