using System;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using UnityEngine;

[CreateAssetMenu(fileName = "OrderSo", menuName = "TestServices/OrderSo")]
public class OrderParcelSo : ScriptableObject
{
    [SerializeField] private OrderModelField _orderModel;
    public (bool isSuccess, string message) CreateOrderService(DeliverOrderModel order) => _orderModel.CreateOrderResponse(order);
    public (bool isSuccess, string message, PaymentMethods methods) PaymentOrderService(PaymentMethods payM) => _orderModel.OrderPaymentResponse(payM);
    public (bool isSuccess, DeliveryOrderStatus status, long OrdId) GetOrderService(long orderId) => _orderModel.CancelOrderResponse(orderId);
    [Serializable]private class OrderModelField
    {
        public bool PaymentMade;
        public DeliveryOrderStatus Status;
        public PaymentMethods PayMethods;
        public long OrderId;
        public string UserId;
        
        public (bool isSuccess, DeliveryOrderStatus databag, long OrdId) CancelOrderResponse(long orderId)
        {
            OrderId = orderId;
            return (true, Status, OrderId);
        }
        public (bool isSuccess, string message) CreateOrderResponse(DeliverOrderModel order)
        {
            var random = new System.Random();
            var newOrder = order with
            {
                Id = random.Next(1000, 9999),
                UserId = UserId,
            };
            //order.Id = 0; //struct: newOrder.Id != 0; //class: newOrder.Id == 0;
            OrderId = newOrder.Id;
            return (true, DataBag.Serialize(newOrder));
        }
        public (bool isSuccess, string databag, PaymentMethods methods) OrderPaymentResponse(PaymentMethods payM)
        {
            PayMethods = payM;
            if (!PaymentMade) return (false, "Payment Unsuccessful, try again", PayMethods);
            return (true, "Payment Success", PayMethods);
        }
    }
}