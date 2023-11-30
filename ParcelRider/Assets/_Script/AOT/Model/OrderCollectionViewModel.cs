using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.DataModel;

namespace AOT.Model
{
    /// <summary>
    /// 订单集合, 用于存储订单列表和当前订单
    /// </summary>
    public abstract class DoDataModel 
    {
        // 订单列表
        private List<DeliveryOrder> _orders = new List<DeliveryOrder>();
        public IReadOnlyList<DeliveryOrder> Orders => _orders;
        protected abstract void SendOrdersUpdateEvent();

        // 设置订单
        public void SetOrders(ICollection<DeliveryOrder> orders)
        {
            _orders.Clear();
            _orders.AddRange(orders);
            SendOrdersUpdateEvent();
        }

        // 删除订单
        public void RemoveOrder(long id)
        {
             // 当前订单被删除
             var o = GetOrder(id);
            if (o == null) throw new NullReferenceException($"No order {id}");
            _orders.Remove(o);
            SendOrdersUpdateEvent();
        }

        // 查询订单
        public DeliveryOrder GetOrder(long id) => Orders.FirstOrDefault(o => o.Id == id);

        // 发送事件
        protected void SendEvent(string eventString) => App.SendEvent(eventString, null);

        public void Reset() => _orders.Clear();
    }

    public class UnassignedDoModel : DoDataModel
    {
        protected override void SendOrdersUpdateEvent()
        {
            SendEvent(EventString.Orders_Unassigned_Update);
        }
    }

    public class ActiveDoModel : DoDataModel
    {
        protected override void SendOrdersUpdateEvent()
        {
            SendEvent(EventString.Orders_Assigned_Update);
        }
    }
    public class HistoryDoModel : DoDataModel
    {
        protected override void SendOrdersUpdateEvent()
        {
            SendEvent(EventString.Orders_History_Update);
        }
    }
}
