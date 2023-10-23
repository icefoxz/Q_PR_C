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
    public void CancelOrder(int orderId) => _activeModel.CancelOrderToList(orderId);
    public void SetPayment(PaymentMethods payM) => _activeModel.SetPayment(payM);

    //Update order status
    public (bool isSuccess, int status, int ordId) OrderAssigned(int orderId) => _activeModel.OrderAssignedResponse(orderId);
    public (bool isSuccess, int status, int ordId) ItemPicked(int orderId) => _activeModel.ItemPickedResponse(orderId);
    public (bool isSuccess, int status, int oId) ItemCollected(int orderId) => _activeModel.ItemCollectedResponse(orderId);
    public (bool isSuccess, int status, int ordId) DeliveryComplete(int orderId) => _activeModel.DeliveryCompleteResponse(orderId);

    [Serializable] private class ActiveOrderField
    {
        public int Count;
        public int CurrentId;
        public List<DeliverOrderModel> deliverOrders = new List<DeliverOrderModel>();
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
                            PaymentInfo = new PaymentInfo
                            {
                                Charge = random.Next(1, 100),
                                Fee = random.Next(1, 100),
                                Method = PaymentMethods.UserCredit
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
                            Tags = new List<TagDto>(),
                            RiderId = random.Next(1, 100),
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
        public void CancelOrderToList(int orderId)
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
            deliverOrders[index].PaymentInfo.Method = payM;
            SetNewList(deliverOrders);
        }

        private DeliverOrderModel GetOrder(int currentId)
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
        public (bool isSuccess, int status, int ordId) OrderAssignedResponse(int orderId)
        {
            DeserializeList();
            var order = GetOrder(orderId);
            var index = deliverOrders.IndexOf(GetOrder(orderId));
            var assigned = (int)DeliveryOrderStatus.Assigned;
            deliverOrders[index].Status = assigned;
            SetNewList(deliverOrders);
            return (true, deliverOrders[index].Status, order.Id);
        }

        internal (bool isSuccess, int status, int ordId) ItemPickedResponse(int orderId)
        {
            DeserializeList();
            var order = GetOrder(orderId);
            var index = deliverOrders.IndexOf(GetOrder(orderId));
            var assigned = (int)DeliveryOrderStatus.Delivering;
            deliverOrders[index].Status = assigned;
            SetNewList(deliverOrders);
            return (true, deliverOrders[index].Status, order.Id);
        }

        internal (bool isSuccess, int status, int oId) ItemCollectedResponse(int orderId)
        {
            DeserializeList();
            var order = GetOrder(orderId);
            var index = deliverOrders.IndexOf(GetOrder(orderId));
            var assigned = (int)DeliveryOrderStatus.Completed;
            deliverOrders[index].Status = assigned;
            SetNewList(deliverOrders);
            return (true, deliverOrders[index].Status, order.Id);
        }

        internal (bool isSuccess, int status, int ordId) DeliveryCompleteResponse(int orderId)
        {
            DeserializeList();
            var order = GetOrder(orderId);
            var index = deliverOrders.IndexOf(GetOrder(orderId));
            var assigned = (int)DeliveryOrderStatus.Close;
            deliverOrders[index].Status = assigned;
            SetNewList(deliverOrders);
            return (true, deliverOrders[index].Status, order.Id);
        }
        #endregion
    }
}

