using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.DataModel;
using OrderHelperLib.Dtos.DeliveryOrders;

namespace AOT.Model
{
    /// <summary>
    /// 订单集合, 用于存储订单列表和当前订单
    /// </summary>
    public class DoDataModel 
    {
        // 订单列表
        private List<DeliveryOrder> _orders = new List<DeliveryOrder>();
        public IReadOnlyList<DeliveryOrder> Orders => _orders;

        // 当前订单
        private DeliveryOrder _current;
        public DeliveryOrder? Current => _current;

        public void SetCurrent(int orderId)
        {
            var o = GetOrder(orderId);
            _current = o;
            SendEvent(EventString.CurrentOrder_Update);
            SendEvent(EventString.Orders_Update);
        }

        // 增加订单
        public void SetOrders(ICollection<DeliveryOrder> orders)
        {
            _orders.AddRange(orders);
            SendEvent(EventString.Orders_Update);
        }

        // 删除订单
        public void RemoveOrder(DeliveryOrder order)
        {
            var o = GetOrder(order.Id);
            if (!_orders.Remove(order)) return;
            SendEvent(EventString.Orders_Update);
            // 当前订单被删除
            if (o == Current)
            {
                _current = null;
                SendEvent(EventString.CurrentOrder_Update);
            }
        }

        // 更新订单
        public void UpdateOrder(DeliveryOrder order)
        {
            var o = GetOrder(order.Id);
            var index = _orders.IndexOf(o);
            if (index == -1) return;
            _orders[index] = order;
            SendEvent(EventString.Orders_Update);
            if (o == Current)
            {
                _current = order;
                SendEvent(EventString.CurrentOrder_Update);
            }
        }

        // 查询订单
        public DeliveryOrder GetOrder(int id) => Orders.FirstOrDefault(o => o.Id == id);

        // 发送事件
        private void SendEvent(string eventString) => App.MessagingManager.Send(eventString, null);

        public void ClearOrders()
        {
            _current = null;
            _orders.Clear();
            SendEvent(EventString.CurrentOrder_Update);
            SendEvent(EventString.Orders_Update);
        }
    }
}