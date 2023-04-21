namespace DataModel
{
    /// <summary>
    /// 运算服务订单
    /// </summary>
    public class DeliveryOrder : Order
    {
        public enum States
        {
            None,
            Wait,
            Delivering,
            Collection,
            Complete,
            Exception,
        }

        public IdentityInfo From { get; set; }
        public IdentityInfo To { get; set; }
        public PackageInfo Package { get; set; }
        public string DeliveryManId { get; set; }
        public int Status { get; set; }
    }

    public class IdentityInfo
    {
        public string Phone { get; }
        public string Name { get; }
        public string Address { get; }

        public IdentityInfo(string phone, string name, string address)
        {
            Phone = phone;
            Name = name;
            Address = address;
        }
    }

    public class PackageInfo
    {
        public float Weight { get; }
        public float Price { get; }
        public float Distance { get; }
        public float Size { get; }

        public PackageInfo(float weight, float price, float distance, float size)
        {
            Weight = weight;
            Price = price;
            Distance = distance;
            Size = size;                    
        }
    }
}