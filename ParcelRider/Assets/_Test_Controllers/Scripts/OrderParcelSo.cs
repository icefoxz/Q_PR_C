using OrderHelperLib;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OrderSo", menuName = "TestServices/OrderSo")]
public class OrderParcelSo : ScriptableObject
{
    [SerializeField] private OrderModelField _orderModel;
    public (bool isSuccess, string message) CreateOrderService() => _orderModel.CreateOrderResponse();
    public (bool isSuccess, string message) PaymentOrderService() => _orderModel.OrderPaymentResponse();
    public (bool isSuccess, DeliveryOrderStatus status) GetOrderService() => _orderModel.CancelOrderResponse();
    [Serializable]private class OrderModelField
    {
        public bool PaymentMade;
        public DeliveryOrderStatus Status;
        public PaymentMethods PayMethods;
        public int OrderId;
        public string UserId;
        public string ReceiverInfo;
        public DeliverOrderModel orderModel;
        
        public (bool isSuccess, DeliveryOrderStatus databag) CancelOrderResponse()
        {
            return (true, DeliveryOrderStatus.Exception);
        }
        public (bool isScuccess, string databag) CreateOrderResponse()
        {
            return (true, DataBag.Serialize(new DeliverOrderModel
            {
                Id = OrderId,
                UserId = UserId,
                Status = (int)Status
            }));
        }
        public (bool isSuccess, string databag) OrderPaymentResponse()
        {
            if (!PaymentMade) return (false, "Payment Unsuccess, try again");
            return (true, "Payment Success");
        }
    }
}
