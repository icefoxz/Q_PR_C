using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AOT.Core;
using AOT.DataModel;
using AOT.Utl;
using AOT.Views;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using UnityEngine;

namespace AOT.Controllers
{
    public class RiderOrderController : OrderControllerBase
    {
        private DeliveryOrder GetOrder(long orderId) => AppModel.GetOrder(orderId);
        private bool IsTestMode([CallerMemberName]string methodName = null)
        {
            if (!App.IsTestMode) return false;
            Debug.LogWarning($"记得写[{methodName}]的So");
            return true;
        }

        public void Get_SubStates()
        {
            if (App.IsTestMode) return;
            ApiPanel.Rider_GetSubStates(b =>
            {
                var subStates = b.Get<DoSubState[]>(0);
                AppModel.SetSubStates(subStates);
            }, msg => MessageWindow.Set("Error", "Error in updating data!"));
        }

        public void Do_State_Update(int stateId)
        {
            var order = AppModel.CurrentOrder;
            Call(new object[] {order, stateId},args => args[0], arg =>
            {

            }, () =>
            {
                var o = AppModel.CurrentOrder;
                ApiPanel.Rider_UpdateState(o.Id, stateId, b =>
                {
                    var dto = b.Get<DeliverOrderModel>(0);
                    var order = new DeliveryOrder(dto);
                    Resolve_OrderCollections(order);
                    Do_Current_Set(order.Id);
                }, msg => MessageWindow.Set("Error", msg));
            });
        }

        public void Do_Current_Set(long orderId) => AppModel.SetCurrentOrder(orderId);

        public void PossibleState_Update(long orderId)
        {
            var o = GetOrder(orderId);
            var subStates = DoStateMap.GetPossibleStates(TransitionRoles.Rider, o.SubState);
            AppModel.SetStateOptions(subStates);
        }

        //处理单个order的update, 并且更新到相应的列表中
        private void Resolve_OrderCollections(DeliveryOrder order)=> AppModel.Resolve_Order(order);

        public void Do_Get_Unassigned(int pageIndex = 0)
        {
            Call(args => args[0], arg =>
            {
                var message = arg;
                var bag = DataBag.Deserialize(message);
                var list = bag.Get<List<DeliverOrderModel>>(0);
                List_ActiveOrder_Set(list.ToArray());
            }, () =>
            {
                ApiPanel.Rider_GetUnassigned(20, pageIndex, pg =>
                {
                    var orders = pg.List;
                    var pageIndex = pg.PageIndex;
                    var pageSize = pg.PageSize;
                    AppModel.UnassignedOrders.SetOrders(orders.Select(o => new DeliveryOrder(o)).ToList());
                }, m => MessageWindow.Set("Error", m));
            });
        }
        public void Do_Get_Assigned(int pageIndex = 0)
        {
            Call(args => args[0], arg =>
            {
                var message = arg;
                var bag = DataBag.Deserialize(message);
                var list = bag.Get<List<DeliverOrderModel>>(0);
                //List_ActiveOrder_Set(list.ToArray());
            }, () =>
            {
                ApiPanel.Rider_GetAssigned(20, pageIndex, pg =>
                {
                    var orders = pg.List;
                    var pageIndex = pg.PageIndex;
                    var pageSize = pg.PageSize;
                    AppModel.UnassignedOrders.SetOrders(orders.Select(o => new DeliveryOrder(o)).ToList());
                }, m => MessageWindow.Set("Error", m));
            });
        }
        public void Do_Get_History(int pageIndex = 0)
        {
            Call(args => args[0], arg =>
            {
                var message = arg;
                var bag = DataBag.Deserialize(message);
                var list = bag.Get<List<DeliverOrderModel>>(0);
                List_HistoryOrderSet(list.ToArray());
            }, () =>
            {
                ApiPanel.Rider_GetHistories(20, pageIndex, pg =>
                {
                    var orders = pg.List;
                    var pageIndex = pg.PageIndex;
                    var pageSize = pg.PageSize;
                    AppModel.History.SetOrders(orders.Select(o => new DeliveryOrder(o)).ToList());
                }, m => MessageWindow.Set("Error", m));
            });
        }

        public void Do_AssignRider(long orderId)
        {
            var order = GetOrder(orderId);
            Call(new object[] { order },args => ((bool)args[0], (int)args[1], (long)args[2]), arg =>
            {
                var (success, status, oId) = arg;
                if (!success) return;
                order.Status = status;
                Do_Current_Set(oId);
                return;
            }, () =>
            {
                var id = orderId;
                ApiPanel.Rider_AssignRider(id, dto =>
                {
                    var o = new DeliveryOrder(dto);
                    Resolve_OrderCollections(o);
                    Do_Current_Set(o.Id);
                }, msg => MessageWindow.Set("Error", msg));
            });
        }

        public void LoggedInTasks()
        {
            Do_Get_Unassigned();
            Do_Get_Assigned();
            Do_Get_History();
            Get_SubStates();
        }
    }
}