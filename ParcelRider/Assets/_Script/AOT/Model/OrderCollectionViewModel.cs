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
        protected abstract void SendCurrentOrderUpdateEvent();
        protected abstract void SendOrdersUpdateEvent();
        // 当前订单
        public DeliveryOrder GetCurrent()
        {
            return GetOrder(_currentId);
        }

        private long _currentId = -1;
        public void SetCurrent(long orderId)
        {
            var o = GetOrder(orderId);
            if (o == null) throw new NullReferenceException($"Unable to find {orderId}");
            _currentId = orderId;
            SendCurrentOrderUpdateEvent();
            SendOrdersUpdateEvent();
        }

        // 设置订单
        public void SetOrders(ICollection<DeliveryOrder> orders)
        {
            _currentId = -1;
            _orders.Clear();
            _orders.AddRange(orders);
            SendCurrentOrderUpdateEvent();
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
            if (id == _currentId)
            {
                _currentId = -1;
                SendCurrentOrderUpdateEvent();
            }
        }

        // 更新订单
        public void UpdateOrder(DeliveryOrder order)
        {
            var o = GetOrder(order.Id);
            var index = _orders.IndexOf(o);
            _orders[index] = order;
            SendOrdersUpdateEvent();
            if (order.Id == _currentId)
            {
                SendCurrentOrderUpdateEvent();
            }
        }

        // 查询订单
        public DeliveryOrder GetOrder(long id) => Orders.FirstOrDefault(o => o.Id == id);

        // 发送事件
        protected void SendEvent(string eventString) => App.SendEvent(eventString, null);
    }

    public class UnassignedDoModel : DoDataModel
    {
        protected override void SendCurrentOrderUpdateEvent()
        {
            SendEvent(EventString.Order_Unassigned_Current_Update);
        }

        protected override void SendOrdersUpdateEvent()
        {
            SendEvent(EventString.Orders_Unassigned_Update);
        }
    }

    public class ActiveDoModel : DoDataModel
    {
        protected override void SendCurrentOrderUpdateEvent()
        {
            SendEvent(EventString.Order_Assigned_Current_Update);
        }

        protected override void SendOrdersUpdateEvent()
        {
            SendEvent(EventString.Orders_Assigned_Update);
        }
    }
    public class HistoryDoModel : DoDataModel
    {
        protected override void SendCurrentOrderUpdateEvent()
        {
            SendEvent(EventString.Order_History_Current_Update);
        }
        protected override void SendOrdersUpdateEvent()
        {
            SendEvent(EventString.Orders_History_Update);
        }
    }
}
