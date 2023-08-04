namespace OrderHelperLib.Dtos.Lingaus;

/// <summary>
/// 令凹币
/// </summary>
public record LingauModel : IntDto
{
    /// <summary>
    /// 余额
    /// </summary>
    public float Credit { get; set; }
}