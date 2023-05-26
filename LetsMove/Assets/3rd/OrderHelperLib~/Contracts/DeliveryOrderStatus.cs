namespace OrderHelperLib.Contracts;

public enum DeliveryOrderStatus
{
    Created, // 等待接单
    Accepted, // 已接单
    InProgress, // 配送中
    Canceled = -1, // 已取消
    Delivered = -2, // 已送达
    Closed = -3, // 关闭
    Exception = -4, // 异常
}