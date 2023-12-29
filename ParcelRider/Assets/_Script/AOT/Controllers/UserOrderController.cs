using AOT.Core;
using AOT.DataModel;
using AOT.Model;
using AOT.Test;
using AOT.Utl;
using AOT.Views;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using OrderHelperLib.Dtos.Lingaus;
using UnityEngine;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;

namespace AOT.Controllers
{
    public abstract class OrderControllerBase : ControllerBase
    {
        public const float KgToPounds = 2.2046226218f;
        public const float MeterToFeet = 3.280839895f;
        protected AppModels AppModel => App.Models;
        private OrderSyncHandler OrderSyncHandler { get; }

        protected OrderControllerBase(OrderSyncHandler orderSyncHandler)
        {
            OrderSyncHandler = orderSyncHandler;
        }

        protected void List_Set(AppModels.PageOrders page, ICollection<DeliverOrderModel> orders, int pageIndex) =>
            AppModel.SetPageOrders(page, orders.Select(o => new DeliveryOrder(o)).ToArray(), pageIndex);

        private ConcurrentDictionary<string, bool> MethodExecutionLocks { get; } = new ConcurrentDictionary<string, bool>();
        protected void SynchronizeOrder(IReadOnlyList<DeliveryOrder> orders,
            Action onUnSyncAction, 
            [CallerMemberName]string methodName = null)
        {
            if (MethodIsLocked(methodName)) return;
            OrderSyncHandler.Req_Do_Version(orders, isSync =>
            {
                if (isSync) return;
                onUnSyncAction();
                MethodUnlock(methodName);
            });
        }

        private bool MethodIsLocked(string methodName) => MethodExecutionLocks.TryAdd(methodName, false);
        private void MethodUnlock(string methodName) => MethodExecutionLocks[methodName] = false;
#if UNITY_EDITOR
        protected bool IsTestMode([CallerMemberName] string methodName = null)
        {
            if (!App.IsTestMode) return false;
            Debug.LogWarning($"记得写[{methodName}]的So");
            return true;
        }
#endif
    }

    public class UserOrderController : OrderControllerBase
    {
        public UserOrderController(OrderSyncHandler orderSyncHandler) : base(orderSyncHandler)
        {
        }
        public void Do_Create(DeliverOrderModel order, Action<bool, string> callbackAction)
        {
            Call(new object[] { order }, args => ((bool)args[0], (string)args[1]),
                arg =>
                {
                    var (isSuccess, message) = arg;
                    if (isSuccess)
                    {
                        var bag = DataBag.Deserialize(message);
                        var dOrder = bag.Get<DeliveryOrder>(0);
                        var model = new DeliveryOrder(dOrder);
                        Resolve_Orders(model);
                        Do_SetCurrent(model.Id);
                        message = string.Empty;
                    }

                    callbackAction(isSuccess, message);
                },
                () =>
                {
                    ApiPanel.CreateDeliveryOrder(order, doModel =>
                    {
                        var m = new DeliveryOrder(doModel);
                        Resolve_Orders(m);
                        Do_SetCurrent(m.Id);
                        callbackAction?.Invoke(true, string.Empty);
                    }, msg =>
                    {
                        callbackAction?.Invoke(false, msg);
                    });
                });
        }

        private void Resolve_Orders(DeliveryOrder model) => AppModel.Resolve_Order(model, isRider: false);

        public void Do_UpdateActives(int pageIndex = -1,Action<bool> resultAction = null)
        {
            pageIndex = ResolvePageIndex(AppModel.GetDoPageModel(AppModels.PageOrders.Assigned),pageIndex);
            Call(args => args[0], arg =>
                {
                    var bag = DataBag.Deserialize(arg);
                    var list = bag.Get<List<DeliverOrderModel>>(0);
                    List_Set(AppModels.PageOrders.Assigned,list.ToArray(), pageIndex);
                    resultAction?.Invoke(true);
                },
                () => ApiPanel.User_GetActives(50, pageIndex, pg =>
                    {
                        List_Set(AppModels.PageOrders.Assigned, pg.List, pageIndex);
                        resultAction?.Invoke(true);
                    },
                    msg =>
                    {
                        MessageWindow.Set("Error", "Error in updating data!");
                        resultAction?.Invoke(false);
                    }));
        }

