using OrderHelperLib.Contracts;

namespace OrderHelperLib.Contracts;

[Flags]public enum TransitionRoles
{
    None = 0,
    User = 1 << 0, // 0001
    Rider = 1 << 1, // 0010
    All = User | Rider // 0011
}
/// <summary>
/// 订单子状态
/// </summary>
public class DoSubState
{
    public static DoSubState Instance(int stateId, string stateName,TransitionRoles flag ,params DeliveryOrderStatus[] allowStatus) =>
        Instance(stateId, stateName, allowStatus, flag, Array.Empty<int>());

    public static DoSubState Instance(int stateId, string stateName, TransitionRoles flag,params int[] allowsStates) =>
        new(stateId, stateName, Array.Empty<DeliveryOrderStatus>(), flag,allowsStates);

    public static DoSubState Instance(int stateId, string stateName, DeliveryOrderStatus[] allowStatus,
        TransitionRoles flag,
        params int[] allowsStates) =>
        new(stateId, stateName, allowStatus, flag, allowsStates);

    public int StateId { get; set; }
    public string StateName { get; set; }

    /// <summary>
    /// 上个状态, 如果为null, 则表示是不能从上个状态执行
    /// </summary>
    public DeliveryOrderStatus[] AllowStatusList { get; set; }

    public TransitionRoles TransitionRole { get; set; }
    public int[] AllowsStates { get; set; }

    public DoSubState()
    {

    }

    private DoSubState(int stateId, string stateName, DeliveryOrderStatus[] allowStatusList, TransitionRoles flag,int[] allowsStates)
    {
        StateId = stateId;
        StateName = stateName;
        AllowsStates = allowsStates;
        AllowStatusList = allowStatusList;
        TransitionRole = flag;
    }

    public bool IsAllow(TransitionRoles role,int subStateId)
    {
        if (!TransitionRole.HasFlag(role)) return false;
        var state = subStateId.ConvertToDoStatus();
        return AllowStatusList.Contains(state) || AllowsStates.Contains(StateId);
    }

    public const int CreateStatus = 0;
    public const int AssignState = 100;
}

/// <summary>
/// 订单状态
/// </summary>
public class DoStateMap
{
    /// <summary>
    /// 作为数据库中Tag的Type
    /// </summary>
    public const string TagType = "DoSubState";
    // Created状态值只会是 0
    private static readonly DoSubState[] Created = new[]
    {
        DoSubState.Instance(0, "Order Created", Array.Empty<DeliveryOrderStatus>(), TransitionRoles.None),
    };

    public static IEnumerable<DoSubState> GetAllSubStates() => Created
        .Concat(Assigned)
        .Concat(Delivering)
        .Concat(PostDelivery)
        .Concat(Exceptions);

