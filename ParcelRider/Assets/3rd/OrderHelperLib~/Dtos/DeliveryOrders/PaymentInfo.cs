using OrderHelperLib.Contracts;

namespace OrderHelperLib.Dtos.DeliveryOrders;

public record PaymentInfo
{
    /// <summary>
    /// 运送费
    /// </summary>
    public float Fee { get; set; }
    public float Charge { get; set; } // 价格
    public PaymentMethods Method { get; set; } // 付款类型
    /// <summary>
    /// 付款Reference,如果骑手代收将会是骑手Id, 如果是在线支付将会是支付平台的Reference, 如果是用户扣账将会是用户Id
    /// </summary>
    public string? Reference { get; set; }
    /// <summary>
    /// 付款TransactionId
    /// </summary>
    public int TransactionId { get; set; }
    public bool IsReceived { get; set; } // 是否已经完成付款
}