namespace OrderHelperLib.Req_Models.Riders;

public record Rider_AssignmentDto
{
    public int DeliveryOrderId { get; set; }
    public int RiderId { get; set; }

    public Rider_AssignmentDto()
    {
        
    }
}