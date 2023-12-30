using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.DataModel;
using AOT.Model;
using AOT.Utl;
using AOT.Views;
using Cysharp.Threading.Tasks;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;

namespace AOT.Controllers
{
    public class RiderOrderController : OrderControllerBase
    {
        private const int OrderPageSize = 50;
        private DeliveryOrder GetOrder(long orderId) => AppModel.GetOrder(orderId);

        public RiderOrderController(OrderSyncHandler orderSyncHandler) : base(orderSyncHandler)
        {
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

        //处理单个order的update, 并且更新到相应的列表中
        private void Resolve_OrderCollections(DeliveryOrder order) => AppModel.Resolve_Order(order, isRider: true);

        public void Do_Get_Unassigned(int pageIndex = -1, Action<bool> resultAction = null)
        {
            pageIndex = ResolvePageIndex(AppModel.GetDoPageModel(AppModels.PageOrders.Unassigned), pageIndex);
            Call(args => args[0], arg =>
            {
                var message = arg;
                var bag = DataBag.Deserialize(message);
                var list = bag.Get<List<DeliverOrderModel>>(0);
                List_Set(AppModels.PageOrders.Unassigned, list, pageIndex);
                resultAction?.Invoke(true);
            }, () => ApiPanel.Rider_GetUnassigned(OrderPageSize, pageIndex, pg =>
            {
                var orders = pg.List;
                var pageIndex = pg.PageIndex;
                var pageSize = pg.PageSize;
                List_Set(AppModels.PageOrders.Unassigned, orders, pageIndex);
                resultAction?.Invoke(true);
            }, m =>
            {
                MessageWindow.Set("Error", m);
                resultAction?.Invoke(false);
            }));
        }

        public void Do_Get_Assigned(int pageIndex = -1, Action<bool> resultAction = null)
        {
            pageIndex = ResolvePageIndex(AppModel.GetDoPageModel(AppModels.PageOrders.Assigned), pageIndex);
            Call(args => args[0], arg =>
            {
                var message = arg;
                var bag = DataBag.Deserialize(message);
                var orders = bag.Get<List<DeliverOrderModel>>(0);
                List_Set(AppModels.PageOrders.Assigned, orders.Where(o => o.Status > 0).ToList(), pageIndex);
                resultAction?.Invoke(true);
                //List_ActiveOrder_Set(list.ToArray());
            }, () => ApiPanel.Rider_GetAssigned(OrderPageSize, pageIndex, pg =>
            {
                var orders = pg.List;
                var pageIndex = pg.PageIndex;
                var pageSize = pg.PageSize;
                List_Set(AppModels.PageOrders.Assigned, orders, pageIndex);
                resultAction?.Invoke(true);
            }, m =>
            {
                MessageWindow.Set("Error", m);
                resultAction?.Invoke(false);
            }));
        }
        public void Do_Get_History(int pageIndex = -1, Action<bool> resultAction = null)
        {
            pageIndex = ResolvePageIndex(AppModel.GetDoPageModel(AppModels.PageOrders.History), pageIndex);
            Call(args => args[0], arg =>
            {
                var message = arg;
                var bag = DataBag.Deserialize(message);
                var list = bag.Get<List<DeliverOrderModel>>(0);
                List_Set(AppModels.PageOrders.History, list, pageIndex);
                resultAction?.Invoke(true);
            }, () => ApiPanel.Rider_GetHistories(OrderPageSize, pageIndex, pg =>
            {
                var orders = pg.List;
                var pageIndex = pg.PageIndex;
                var pageSize = pg.PageSize;
                List_Set(AppModels.PageOrders.History, orders, pageIndex);
                resultAction?.Invoke(true);
            }, m =>
            {
                MessageWindow.Set("Error", m);
                resultAction?.Invoke(false);
            }));
        }

        public void Do_AssignRider()
        {
            var order = App.Models.CurrentOrder;
            Call(new object[] { order },args => ((bool)args[0], (int)args[1], (long)args[2]), arg =>
            {
                var (success, status, oId) = arg;
                if (!success) return;
                order.Status = status;
                Do_Current_Set(oId);
                return;
            }, () =>
            {
                var id = order.Id;
                ApiPanel.Rider_AssignRider(id, dto =>
                {
                    var o = new DeliveryOrder(dto);
                    Resolve_OrderCollections(o);
                    Do_Current_Set(o.Id);
                }, msg =>
                {
                    ApiPanel.Rider_GetUnassigned(OrderPageSize, AppModel.GetDoPageModel(AppModels.PageOrders.Unassigned).PageIndex, pg =>
                    {
                        var list = pg.List.Select(o => new DeliveryOrder(o)).ToArray();
                        List_Set(AppModels.PageOrders.Unassigned, list, pg.PageIndex);
                    }, _ => MessageWindow.Set("Network error", "Unable to connect to server."));
                    MessageWindow.Set("Error", msg);
                });
            });
        }

        //同步订单版本, 如果版本号不一致, 需要请求更新
        public void Do_Sync_Assigned()
        {
            var assigned = AppModel.GetDoPageModel(AppModels.PageOrders.Assigned);
            SynchronizeOrder(assigned.Orders, () => Do_Get_Assigned(assigned.PageIndex));
        }
        public void Do_Sync_Unassigned()
        {
            var assigned = AppModel.GetDoPageModel(AppModels.PageOrders.Unassigned);
            SynchronizeOrder(assigned.Orders, () => Do_Get_Unassigned(assigned.PageIndex));
        }
        public void Do_Sync_History()
        {
            var assigned = AppModel.GetDoPageModel(AppModels.PageOrders.History);
            SynchronizeOrder(assigned.Orders, () => Do_Get_History(assigned.PageIndex));
        }

        public UniTask OnLoginLoadTask()
        {
            Do_Get_Unassigned();
            Do_Get_Assigned();
            Do_Get_History();
            Get_SubStates();
            return UniTask.CompletedTask;
            //var unassignedSuccess = false;
            //var assignedSuccess = false;
            //var historySuccess = false;
            //Do_Get_Unassigned(-1, success => unassignedSuccess = success);
            //await UniTask.WaitUntil(() => unassignedSuccess);
            //Do_Get_Assigned(-1, success => assignedSuccess = success);
            //await UniTask.WaitUntil(() => assignedSuccess);
            //Do_Get_History(-1, success => historySuccess = success);
            //await UniTask.WaitUntil(() => historySuccess);
            //Get_SubStates();
        }
    }
}