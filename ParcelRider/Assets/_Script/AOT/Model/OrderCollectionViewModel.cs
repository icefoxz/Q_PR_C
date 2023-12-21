using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.DataModel;

namespace AOT.Model
{
    /// <summary>
    /// 订单集合, 用于存储订单列表和当前订单
    /// </summary>
    public abstract class DoPageModel 
    {
        // 订单列表
        private List<DeliveryOrder> _orders = new List<DeliveryOrder>();
        public IReadOnlyList<DeliveryOrder> Orders => _orders;
        protected abstract void SendOrdersUpdateEvent();

        public int PageIndex { get; private set; }

        /// <summary>
        /// 设置订单
        /// </summary>
        public void SetOrders(ICollection<DeliveryOrder> orders,int pageIndex)
        {
            PageIndex = pageIndex;
            _orders.Clear();
            _orders.AddRange(orders);
            SendOrdersUpdateEvent();
        }

        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="id"></param>
        public void RemoveOrder(long id)
        {
             // 当前订单被删除
             var o = GetOrder(id);
             if (o == null) return;
            _orders.Remove(o);
            SendOrdersUpdateEvent();
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DeliveryOrder GetOrder(long id) => Orders.FirstOrDefault(o => o.Id == id);

        // 发送事件
        protected void SendEvent(string eventString) => App.SendEvent(eventString, null);

        public void Reset()
        {
            _orders.Clear();
            PageIndex = 0;
        }

        /// <summary>
        /// 添加订单 (删除已有的Id)
        /// </summary>
        /// <param name="order"></param>
        public void AddOrder(DeliveryOrder order)
        {
            var o = _orders.FirstOrDefault(x => x.Id == order.Id);
            if (o != null) _orders.Remove(o);
            _orders.Add(order);
            SendOrdersUpdateEvent();
        }
    }

    public class UnassignedDoModel : DoPageModel
    {
        protected override void SendOrdersUpdateEvent()
        {
            SendEvent(EventString.Orders_Unassigned_Update);
        }
    }

    public class ActiveDoModel : DoPageModel
    {
        protected override void SendOrdersUpdateEvent()
        {
            SendEvent(EventString.Orders_Assigned_Update);
        }
    }
    public class HistoryDoModel : DoPageModel
    {
        protected override void SendOrdersUpdateEvent()
        {
            SendEvent(EventString.Orders_History_Update);
        }
    }
}
