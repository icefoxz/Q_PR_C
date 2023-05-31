namespace OrderHelperLib.Contracts;

public enum DeliveryOrderStatus
{
    Created, // 等待接单
    Accepted, // 已接单
    Delivering, // 配送中
    Canceled = -1, // 已取消
    Delivered = -2, // 已送达
    Settle = -3, // 解决
    Exception = -4, // 异常
}

public static class DeliveryOrderStatusExtension
{
    /// <summary>
    /// 是否已关闭
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static bool IsClosed(this DeliveryOrderStatus status) => status switch
    {
        DeliveryOrderStatus.Canceled => true,
        DeliveryOrderStatus.Delivered => true,
        DeliveryOrderStatus.Settle => true,
        DeliveryOrderStatus.Exception => true,
        _ => false,
    };
    /// <summary>
    /// 是否还在进行中
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static bool IsOpen(this DeliveryOrderStatus status) => !status.IsClosed();
    public static bool IsCreated(this DeliveryOrderStatus status) => status == DeliveryOrderStatus.Created;
    public static bool IsAccepted(this DeliveryOrderStatus status) => status == DeliveryOrderStatus.Accepted;
    public static bool IsDelivering(this DeliveryOrderStatus status) => status == DeliveryOrderStatus.Delivering;
    public static bool IsCanceled(this DeliveryOrderStatus status) => status == DeliveryOrderStatus.Canceled;
    public static bool IsDelivered(this DeliveryOrderStatus status) => status == DeliveryOrderStatus.Delivered;
}