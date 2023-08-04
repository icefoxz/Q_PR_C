namespace OrderHelperLib.Contracts;

public enum DeliveryOrderStatus
{
    Created, // 等待接单
    Assigned, // 已接单
    Delivering, // 配送中
    Exception, // 异常
    Canceled = -1, // 已取消
    Completed = -2, // 已完成
    Close = -3, // 关闭
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
        DeliveryOrderStatus.Completed => true,
        DeliveryOrderStatus.Close => true,
        _ => false,
    };
    /// <summary>
    /// 是否还在进行中
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static bool IsOnProgressing(this DeliveryOrderStatus status) => !status.IsClosed();
    public static bool IsCreated(this DeliveryOrderStatus status) => status == DeliveryOrderStatus.Created;
    public static bool IsAssigned(this DeliveryOrderStatus status) => status == DeliveryOrderStatus.Assigned;
    public static bool IsOnDelivering(this DeliveryOrderStatus status) => status == DeliveryOrderStatus.Delivering;
    public static bool IsCanceled(this DeliveryOrderStatus status) => status == DeliveryOrderStatus.Canceled;
    public static bool IsCompleted(this DeliveryOrderStatus status) => status == DeliveryOrderStatus.Completed;
}