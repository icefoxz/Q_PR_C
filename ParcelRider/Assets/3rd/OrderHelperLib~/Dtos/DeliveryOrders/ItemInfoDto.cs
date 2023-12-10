namespace OrderHelperLib.Dtos.DeliveryOrders;

/// <summary>
/// 物品信息
/// </summary>
public record ItemInfoDto
{
    /// <summary>
    /// 重量, 单位是kg
    /// </summary>
    public float Weight { get; set; }
    /// <summary>
    /// 物件数量, 常规来说是一个包裹. 但有时候客户会有多个物件, 这时候就需要填写数量
    /// </summary>
    public int Quantity { get; set; }
    /// <summary>
    /// 体积, 长*宽*高
    /// </summary>
    public double Volume { get; set; }
    /// <summary>
    /// 价值
    /// </summary>
    public double Value{get; set; }
    /// <summary>
    /// 长, 单位是米
    /// </summary>
    public float Length { get; set; }
    /// <summary>
    /// 宽, 单位是米
    /// </summary>
    public float Width { get; set; }
    /// <summary>
    /// 高, 单位是米
    /// </summary>
    public float Height { get; set; }
    /// <summary>
    /// 附加信息
    /// </summary>
    public string? Remark { get; set; }
}