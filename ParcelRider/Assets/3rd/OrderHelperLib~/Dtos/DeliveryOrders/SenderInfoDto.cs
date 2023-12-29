using OrderHelperLib.Dtos.Users;

namespace OrderHelperLib.Dtos.DeliveryOrders;

/// <summary>
/// 发货员信息
/// </summary>
public record SenderInfoDto
{
    // 发件人Id(如果有账号的话))
    public string? UserId { get; set; }
    public UserModel? User { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string NormalizedPhoneNumber { get; set; }

    public SenderInfoDto()
    {
        
    }
}