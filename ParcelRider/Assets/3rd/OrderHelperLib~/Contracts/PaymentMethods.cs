namespace OrderHelperLib.Contracts;

public enum PaymentMethods : byte
{
    UserCredit, // 用户扣账
    RiderCollection, // 骑手代收
    OnlinePayment, // 在线支付
}