namespace DataModel
{
    /// <summary>
    /// 运算服务订单
    /// </summary>
    public class DeliveryOrder : Order
    {
        public int ItemId { get; set; }
        public string StartPoint { get; set; }
        public string EndPoint { get; set; }
        public float Distance { get; set; }
        public float Weight { get; set; }
        public float Price { get; set; }
        public int DeliveryManId { get; set; }
        public int Status { get; set; }
    }
}