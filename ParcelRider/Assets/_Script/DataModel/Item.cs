namespace DataModel
{
    public class Item : EntityBase
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public float Weight { get; set; }
        public int Quantity { get; set; }
        public string Size { get; set; }
    }
}