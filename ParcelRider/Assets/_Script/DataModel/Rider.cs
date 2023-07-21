using OrderHelperLib.DtoModels.DeliveryOrders;

namespace DataModel
{
    public class Rider : EntityBase<string>
    {
        public string Name { get; set; }
        public string Phone{ get; set; }
        public string Location { get; set; }

        public Rider()
        {
            
        }

        public Rider(RiderDto r)
        {
            Id = r.Id.ToString();
            Name = r.Name;
            Phone = r.Phone;
            Location = r.Location;
        }
    }
}