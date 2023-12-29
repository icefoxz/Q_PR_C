namespace OrderHelperLib.Req_Models.Riders
{
    public record Rider_DoStatusUpdateDto
    {
        public int DeliveryOrderId { get; set; }
        public int Status { get; set; }
        public Rider_DoStatusUpdateDto()
        {

        }
    }
}