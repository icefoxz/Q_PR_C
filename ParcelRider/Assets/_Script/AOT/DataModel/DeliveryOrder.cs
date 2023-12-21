using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;

namespace AOT.DataModel
{
    public record DeliveryOrder : DeliverOrderModel
    {
        public DeliveryOrderStatus State => (DeliveryOrderStatus)Status;

        public DeliveryOrder() { }

        public DeliveryOrder(DeliverOrderModel dto): base(dto)
        {
        }

        public void SetPaymentMethod(PaymentMethods paymentMethod)
        {
            PaymentInfo.Method = paymentMethod.ToString();
        }
    }
}