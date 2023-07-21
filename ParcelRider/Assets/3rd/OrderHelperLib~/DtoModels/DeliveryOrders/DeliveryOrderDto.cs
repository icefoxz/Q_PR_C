using OrderHelperLib.Contracts;
using OrderHelperLib.DtoModels.Users;

namespace OrderHelperLib.DtoModels.DeliveryOrders
{
    public class DeliveryOrderDto
    {
        public string Id { get; set; }
        public UserDto User { get; set; }
        public ItemInfoDto ItemInfo { get; set; }
        public CoordinatesDto StartCoordinates { get; set; }
        public CoordinatesDto EndCoordinates { get; set; }
        public string ReceiverUserId { get; set; }
        public ReceiverInfoDto ReceiverInfo { get; set; }
        public DeliveryInfoDto DeliveryInfo { get; set; }
        public MyStates MyState { get; set; }
        public int? RiderId { get; set; }
        public RiderDto? Rider { get; set; }
        public PaymentInfoDto PaymentInfo { get; set; }
        public DeliveryOrderStatus Status { get; set; }
    }

    public class PaymentInfoDto
    {
        public float Price { get; set; } // 价格
        public PaymentMethods PaymentMethod { get; set; } // 付款类型
        /// <summary>
        /// 付款Reference,如果骑手代收将会是骑手Id, 如果是在线支付将会是支付平台的Reference, 如果是用户扣账将会是用户Id
        /// </summary>
        public string? PaymentReference { get; set; }
        public bool PaymentReceived { get; set; } // 是否已经完成付款
    }

    public class LingauDto
    {
        public float Credit { get; set; }
    }

    public class CoordinatesDto
    {
        public string Address { get; set; } // 地址
        public double Latitude { get; set; } // 纬度坐标
        public double Longitude { get; set; } // 经度坐标
    }

    public class ItemInfoDto
    {
        public float Weight { get; set; }
        public int Quantity { get; set; }
        public float Length { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string Remark { get; set; }
    }

    public class DeliveryInfoDto
    {
        public float Distance { get; set; }
        public float Price { get; set; }
    }

    public class ReceiverInfoDto
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class RiderDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Location { get; set; }
    }
}