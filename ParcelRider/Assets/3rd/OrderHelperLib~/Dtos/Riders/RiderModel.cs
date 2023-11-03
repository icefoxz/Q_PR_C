namespace OrderHelperLib.Dtos.Riders;

public record RiderModel : StringDto
{
    /// <summary>
    /// 名字
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 手机
    /// </summary>
    public string? Phone { get; set; }
}