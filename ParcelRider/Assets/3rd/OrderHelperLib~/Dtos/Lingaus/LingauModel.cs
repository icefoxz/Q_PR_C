namespace OrderHelperLib.Dtos.Lingaus;

/// <summary>
/// 令凹币
/// </summary>
public record LingauModel : Dto<string>
{
    /// <summary>
    /// 余额
    /// </summary>
    public float Credit { get; set; }
}