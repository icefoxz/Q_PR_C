using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using OrderHelperLib.Dtos.Riders;
using OrderHelperLib.Dtos.Users;
using OrderHelperLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "ActiveOrderSo", menuName ="TestServices/ActiveOrderSo")]
public class ActiveOrderSo : ScriptableObject
{
    [SerializeField] private ActiveOrderField _activeModel;
    public string GetActiveOrderList() => _activeModel.GetActiveOrderList();
    public void SetNewOrder(DeliverOrderModel order) => _activeModel.SetNewOrderToList(order);
    public void CancelOrder(long orderId) => _activeModel.CancelOrderToList(orderId);
    public void SetPayment(PaymentMethods payM) => _activeModel.SetPayment(payM);
    public (string order, int state) DoStateUpdate(string order, int stateId) => _activeModel.DoState_Update(order, stateId);

    //Update order status
    public (bool isSuccess, int status, long ordId) OrderAssigned(DeliverOrderModel order) => _activeModel.OrderAssignedResponse(order);
    public (bool isSuccess, int status, long ordId) ItemPicked(long orderId) => _activeModel.ItemPickedResponse(orderId);
    public (bool isSuccess, int status, long oId) ItemCollected(long orderId) => _activeModel.ItemCollectedResponse(orderId);
    public (bool isSuccess, int status, long ordId) DeliveryComplete(long orderId) => _activeModel.DeliveryCompleteResponse(orderId);

