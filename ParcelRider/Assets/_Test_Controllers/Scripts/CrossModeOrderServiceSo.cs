using OrderHelperLib;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CrossOrderControllerSo", menuName = "TestServices/CrossOrderApi")]
public class CrossModeOrderServiceSo : ScriptableObject
{
    [SerializeField] private CrossOrdelModel _crossOrderModel;
    public (bool isSuccess, int status, int orderId) AssignRider(int orderId) => _crossOrderModel.AssignedRider(orderId);
    public (int status, int ordId) Complete(int orderId) => _crossOrderModel.Complete(orderId);
    public string GetOrders() => _crossOrderModel.GetOrders();
    public (bool isSuccess, int status, int oId) ItemCollection(int orderId) => _crossOrderModel.ItemCollection(orderId);
    public (bool isSuccess, string databag) PickItem(DeliverOrderModel orderId) => _crossOrderModel.PickItem(orderId);

    public void SetData(DeliverOrderModel data) => _crossOrderModel.SetData(data);
    [Serializable]private class CrossOrdelModel
    {
        [TextArea(3,20)]
        public string data;
        public int OrderId;
        public void SetData(DeliverOrderModel doData)
        {
            data = DataBag.Serialize(doData);
        }
        public string GetOrders()
        {
            return data;
        }
        public (bool isSuccess, int status, int OrdId)  AssignedRider(int orderId)
        {
            OrderId = orderId;
            return(true, (int)DeliveryOrderStatus.Assigned, OrderId);
        }
        public (bool isSuccess, string databag) PickItem(DeliverOrderModel orderId)
        {
            var newStatus = orderId with
            {
                Status = (int)DeliveryOrderStatus.Delivering
            };
            return (true, DataBag.Serialize(newStatus));
        }
        public (bool isSuccess, int status, int oId) ItemCollection(int orderId)
        {
            OrderId = orderId;
            return (true, (int)DeliveryOrderStatus.Completed, OrderId);
        }

        public (int status, int ordId) Complete(int orderId)
        {
            OrderId = orderId;
            return ((int)DeliveryOrderStatus.Close , OrderId);
        }
    }
}
