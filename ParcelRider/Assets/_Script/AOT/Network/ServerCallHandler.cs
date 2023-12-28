using System.Linq;
using AOT.Controllers;
using AOT.Core;
using AOT.DataModel;
using OrderHelperLib;
using OrderHelperLib.Contracts;

namespace AOT.Network
{
    //专门处理服务器调用的类
    public class ServerCallHandler
    {
        public ServerCallHandler()
        {
            App.RegEvent(SignalREvents.Call_Do_Ver, Call_Do_Ver);
        }

        //服务器调用, 用于检查订单版本
        private void Call_Do_Ver(DataBag bag)
        {
            var orderId = bag.Get<long>(0);
            var version = bag.Get<int>(1);
            var status = bag.Get<int>(2);
            var updatedState = status.ConvertToDoStatus();
            var model = App.Models;
            var order = model.AllOrders.FirstOrDefault(o => o.Id == orderId);
            var isNeedToSync = order == null || order.Version != version;
            if (!isNeedToSync) return;
            if (App.IsUserMode)
            {
                var uoc = App.GetController<UserOrderController>();
                //用户不可能没有订单, 如果没有, 则说明当前看的页面不是订单页面
                if (order == null) return;
                var (isActive, isSameStatus) = GetStatus(order, updatedState);
                if (isSameStatus)
                {
                    if (isActive)
                        uoc.Do_UpdateActives();
                    else
                        uoc.Do_UpdateHistory();
                    return;
                }

                //如果状态不一致, 则需要更新所有数据
                uoc.Do_UpdateActives();
                uoc.Do_UpdateHistory();
            }
            else
            {
                //rider
                var roc = App.GetController<RiderOrderController>();
                if (order == null)
                {
                    if (updatedState == DeliveryOrderStatus.Created)
                        //如果没有订单, 则说明这是新创建的订单, 需要重新获取未分配的订单
                        roc.Do_Get_Unassigned();
                    return;
                }

                var (isActive, isSameStatus) = GetStatus(order, updatedState);

                if (isSameStatus)
                {
                    if (isActive)
                        roc.Do_Get_Assigned();
                    else
                        roc.Do_Get_History();
                    return;
                }

                roc.Do_Get_History();
                roc.Do_Get_Assigned();
            }

            return;

            (bool isActive, bool isSameStatus) GetStatus(DeliveryOrder order, DeliveryOrderStatus updatedState)
            {
                var orderInProgress = order.State.IsInProgress();
                return (orderInProgress, orderInProgress == updatedState.IsInProgress());
            }
        }

    }
}