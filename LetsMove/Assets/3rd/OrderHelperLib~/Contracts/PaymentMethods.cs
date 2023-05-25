namespace OrderHelperLib.Contracts;

public enum PaymentMethods : byte
{
    UserCreditDeduction, // 用户扣账
    RiderCollection, // 骑手代收
    OnlinePayment, // 在线支付
}