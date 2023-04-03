namespace DataModel
{
    public class DeliveryMan : EntityBase
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public bool IsWorking { get; set; }
    }
}