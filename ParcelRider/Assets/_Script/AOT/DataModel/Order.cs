using System.Collections.Generic;

namespace AOT.DataModel
{
    /// <summary>
    /// 订单的基础类, 主要已线程方式实现各种状态<see cref="OrderTag"/>
    /// </summary>
    public class Order : EntityBase<string>
    {
        public string UserId { get; set; }
        public ICollection<OrderTag> Tags { get; set; }
    }
}