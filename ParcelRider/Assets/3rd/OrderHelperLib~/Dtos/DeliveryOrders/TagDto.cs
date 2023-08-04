namespace OrderHelperLib.Dtos.DeliveryOrders;

public record TagDto
{
    public string Type { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}