using System;
using OrderHelperLib.DtoModels.DeliveryOrders;

namespace DataModel
{
    /// <summary>
    /// 运算服务订单
    /// </summary>
    public class DeliveryOrder : Order
    {
        public enum States
        {
            None,
            Wait,
            Delivering,
            Collection,
            Complete,
            Exception,
        }

        public IdentityInfo From { get; set; }
        public IdentityInfo To { get; set; }
        public PackageInfo Package { get; set; }
        public int? DeliveryManId { get; set; }
        public int Status { get; set; }

        public DeliveryOrder()
        {

        }

        public DeliveryOrder(DeliveryOrderDto dto)
        {
            Id = dto.Id;
            From = new IdentityInfo(dto.User.PhoneNumber, dto.User.Name, dto.StartCoordinates.Address);
            To = new IdentityInfo(dto.ReceiverInfo.PhoneNumber, dto.ReceiverInfo.Name, dto.EndCoordinates.Address);
            Package = new PackageInfo(dto.ItemInfo.Weight, dto.DeliveryInfo.Price, dto.DeliveryInfo.Distance,
                dto.ItemInfo.Length, dto.ItemInfo.Width, dto.ItemInfo.Height);
            DeliveryManId = dto.DeliveryManId;
            Status = (int)dto.Status;
        }
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