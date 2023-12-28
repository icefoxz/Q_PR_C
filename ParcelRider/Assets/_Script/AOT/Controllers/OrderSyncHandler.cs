using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.DataModel;
using AOT.Model;
using AOT.Network;

namespace AOT.Controllers
{
    public class OrderSyncHandler 
    {
        private SignalRCaller _caller;
        private AppModels AppModel => App.Models;
        
        public OrderSyncHandler(SignalRCaller caller)
        {
            _caller = caller;
        }

        /// <summary>
        /// SignalR请求检查服务器订单版本号
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="onSuccess">如果版本号不一致, 则返回false</param>
        public void Req_Do_Version(IReadOnlyList<DeliveryOrder> orders, Action<bool> onSuccess) =>
            _caller.Req_Do_Version(orders.Select(o => o.Id).ToArray(), b =>
            {
                var result = b.Get<Dictionary<long, int>>(0);
                onSuccess(IsAllSynchronized(result));
                return;

                bool IsAllSynchronized(Dictionary<long, int> dic)
                {
                    var assignedIds = orders.Select(o => new { o.Id, o.Version }).ToArray();
                    var map = dic.Join(assignedIds, a => a.Key, o => o.Id, (a, o) => a.Value == o.Version)
                        .ToArray();
                    var isSameLength = assignedIds.Length == map.Length;
                    var isSameVersion = map.All(t => t);
                    return isSameLength && isSameVersion;
                }
            }, "Connection error.");
    }
}