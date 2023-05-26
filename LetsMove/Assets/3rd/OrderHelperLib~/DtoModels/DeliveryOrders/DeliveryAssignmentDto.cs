namespace OrderHelperLib.DtoModels.DeliveryOrders
{
    public class DeliveryAssignmentDto
    {
        public int DeliveryOrderId { get; set; }
        public int RiderId { get; set; }
    }

    public class DeliverySetStatusDto
    {
        public int DeliveryOrderId { get; set; }
        public int Status { get; set; }
    }
}