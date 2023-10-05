using OrderHelperLib;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using OrderHelperLib.Dtos.Users;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OrderSo", menuName = "TestServices/OrderSo")]
public class OrderParcelSo : ScriptableObject
{
    [SerializeField] private OrderModelField _orderModel;
    public (bool isSuccess, string message) CreateOrderService(DeliverOrderModel order) => _orderModel.CreateOrderResponse(order);
    public (bool isSuccess, string message, PaymentMethods methods) PaymentOrderService() => _orderModel.OrderPaymentResponse();
    public (bool isSuccess, DeliveryOrderStatus status, int OrdId) GetOrderService(int orderId) => _orderModel.CancelOrderResponse(orderId);
    [Serializable]private class OrderModelField
    {
        public bool PaymentMade;
        public DeliveryOrderStatus Status;
        public PaymentMethods PayMethods;
        public int OrderId;
        public string UserId;
        public string ReceiverInfo;
        public DeliverOrderModel orderModel;
        
        public (bool isSuccess, DeliveryOrderStatus databag, int OrdId) CancelOrderResponse(int orderId)
        {
            OrderId = orderId;
            return (true, Status, OrderId);
        }
        public (bool isScuccess, string databag) CreateOrderResponse(DeliverOrderModel order)
        {
            var newOrder = order with
            {
                Id = OrderId,
                UserId = UserId,
            };
            //order.Id = 0; //struct: newOrder.Id != 0; //class: newOrder.Id == 0;
            return (true, DataBag.Serialize(newOrder));
        }
        public (bool isSuccess, string databag, PaymentMethods methods) OrderPaymentResponse()
        {
            if (!PaymentMade) return (false, "Payment Unsuccess, try again", PayMethods);
            return (true, "Payment Success", PayMethods);
        }
    }
}