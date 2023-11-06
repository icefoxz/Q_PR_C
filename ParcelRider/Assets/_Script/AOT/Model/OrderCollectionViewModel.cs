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

        private string? _currentId;
        public void SetCurrent(string orderId)
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
            _currentId = null;
            _orders.Clear();
            _orders.AddRange(orders);
            SendCurrentOrderUpdateEvent();
            SendOrdersUpdateEvent();
        }

        // 删除订单
        public void RemoveOrder(string id)
        {
             // 当前订单被删除
             var o = GetOrder(id);
            if (o == null) throw new NullReferenceException($"No order {id}");
            _orders.Remove(o);
            SendOrdersUpdateEvent();
            if (id == _currentId)
            {
                _currentId = null;
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
        public DeliveryOrder GetOrder(string id) => Orders.FirstOrDefault(o => o.Id == id);

        // 发送事件
        protected void SendEvent(string eventString) => App.SendEvent(eventString, null);
    }

    public class ActiveDoModel : DoDataModel
    {
        protected override void SendCurrentOrderUpdateEvent()
        {
            SendEvent(EventString.CurrentOrder_Update);
        }

        protected override void SendOrdersUpdateEvent()
        {
            SendEvent(EventString.Orders_Update);
        }
    }
    public class HistoryDoModel : DoDataModel
    {
        protected override void SendCurrentOrderUpdateEvent()
        {
            SendEvent(EventString.HistoryCurrentOrder_Update);
        }
        protected override void SendOrdersUpdateEvent()
        {
            SendEvent(EventString.HistoryOrders_Update);
        }
    }
}
