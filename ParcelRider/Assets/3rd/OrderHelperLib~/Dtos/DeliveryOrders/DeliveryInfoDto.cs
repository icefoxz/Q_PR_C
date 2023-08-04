namespace OrderHelperLib.Dtos.DeliveryOrders;

public record DeliveryInfoDto
{
    //出发地点
    public LocationDto StartLocation { get; set; }
    //目的地点
    public LocationDto EndLocation { get; set; }
    /// <summary>
    /// 距离, 单位是公里
    /// </summary>
    public float Distance { get; set; }
}