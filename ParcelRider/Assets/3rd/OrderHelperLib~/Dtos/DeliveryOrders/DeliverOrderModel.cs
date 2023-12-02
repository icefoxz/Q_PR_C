using OrderHelperLib.Dtos.Riders;
using OrderHelperLib.Dtos.Users;

namespace OrderHelperLib.Dtos.DeliveryOrders;

public record DeliverOrderModel : LongDto
{
    // 执行用户Id
    public string UserId { get; set; }
    // 执行用户
    public UserModel User { get; set; }
    //物品信息
    public ItemInfoDto ItemInfo { get; set; }
    // (马来西亚)州属Id
    public string MyState { get; set; }
    //寄件人信息
    public SenderInfoDto SenderInfo { get; set; }
    //收件人信息
    public ReceiverInfoDto ReceiverInfo { get; set; }
    //运送信息
    public DeliveryInfoDto DeliveryInfo { get; set; }
    //骑手信息
    public string? RiderId { get; set; }
    public RiderModel? Rider { get; set; }
    //付款信息
    public PaymentInfo? PaymentInfo { get; set; }
    //订单状态, 正数 = 进行中, 负数 = 已完成
    public int Status { get; set; }
    //订单子状态
    public int SubState { get; set; }
    //订单状态进程, 用于记录订单的状态变化
    public StateSegmentModel[] StateHistory { get; set; }
}

/// <summary>
/// 状态的纪录片短
/// </summary>
public record StateSegmentModel
{
    public int SubState { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Remark { get; set; }
}