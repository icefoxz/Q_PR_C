namespace OrderHelperLib.DtoModels.DeliveryOrders
{
    public enum DeliveryOrderStatus
    {
        Created, // 等待接单
        Accepted, // 已接单
        InProgress, // 配送中
        Delivered, // 已送达
        Canceled, // 已取消
        Exception, // 异常
        Closed, // 关闭
    }
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
        public int? DeliveryManId { get; set; }
        public DeliveryManDto DeliveryMan { get; set; }
        public DeliveryOrderStatus Status { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
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
        public float Weight { get; set; }
        public float Price { get; set; }
    }

    public class ReceiverInfoDto
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class DeliveryManDto
    {
        public string Location { get; set; }
        public bool IsWorking { get; set; }
        public string UserId { get; set; }
    }
}