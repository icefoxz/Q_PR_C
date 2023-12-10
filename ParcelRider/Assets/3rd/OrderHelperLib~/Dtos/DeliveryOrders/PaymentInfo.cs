using OrderHelperLib.Contracts;

namespace OrderHelperLib.Dtos.DeliveryOrders;

public record PaymentInfoDto
{
    /// <summary>
    /// 运送费
    /// </summary>
    public float Fee { get; set; }
    public float Charge { get; set; } // 价格
    public string Method { get; set; } = string.Empty; // 付款类型
    /// <summary>
    /// 付款Reference,如果骑手代收将会是骑手Id,
    /// 如果是在线支付将会是支付平台的Reference,
    /// 如果是用户扣账将会是用户Id
    /// </summary>
    public string? Reference { get; set; }
    /// <summary>
    /// 付款TransactionId
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;
    public bool IsReceived { get; set; } // 是否已经完成付款
}

public enum PaymentMethods : byte
{
    UserCredit, // 用户扣账
    RiderCollection, // 骑手代收
    OnlinePayment, // 在线支付
}

public static class PaymentMethodsExtensions
{
    public static string ToDisplayString(this PaymentMethods method)
    {
        return method switch
        {
            PaymentMethods.UserCredit => "UserCredit",
            PaymentMethods.RiderCollection => "RiderCollection",
            PaymentMethods.OnlinePayment => "OnlinePayment",
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
        };
    }

    public static PaymentMethods ToPaymentMethod(this string method)
    {
        return method switch
        {
            "UserCredit" => PaymentMethods.UserCredit,
            "RiderCollection" => PaymentMethods.RiderCollection,
            "OnlinePayment" => PaymentMethods.OnlinePayment,
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
        };
    }
}