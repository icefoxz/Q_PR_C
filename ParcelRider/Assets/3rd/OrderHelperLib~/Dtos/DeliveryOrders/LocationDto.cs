namespace OrderHelperLib.Dtos.DeliveryOrders;

/// <summary>
/// 地点
/// </summary>
public record LocationDto
{
    public string? PlaceId { get; set; } // 地点Id
    public string? Address { get; set; } // 地址
    public double Latitude { get; set; } // 纬度坐标
    public double Longitude { get; set; } // 经度坐标
}