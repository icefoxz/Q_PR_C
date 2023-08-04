using OrderHelperLib.Dtos.Users;

namespace OrderHelperLib.Dtos.DeliveryOrders;

/// <summary>
/// 收货员信息
/// </summary>
public record ReceiverInfoDto
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 手机号
    /// </summary>
    public string PhoneNumber { get; set; }
    /// <summary>
    /// 规范手机号
    /// </summary>
    public string NormalizedPhoneNumber { get; set; }
    // 收件人Id(如果有账号的话))
    public string? UserId { get; set; }
    //收件人(如果有账号的话)
    public UserModel? User { get; set; }
}