        public void Do_UpdateHistory(int pageIndex = -1,Action<bool> resultAction = null)
        {
            pageIndex = ResolvePageIndex(AppModel.GetDoPageModel(AppModels.PageOrders.History), pageIndex);
            Call(args => (string)args[0], arg =>
            {
                var bag = DataBag.Deserialize(arg);
                List<DeliverOrderModel> list = bag.Get<List<DeliverOrderModel>>(0);
                List_Set(AppModels.PageOrders.History, list, pageIndex);
                resultAction?.Invoke(true);
            }, () =>
            {
                ApiPanel.User_GetHistories(50, pageIndex, pg =>
                    {
                        List_Set(AppModels.PageOrders.History, pg.List, pageIndex);
                        resultAction?.Invoke(true);
                    },
                    msg =>
                    {
                        MessageWindow.Set("Error", "Error in updating data!");
                        resultAction?.Invoke(false);
                    });
            });
        }

        public void Do_RequestCancel(long orderId)
        {
            Call(new object[] { orderId }, args => ((bool)args[0], (DeliveryOrderStatus)args[1], (long)args[2]), arg =>
            {
                var (success, status, ordId) = arg;
                if (success)
                {
                    var o = AppModel.TryGetOrder(AppModels.PageOrders.Assigned, ordId);
                    o.Status = ((int)status);
                    o.SubState = DoSubState.SenderCancelState;
                    Resolve_Orders(o);
                    Do_SetCurrent(o.Id);
                }
            }, () =>
            {
                var o = AppModel.TryGetOrder(AppModels.PageOrders.Assigned, orderId);
                ApiPanel.CancelDeliveryOrder(orderId, o.SubState, (success, bag, message) =>
                {
                    if (success)
                    {
                        var order = bag.Get<DeliverOrderModel>(0);
                        Resolve_Orders(new DeliveryOrder(order));
                        Do_SetCurrent(orderId);
                        return;
                    }
                    MessageWindow.Set("Error", message);
                });
            });
        }

        public void Get_SubStates()
        {
#if UNITY_EDITOR
            if (IsTestMode()) return;
#endif
            ApiPanel.User_GetSubStates(b =>
            {
                var subStates = b.Get<DoSubState[]>(0);
                AppModel.SetSubStates(subStates);
            }, msg => MessageWindow.Set("Error", "Error in updating data!"));
        }

        public void Logout() => AppModel.UserLogout();

        public void Do_SetCurrent(long orderId) => App.Models.SetCurrentOrder(orderId);

        public void DoPay_RiderCollect(Action<bool, string> callbackAction)
        {
            Call(args => ((bool)args[0], (string)args[1]), arg =>
            {
                var (isSuccess, message) = arg;
                if (isSuccess)
                {
                    message = string.Empty;
                }
                callbackAction(isSuccess, message);
            }, () =>
            {
                var orderId = AppModel.CurrentOrder.Id;
                ApiPanel.User_DoPay_Rider(orderId, b =>
                    {
                        var order = b.Get<DeliverOrderModel>(0);
                        Resolve_Orders(new DeliveryOrder(order));
                        callbackAction(true, string.Empty);
                    },
                    message => callbackAction(false, message));
            });
        }

        public void DoPay_DeductFromCredit(Action<bool, string> callbackAction)
        {
            Call(args => ((bool)args[0], (string)args[1]), arg =>
            {
                var (isSuccess, message) = arg;
                if (isSuccess)
                {
                    message = String.Empty;
                }
                callbackAction(isSuccess, message);
            }, () =>
            {
                var orderId = AppModel.CurrentOrder.Id;
                ApiPanel.User_DoPay_Credit(orderId, b =>
                {
                    var lingau = b.Get<LingauModel>(0);
                    AppModel.SetUserLingau(lingau);
                }, message => callbackAction(false, message));
            });
        }

        public async UniTask OnLoginLoadingTask()
        {
            var activeSuccess = false;
            var historySuccess = false;
            Do_UpdateActives(-1, success => activeSuccess= success);
            await UniTask.WaitUntil(() => activeSuccess);
            Do_UpdateHistory(-1, success => historySuccess = success);
            await UniTask.WaitUntil(() => historySuccess);
            Get_SubStates();
        }
    }
}