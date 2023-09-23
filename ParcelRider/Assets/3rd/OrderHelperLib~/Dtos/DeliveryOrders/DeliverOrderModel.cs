﻿using OrderHelperLib.Dtos.Riders;
using OrderHelperLib.Dtos.Users;

namespace OrderHelperLib.Dtos.DeliveryOrders;

public record DeliverOrderModel : IntDto
{
    // 执行用户Id
    public string UserId { get; set; }
    // 执行用户
    public UserModel User { get; set; }
    // 标签
    public ICollection<TagDto> Tags { get; set; }
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
    public int? RiderId { get; set; }
    public RiderModel? Rider { get; set; }
    //付款信息
    public PaymentInfo? PaymentInfo { get; set; }
    //订单状态, 正数 = 进行中, 负数 = 已完成
    public int Status { get; set; }
}