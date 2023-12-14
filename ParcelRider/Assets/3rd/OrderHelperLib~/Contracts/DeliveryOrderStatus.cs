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
    public static DoSubState Instance(int stateId, DeliveryOrderStatus status, string stateName, TransitionRoles flag,
        params DeliveryOrderStatus[] allowStatus) =>
        Instance(stateId, status, stateName, allowStatus, flag, Array.Empty<int>());

    public static DoSubState Instance(int stateId,DeliveryOrderStatus status, string stateName, TransitionRoles flag,params int[] fromStates) =>
        new(stateId, status, stateName, Array.Empty<DeliveryOrderStatus>(), flag,fromStates);

    public static DoSubState Instance(int stateId, DeliveryOrderStatus status, string stateName, DeliveryOrderStatus[] fromStatus,
        TransitionRoles flag,
        params int[] fromStates) =>
        new(stateId, status, stateName, fromStatus, flag, fromStates);

    public int StateId { get; set; }
    public string StateName { get; set; }
    public int Status { get; set; }
    public DeliveryOrderStatus GetStatus => (DeliveryOrderStatus)Status;

    /// <summary>
    /// 上个状态, 如果为null, 则表示是不能从上个状态执行
    /// </summary>
    public DeliveryOrderStatus[] FromStatusList { get; set; }
    /// <summary>
    /// 可操作角色
    /// </summary>
    public TransitionRoles TransitionRole { get; set; }
    public int[] FromStates { get; set; }

    public DoSubState()
    {
    }

    private DoSubState(int stateId, DeliveryOrderStatus status, string stateName, DeliveryOrderStatus[] fromStatusList,TransitionRoles flag, int[] fromStates)
    {
        StateId = stateId;
        StateName = stateName;
        FromStates = fromStates;
        Status = (int)status;
        FromStatusList = fromStatusList;
        TransitionRole = flag;
    }

    public bool IsAllowFrom(TransitionRoles role,int fromSubStateId)
    {
        if (!TransitionRole.HasFlag(role)) return false;
        var state = DoStateMap.GetState(fromSubStateId);
        var status = state?.GetStatus ?? fromSubStateId.ConvertToDoStatus();
        return FromStatusList.Contains(status) || FromStates.Contains(fromSubStateId);
    }

    public const int CreateStatus = 0;
    public const int AssignState = 100;
    public const int SenderCancelState = -100;
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
        DoSubState.Instance(0, DeliveryOrderStatus.Created,"Order Created", Array.Empty<DeliveryOrderStatus>(), TransitionRoles.None)
    };

    public static IEnumerable<DoSubState> GetAllSubStates() => Created
        .Concat(Assigned)
        .Concat(Delivering)
        .Concat(PostDelivery)
        .Concat(Exceptions)
        .Concat(Cancel)
        .Concat(Completed);

    public static DoSubState? GetState(int stateId) => GetAllSubStates().FirstOrDefault(s => s.StateId == stateId);

    public static readonly DoSubState[] Exceptions = new[]
    {
        DoSubState.Instance(4001,DeliveryOrderStatus.Exception, "Sender Complaint", TransitionRoles.User, DeliveryOrderStatus.Assigned,
            DeliveryOrderStatus.Delivering, DeliveryOrderStatus.PostDelivery),
        //Assigned
        DoSubState.Instance(4101,DeliveryOrderStatus.Exception, "Unable to pickup", TransitionRoles.Rider, DeliveryOrderStatus.Assigned),
        DoSubState.Instance(4102,DeliveryOrderStatus.Exception, "Item not match", TransitionRoles.Rider, DeliveryOrderStatus.Assigned),
        DoSubState.Instance(4103,DeliveryOrderStatus.Exception, "Item Damaged", TransitionRoles.Rider, DeliveryOrderStatus.Assigned),
        //Delivering
        DoSubState.Instance(4201,DeliveryOrderStatus.Exception, "Traffic Issues", TransitionRoles.Rider, DeliveryOrderStatus.Delivering),
        DoSubState.Instance(4202,DeliveryOrderStatus.Exception, "Wrong Address", TransitionRoles.Rider, DeliveryOrderStatus.Delivering),
        DoSubState.Instance(4203,DeliveryOrderStatus.Exception, "No Receiver", TransitionRoles.Rider, DeliveryOrderStatus.Delivering),
        DoSubState.Instance(4204,DeliveryOrderStatus.Exception, "Vehicle Breakdown", TransitionRoles.Rider, DeliveryOrderStatus.Delivering),
        //PostDelivery
        DoSubState.Instance(4301,DeliveryOrderStatus.Exception, "Unreachable Recipient", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance(4302,DeliveryOrderStatus.Exception, "No Consensus with Sender", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance(4303,DeliveryOrderStatus.Exception, "Unsafe Delivery Location", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance(4304,DeliveryOrderStatus.Exception, "Refusal by Recipient", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance(4305,DeliveryOrderStatus.Exception, "Changed Delivery Conditions", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
    };

    public static readonly DoSubState[] Assigned = new[]
    {
        //骑手最开始的阶段, 获取订单后会自动转成这个状态
        DoSubState.Instance(100,DeliveryOrderStatus.Assigned, "Rider assigned", TransitionRoles.Rider, DeliveryOrderStatus.Created),
        DoSubState.Instance(101,DeliveryOrderStatus.Assigned, "Ongoing to Pickup", TransitionRoles.Rider, DeliveryOrderStatus.Created,
            DeliveryOrderStatus.Assigned),
    };

    public static readonly DoSubState[] Delivering = new[]
    {
        DoSubState.Instance(201,DeliveryOrderStatus.Delivering, "Enroute to Destination", TransitionRoles.Rider, DeliveryOrderStatus.Assigned),
        DoSubState.Instance(202,DeliveryOrderStatus.Delivering, "Scheduled Arrival", TransitionRoles.Rider, 201),
        DoSubState.Instance(203,DeliveryOrderStatus.Delivering, "Awaiting Delivery", TransitionRoles.Rider, 202),
        DoSubState.Instance(204,DeliveryOrderStatus.Delivering, "Consignment Transfer", TransitionRoles.Rider, 203),
    };

    public static readonly DoSubState[] PostDelivery = new[]
    {
        DoSubState.Instance(301,DeliveryOrderStatus.PostDelivery, "Destination Arrived", TransitionRoles.Rider, DeliveryOrderStatus.Delivering),
        DoSubState.Instance(302,DeliveryOrderStatus.PostDelivery, "Waiting for Collector", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance(303,DeliveryOrderStatus.PostDelivery, "No Receiver Found", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance(304,DeliveryOrderStatus.PostDelivery, "Calling Sender", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance(305,DeliveryOrderStatus.PostDelivery, "Consignment Transfer", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance(399,DeliveryOrderStatus.PostDelivery, "Item Delivered", TransitionRoles.Rider, DeliveryOrderStatus.Delivering, DeliveryOrderStatus.PostDelivery),
    };
    public static readonly DoSubState[] Cancel = new[]
    {
        DoSubState.Instance(DoSubState.SenderCancelState,DeliveryOrderStatus.Canceled, "Sender Cancellation",TransitionRoles.User, DeliveryOrderStatus.Created),
        DoSubState.Instance(-101,DeliveryOrderStatus.Canceled, "Rider Cancellation", TransitionRoles.Rider, DeliveryOrderStatus.Assigned),
        DoSubState.Instance(-102,DeliveryOrderStatus.Canceled, "Cancellation by Ordered", TransitionRoles.None, DeliveryOrderStatus.Created),
    };

    public static readonly DoSubState[] Completed = new[]
    {
        DoSubState.Instance(-200,DeliveryOrderStatus.Completed, "Customer Confirmed", TransitionRoles.User, DeliveryOrderStatus.Delivering),
        DoSubState.Instance(-201,DeliveryOrderStatus.Completed, "Automatically Completed", TransitionRoles.None, DeliveryOrderStatus.Delivering),
    };

    public static bool IsAssignableSubState(TransitionRoles role,int fromId, int toId)
    {
        var state = GetState(toId);
        return state != null && state.IsAllowFrom(role, fromId);
    }

    public static DoSubState[] GetPossibleStates(TransitionRoles role,int stateId)
    {
        var status = stateId.ConvertToDoStatus();
        var states = GetAllSubStates().ToArray();
        var toStates = states.Where(s => s.FromStates.Contains(stateId) || s.FromStatusList.Contains(status)).ToArray();
        return toStates.Where(s=>s.IsAllowFrom(role,stateId)).ToArray();
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
    public static bool IsInProgress(this DeliveryOrderStatus status) => !status.IsClosed();
    public static bool IsCreated(this DeliveryOrderStatus status) => status == DeliveryOrderStatus.Created;
    public static bool IsAssigned(this DeliveryOrderStatus status) => status == DeliveryOrderStatus.Assigned;
    public static bool IsInDelivery(this DeliveryOrderStatus status) => status == DeliveryOrderStatus.Delivering;
    public static bool IsInException(this DeliveryOrderStatus status) => status == DeliveryOrderStatus.Exception;
    public static bool IsInPostDelivery(this DeliveryOrderStatus status) => status == DeliveryOrderStatus.PostDelivery;
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
        // 判断原始数字是否为负
        bool isNegative = stateId < 0;

        // 处理负数，取绝对值
        stateId = Math.Abs(stateId);

        // 零的特殊情况
        if (stateId == 0) return 0;

        // 计算数字长度
        int length = (int)Math.Floor(Math.Log10(stateId));

        // 计算除数（10的length次幂）
        int divisor = (int)Math.Pow(10, length);

        // 返回第一个数字，并根据原始数字的符号调整
        return isNegative ? -(stateId / divisor) : stateId / divisor;
    }
}