    public static readonly DoSubState[] Exceptions = new[]
    {
        DoSubState.Instance(4001, "Sender Complaint", TransitionRoles.User, DeliveryOrderStatus.Assigned,
            DeliveryOrderStatus.Delivering, DeliveryOrderStatus.PostDelivery),
        //Assigned
        DoSubState.Instance(4101, "Unable to pickup", TransitionRoles.Rider, DeliveryOrderStatus.Assigned),
        DoSubState.Instance(4102, "Item not match", TransitionRoles.Rider, DeliveryOrderStatus.Assigned),
        DoSubState.Instance(4103, "Item Damaged", TransitionRoles.Rider, DeliveryOrderStatus.Assigned),
        //Delivering
        DoSubState.Instance(4201, "Traffic Issues", TransitionRoles.Rider, DeliveryOrderStatus.Delivering),
        DoSubState.Instance(4202, "Wrong Address", TransitionRoles.Rider, DeliveryOrderStatus.Delivering),
        DoSubState.Instance(4203, "No Receiver", TransitionRoles.Rider, DeliveryOrderStatus.Delivering),
        DoSubState.Instance(4204, "Vehicle Breakdown", TransitionRoles.Rider, DeliveryOrderStatus.Delivering),
        //PostDelivery
        DoSubState.Instance(4301, "Unreachable Recipient", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance(4302, "No Consensus with Sender", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance(4303, "Unsafe Delivery Location", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance(4304, "Refusal by Recipient", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance(4305, "Changed Delivery Conditions", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
    };

    public static readonly DoSubState[] Assigned = new[]
    {
        //骑手最开始的阶段, 获取订单后会自动转成这个状态
        DoSubState.Instance(100, "Rider assigned", TransitionRoles.Rider, DeliveryOrderStatus.Created),
        DoSubState.Instance(101, "Ongoing to Pickup", TransitionRoles.Rider, DeliveryOrderStatus.Created,
            DeliveryOrderStatus.Assigned),
    };

    public static readonly DoSubState[] Delivering = new[]
    {
        DoSubState.Instance(201, "Enroute to Destination", TransitionRoles.Rider, DeliveryOrderStatus.Created),
        DoSubState.Instance(202, "Scheduled Arrival", TransitionRoles.Rider, 201),
        DoSubState.Instance(203, "Awaiting Delivery", TransitionRoles.Rider, 202),
        DoSubState.Instance(204, "Consignment Transfer", TransitionRoles.Rider, 203),
    };

    public static readonly DoSubState[] PostDelivery = new[]
    {
        DoSubState.Instance(301, "Destination Arrived", TransitionRoles.Rider, DeliveryOrderStatus.Delivering),
        DoSubState.Instance(302, "Waiting for Collector", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance(303, "No Receiver Found", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance(304, "Calling Sender", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance(305, "Consignment Transfer", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance(399, "Item Delivered", TransitionRoles.Rider, DeliveryOrderStatus.Delivering,
            DeliveryOrderStatus.PostDelivery),
    };
    public static readonly DoSubState[] Cancel = new[]
    {
        DoSubState.Instance(-100, "Sender Cancellation",TransitionRoles.User, DeliveryOrderStatus.Created),
        DoSubState.Instance(-101, "Rider Cancellation", TransitionRoles.Rider, DeliveryOrderStatus.Created),
        DoSubState.Instance(-102, "Cancellation by Ordered", TransitionRoles.None, DeliveryOrderStatus.Created),
    };

    public static readonly DoSubState[] Completed = new[]
    {
        DoSubState.Instance(-200, "Customer Confirmed", TransitionRoles.User, DeliveryOrderStatus.Delivering),
        DoSubState.Instance(-201, "Automatically Completed", TransitionRoles.None, DeliveryOrderStatus.Delivering),
    };

    public static bool IsAssignableSubState(TransitionRoles role,int fromId, int toId)
    {
        var state = GetAllSubStates().FirstOrDefault(s => s.StateId == fromId);
        return state != null && state.IsAllow(role, toId);
    }

    public static DoSubState[] GetPossibleStates(TransitionRoles role,int stateId)
    {
        var states = GetAllSubStates().ToArray();
        var state = states.FirstOrDefault(s => s.StateId == stateId);
        return state == null 
            ? Array.Empty<DoSubState>() 
            : states.Where(s => s.IsAllow(role, stateId)).ToArray();
    }

    public static DoSubState[] GetSubStates(DeliveryOrderStatus status)
    {
        return status switch
        {
            DeliveryOrderStatus.Assigned => Assigned,
            DeliveryOrderStatus.Delivering => Delivering,
            DeliveryOrderStatus.Canceled => Cancel,
            DeliveryOrderStatus.Completed => Completed,
            DeliveryOrderStatus.Created => Created,
            DeliveryOrderStatus.PostDelivery => PostDelivery,
            DeliveryOrderStatus.Exception => Exceptions,
            DeliveryOrderStatus.Closed => Array.Empty<DoSubState>(),
            _ => throw new ArgumentException("Invalid status", nameof(status)),
        };
    }
}

public enum DeliveryOrderStatus
{
    Created, // 等待接单
    Assigned = 1, // 已接单
    Delivering = 2, // 配送中
    PostDelivery = 3, // 运输后处理
    Exception = 4, // 异常
    Canceled = -1, // 已取消
    Completed = -2, // 已完成
    Closed = -3, // 关闭
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
        DeliveryOrderStatus.Closed => true,
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

    public static DeliveryOrderStatus ConvertToDoStatus(this int stateId) =>
        ConvertToDoStatusInt(stateId) switch
        {
            0 => DeliveryOrderStatus.Created,
            1 => DeliveryOrderStatus.Assigned,
            2 => DeliveryOrderStatus.Delivering,
            3 => DeliveryOrderStatus.PostDelivery,
            4 => DeliveryOrderStatus.Exception,
            -1 => DeliveryOrderStatus.Canceled,
            -2 => DeliveryOrderStatus.Completed,
            -3 => DeliveryOrderStatus.Closed,
            _ => throw new ArgumentException("Invalid status", nameof(stateId)),
        };

    public static int ConvertToDoStatusInt(this int stateId)
    {
        if (stateId == 0) return 0; // 零的特殊情况

        // 处理负数，取绝对值
        stateId = Math.Abs(stateId);

        // 计算数字长度
        int length = (int)Math.Floor(Math.Log10(stateId));

        // 计算除数（10的length次幂）
        int divisor = (int)Math.Pow(10, length);

        // 返回第一个数字
        return stateId / divisor;
    }
}