    [Serializable] private class ActiveOrderField
    {
        public int Count;
        public long CurrentId;
        public List<DeliverOrderModel> deliverOrders = new List<DeliverOrderModel>();
        [TextArea(5,20)]
        public string DoList;
        public string GetActiveOrderList()
        {
            if (DoList == string.Empty)
            {
                Debug.Log("Create order list");
                var hOrder = GenerateRandomHistory(Count);
                List<DeliverOrderModel> GenerateRandomHistory(int count)
                {
                    var random = new System.Random();
                    var orders = new List<DeliverOrderModel>();

                    for (int i = 0; i < count; i++)
                    {
                        var order = new DeliverOrderModel
                        {
                            Id = random.Next(1000, 9999),
                            UserId = $"User{random.Next(1000, 9999)}",
                            User = new UserModel
                            {
                                //
                            },
                            MyState = $"State{random.Next(1, 10)}",
                            Status = random.Next(0, 1),
                            ItemInfo = new ItemInfoDto
                            {
                                Weight = random.Next(1, 10),
                                Length = random.Next(1, 10),
                                Width = random.Next(1, 10),
                                Height = random.Next(1, 10)
                            },
                            PaymentInfo = new PaymentInfoDto
                            {
                                Charge = random.Next(1, 100),
                                Fee = random.Next(1, 100),
                                Method = PaymentMethods.UserCredit.ToString()
                            },
                            DeliveryInfo = new DeliveryInfoDto
                            {
                                Distance = random.Next(1, 100),
                                StartLocation = new LocationDto
                                {
                                    Address = $"Address{random.Next(1, 10)}",
                                    Latitude = random.NextDouble() * (90.0 - -90.0) + -90.0,
                                    Longitude = random.NextDouble() * (180.0 - -180.0) + -180.0
                                },
                                EndLocation = new LocationDto
                                {
                                    Address = $"Address{random.Next(1, 10)}",
                                    Latitude = random.NextDouble() * (90.0 - -90.0) + -90.0,
                                    Longitude = random.NextDouble() * (180.0 - -180.0) + -180.0
                                }
                            },
                            SenderInfo = new SenderInfoDto
                            {
                                User = new UserModel
                                {
                                    //
                                },
                                UserId = $"User{random.Next(1000, 9999)}"
                            },
                            ReceiverInfo = new ReceiverInfoDto
                            {
                                PhoneNumber = $"PhoneNumber{random.Next(1000, 9999)}",
                                Name = $"Name{random.Next(1000, 9999)}"
                            },
                            RiderId = random.Next(1, 100).ToString(),
                            Rider = new RiderModel
                            {
                                //
                            }
                        };
                        orders.Add(order);
                    }
                    return orders;
                }
                deliverOrders.AddRange(hOrder);
                var data = DataBag.Serialize(deliverOrders);
                DoList = data;
            }
            Debug.Log("Get data from SO");
            return DoList;
        }

        public void SetNewOrderToList(DeliverOrderModel order)
        {
            DeserializeList();
            CurrentId = order.Id;
            deliverOrders.Add(order);
            SetNewList(deliverOrders);
        }
        public void CancelOrderToList(long orderId)
        {
            DeserializeList();
            var order = deliverOrders.FirstOrDefault(o => o.Id == orderId);
            deliverOrders.Remove(order);
            SetNewList(deliverOrders);
            CurrentId = -1;
        }
        public void SetPayment(PaymentMethods payM)
        {
            var order = GetOrder(CurrentId);
            var index = deliverOrders.IndexOf(order);
            deliverOrders[index].PaymentInfo.Method = payM.ToString();
            SetNewList(deliverOrders);
        }

        private DeliverOrderModel GetOrder(long currentId)
        {
            var order = deliverOrders.FirstOrDefault(o=> o.Id == currentId);
            return order;
        }

        private void DeserializeList()
        {
            var bag = DataBag.Deserialize(DoList);
            var DoData = bag.Get<List<DeliverOrderModel>>(0);
            deliverOrders.Clear();
            DoList = string.Empty;
            deliverOrders.AddRange(DoData);
        }

        private void SetNewList(List<DeliverOrderModel> deliverOrderModels)
        {
            var data = DataBag.Serialize(deliverOrderModels);
            DoList = data;
        }

        #region Update Order Status
        public (bool isSuccess, int status, long ordId) OrderAssignedResponse(DeliverOrderModel order)
        {
            DeserializeList();
            var getOrder = GetOrder(order.Id);
            var index = deliverOrders.IndexOf(getOrder);
            deliverOrders[index].RiderId = order.RiderId;
            deliverOrders[index].Rider = order.Rider;
            var assigned = (int)DeliveryOrderStatus.Assigned;
            deliverOrders[index].Status = assigned;
            SetNewList(deliverOrders);
            return (true, deliverOrders[index].Status, order.Id);
        }

        internal (bool isSuccess, int status, long ordId) ItemPickedResponse(long orderId)
        {
            DeserializeList();
            var order = GetOrder(orderId);
            var index = deliverOrders.IndexOf(GetOrder(orderId));
            var assigned = (int)DeliveryOrderStatus.Delivering;
            deliverOrders[index].Status = assigned;
            SetNewList(deliverOrders);
            return (true, deliverOrders[index].Status, order.Id);
        }

        internal (bool isSuccess, int status, long oId) ItemCollectedResponse(long orderId)
        {
            DeserializeList();
            var order = GetOrder(orderId);
            var index = deliverOrders.IndexOf(GetOrder(orderId));
            var assigned = (int)DeliveryOrderStatus.Completed;
            deliverOrders[index].Status = assigned;
            SetNewList(deliverOrders);
            return (true, deliverOrders[index].Status, order.Id);
        }

        internal (bool isSuccess, int status, long ordId) DeliveryCompleteResponse(long orderId)
        {
            DeserializeList();
            var order = GetOrder(orderId);
            var index = deliverOrders.IndexOf(GetOrder(orderId));
            var assigned = (int)DeliveryOrderStatus.Closed;
            deliverOrders[index].Status = assigned;
            SetNewList(deliverOrders);
            return (true, deliverOrders[index].Status, order.Id);
        }
        #endregion

        public (string order, int state) DoState_Update(string order, int stateId)
        {
            return (order, stateId);
        }
    }

}

