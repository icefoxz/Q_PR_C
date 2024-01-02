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
    public const string Created = "0"; // 等待接单
    public const string Assigned  ="1"; // 已接单
    public const string Delivering  ="2"; // 配送中
    public const string PostDelivery  ="3"; // 运输后处理
    public const string Exception  ="4"; // 异常
    public const string Canceled  ="-1"; // 已取消
    public const string Completed  ="-2"; // 已完成
    public const string Closed  ="-3"; // 关闭

    public static DoSubState Instance(string stateId, DeliveryOrderStatus status, string stateName,
        TransitionRoles flag,
        params DeliveryOrderStatus[] allowStatus) =>
        Instance(stateId, status, stateName, allowStatus, flag, Array.Empty<string>());

    public static DoSubState Instance(string stateId,DeliveryOrderStatus status, string stateName, TransitionRoles flag,params string[] fromStates) =>
        new(stateId, status, stateName, Array.Empty<DeliveryOrderStatus>(), flag,fromStates);

    public static DoSubState Instance(string stateId, DeliveryOrderStatus status, string stateName,
        DeliveryOrderStatus[] fromStatus,
        TransitionRoles flag,
        params string[] fromStates) =>
        new(stateId, status, stateName, fromStatus, flag, fromStates);

    public string StateId { get; set; }
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
    public string[] FromStates { get; set; }

    public DoSubState()
    {
    }

    private DoSubState(string stateId, DeliveryOrderStatus status, string stateName,
        DeliveryOrderStatus[] fromStatusList, TransitionRoles flag, string[] fromStates)
    {
        StateId = stateId;
        StateName = stateName;
        FromStates = fromStates;
        Status = (int)status;
        FromStatusList = fromStatusList;
        TransitionRole = flag;
    }

    public bool IsAllowFrom(TransitionRoles role,string fromSubStateId)
    {
        if (!TransitionRole.HasFlag(role)) return false;
        var state = DoStateMap.GetState(fromSubStateId);
        var status = state?.GetStatus ?? fromSubStateId.ConvertToDoStatus();
        return FromStatusList.Contains(status) || FromStates.Contains(fromSubStateId);
    }

    public const string CreateStatus = "0";
    public const string AssignState = "1.0";
    public const string SenderCancelState = "-1.0";
    public const string RiderItemDelivered = "-2.1";
    public const string ConsignmentTransfer = "-2.11";
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
    public static readonly DoSubState[] Created = new[]
    {
        DoSubState.Instance(DoSubState.CreateStatus, DeliveryOrderStatus.Created,"Order Created", Array.Empty<DeliveryOrderStatus>(), TransitionRoles.None)
    };

    public static IEnumerable<DoSubState> GetAllSubStates() => Created
        .Concat(Assigned)
        .Concat(Delivering)
        .Concat(PostDelivery)
        .Concat(Exceptions)
        .Concat(Cancel)
        .Concat(Completed);

    public static DoSubState? GetState(string stateId) => GetAllSubStates().FirstOrDefault(s => s.StateId == stateId);

    public static readonly DoSubState[] Exceptions = new[]
    {
        DoSubState.Instance("4.1",DeliveryOrderStatus.Exception, "Sender Complaint.", TransitionRoles.User, DeliveryOrderStatus.Assigned,
            DeliveryOrderStatus.Delivering, DeliveryOrderStatus.PostDelivery, DeliveryOrderStatus.Completed),
        //Assigned
        DoSubState.Instance("4.11",DeliveryOrderStatus.Exception, "Unable To Pickup.", TransitionRoles.Rider, DeliveryOrderStatus.Assigned),
        DoSubState.Instance("4.12",DeliveryOrderStatus.Exception, "Item Not Match.", TransitionRoles.Rider, DeliveryOrderStatus.Assigned),
        DoSubState.Instance("4.13",DeliveryOrderStatus.Exception, "Item Damaged.", TransitionRoles.Rider, DeliveryOrderStatus.Assigned),
        //Delivering
        DoSubState.Instance("4.21",DeliveryOrderStatus.Exception, "Traffic Issues.", TransitionRoles.Rider, DeliveryOrderStatus.Delivering),
        DoSubState.Instance("4.22",DeliveryOrderStatus.Exception, "Wrong Address.", TransitionRoles.Rider, DeliveryOrderStatus.Delivering),
        DoSubState.Instance("4.23",DeliveryOrderStatus.Exception, "No Receiver.", TransitionRoles.Rider, DeliveryOrderStatus.Delivering),
        DoSubState.Instance("4.24",DeliveryOrderStatus.Exception, "Vehicle Breakdown.", TransitionRoles.Rider, DeliveryOrderStatus.Delivering),
        //PostDelivery
        DoSubState.Instance("4.31",DeliveryOrderStatus.Exception, "Unreachable Recipient.", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance("4.32",DeliveryOrderStatus.Exception, "No Consensus with Sender.", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance("4.33",DeliveryOrderStatus.Exception, "Unsafe Delivery Location.", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance("4.34",DeliveryOrderStatus.Exception, "Refusal by Recipient.", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
        DoSubState.Instance("4.35",DeliveryOrderStatus.Exception, "Changed Delivery Conditions.", TransitionRoles.Rider, DeliveryOrderStatus.PostDelivery),
    };

    public static readonly DoSubState[] Assigned = new[]
    {
        //骑手最开始的阶段, 获取订单后会自动转成这个状态
        DoSubState.Instance("1.1",DeliveryOrderStatus.Assigned, "Rider assigned.", TransitionRoles.Rider, DeliveryOrderStatus.Created),
        DoSubState.Instance("1.2",DeliveryOrderStatus.Assigned, "Ongoing to Pickup.", TransitionRoles.Rider, DeliveryOrderStatus.Assigned),
    };

    public static readonly DoSubState[] Delivering = new[]
    {
        DoSubState.Instance("2.1",DeliveryOrderStatus.Delivering, "Enroute to Destination.", TransitionRoles.Rider, DeliveryOrderStatus.Assigned),
    };

    public static readonly DoSubState[] PostDelivery = new[]
    {
        DoSubState.Instance("3.1",DeliveryOrderStatus.PostDelivery, "Destination Arrived.", TransitionRoles.Rider, DeliveryOrderStatus.Delivering),
        DoSubState.Instance("3.2",DeliveryOrderStatus.PostDelivery, "Waiting for Collector.", TransitionRoles.Rider, "3.1","3.3","3.4"),
        DoSubState.Instance("3.3",DeliveryOrderStatus.PostDelivery, "No Receiver Found.", TransitionRoles.Rider, "3.1","3.2"),
        DoSubState.Instance("3.4",DeliveryOrderStatus.PostDelivery, "Calling Sender.", TransitionRoles.Rider,"3.1","3.2","3.3"),
    };
    public static readonly DoSubState[] Cancel = new[]
    {
        DoSubState.Instance(DoSubState.SenderCancelState,DeliveryOrderStatus.Canceled, "Sender Canceled.",TransitionRoles.User, DeliveryOrderStatus.Created),
        DoSubState.Instance("-1.1",DeliveryOrderStatus.Canceled, "Rider Canceled.", TransitionRoles.Rider, DeliveryOrderStatus.Assigned),
        DoSubState.Instance("-1.2",DeliveryOrderStatus.Canceled, "Cancellation by Ordered.", TransitionRoles.None, DeliveryOrderStatus.Created),
    };

    public static readonly DoSubState[] Completed = new[]
    {
        DoSubState.Instance(DoSubState.ConsignmentTransfer,DeliveryOrderStatus.PostDelivery, "Consignment Transfer.", TransitionRoles.Rider, "3.1","3.2","3.3","3.4"),
        DoSubState.Instance(DoSubState.RiderItemDelivered,DeliveryOrderStatus.PostDelivery, "Item Delivered.", TransitionRoles.Rider, DeliveryOrderStatus.Delivering),
        DoSubState.Instance("-2.0",DeliveryOrderStatus.Completed, "Customer Confirmed.", TransitionRoles.User, DeliveryOrderStatus.Delivering),
        DoSubState.Instance("-2.1",DeliveryOrderStatus.Completed, "Automatically Completed.", TransitionRoles.None, DeliveryOrderStatus.Delivering),
    };

    public static bool IsAssignableSubState(TransitionRoles role,string fromId,string toId)
    {
        var state = GetState(toId);
        return state != null && state.IsAllowFrom(role, fromId);
    }

    public static DoSubState[] GetPossibleStates(TransitionRoles role,string stateId)
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
            DeliveryOrderStatus.Closed => [],
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

    public static bool IsRiderJobDone(this DoSubState state) =>
        state.StateId is DoSubState.RiderItemDelivered or DoSubState.ConsignmentTransfer;

    /// <summary>
    /// 是否还在进行中
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static bool IsInProgress(this DeliveryOrderStatus status) => !status.IsClosed();

    public static DeliveryOrderStatus ConvertToDoStatus(this string stateId)
    {
        return ConvertToDoStatusInt(stateId) switch
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
    }

    private static readonly Dictionary<string, int> StatusMap = new Dictionary<string, int>
    {
        [DoSubState.Created] = 0,
        [DoSubState.Assigned] = 1,
        [DoSubState.Delivering] = 2,
        [DoSubState.PostDelivery] = 3,
        [DoSubState.Exception] = 4,
        [DoSubState.Canceled] = -1,
        [DoSubState.Completed] = -2,
        [DoSubState.Closed] = -3,
    };

    //高效优化的转换方法
    public static int ConvertToDoStatusInt(this string stateId)
    {
        if (string.IsNullOrEmpty(stateId))
            throw new ArgumentException("StateId cannot be null or empty", nameof(stateId));

        var index = 0;
        while (index < stateId.Length && stateId[index] >= '0' && stateId[index] <= '9') index++;

        var key = stateId.Substring(0, index);

        if (StatusMap.TryGetValue(key, out var status))
            return status;

        throw new ArgumentException("Invalid status", nameof(stateId));
    }
}