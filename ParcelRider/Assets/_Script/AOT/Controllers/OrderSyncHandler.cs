using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AOT.Core;
using AOT.DataModel;
using AOT.Model;
using AOT.Network;
using OrderHelperLib;

namespace AOT.Controllers
{
    public class OrderSyncHandler 
    {
        private SignalRCaller _caller;
        private AppModels AppModel => App.Models;
        
        public OrderSyncHandler(SignalRClient signalRClient)
        {
            _caller = new SignalRCaller(signalRClient);
        }

        private Task Req_Do_Version(long[] ids,
            Action<DataBag> onSuccess,
            Func<string, Task<bool>> onErrRetry) =>
            _caller.Req_Do_Version(ids, onSuccess, onErrRetry);

        public async void SynchronizeOrders(Func<string, Task<bool>> onErrRetry, 
            Action<bool> assignedSyncAction,
            Action<bool> historySyncAction,
            Action<bool> unassignedSyncAction)
        {
            var orderIds = AppModel.AllOrders.Select(o => o.Id).ToArray();
            await Req_Do_Version(orderIds, bag => VerifyVersions(bag.Get<Dictionary<long, int>>(0)), onErrRetry);

            void VerifyVersions(Dictionary<long, int> dic)
            {
                var isASync = IsAllSynchronized(dic, AppModel.AssignedOrders.Orders);
                var isUSync = IsAllSynchronized(dic, AppModel.UnassignedOrders.Orders);
                var isHSync = IsAllSynchronized(dic, AppModel.History.Orders);
                assignedSyncAction(isASync);
                historySyncAction(isHSync);
                unassignedSyncAction?.Invoke(isUSync);
            }
        }

        private static bool IsAllSynchronized(Dictionary<long, int> dic, IReadOnlyList<DeliveryOrder> orders)
        {
            var assignedIds = orders.Select(o => new { o.Id, o.Version }).ToArray();
            var map = dic.Join(assignedIds, a => a.Key, o => o.Id, (a, o) => a.Value == o.Version).ToArray();
            var isSameLength = assignedIds.Length == map.Length;
            var isSameVersion = map.All(t => t);
            return isSameLength && isSameVersion;
        }
    }
}