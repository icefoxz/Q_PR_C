using OrderHelperLib.Contracts;
using OrderHelperLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using OrderHelperLib.Dtos.DeliveryOrders;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine.Serialization;
using Random = System.Random;

[CreateAssetMenu(fileName = "OrderSimSo", menuName ="TestServices/OrderSimSo")]
public class OrderSimSo : ScriptableObject
{
    [FormerlySerializedAs("_activeModel")][SerializeField] private OrderField _field;
    
    public string GetOrders() => _field.GetOrders();
    public string GetHistories() => _field.GetHistory();
    public DeliverOrderModel GetOrder(long orderId) => _field.GetOrderFromList(orderId);
    public void SetNewOrder(DeliverOrderModel order) => _field.SetNewOrderToList(order);
    public void CancelOrder(long orderId) => _field.CancelOrderToList(orderId);
    public (bool isSuccess, string message) PaymentRiderCollect() => _field.RiderCollectPay();
    public (bool isSuccess, string message) PaymentCreditDeduction() => _field.CreditDeductPay();
    public void SetPayment(PaymentMethods payM) => _field.SetPayment(payM);
    public (string order, int state) DoStateUpdate(string order, int stateId) => _field.DoState_Update(order, stateId);

    //Update order status
    public (bool isSuccess, int status, long ordId) OrderAssigned(DeliverOrderModel order) => _field.OrderAssignedResponse(order);
    public (bool isSuccess, int status, long ordId) ItemPicked(long orderId) => _field.ItemPickedResponse(orderId);
    public (bool isSuccess, int status, long oId) ItemCollected(long orderId) => _field.ItemCollectedResponse(orderId);
    public (bool isSuccess, int status, long ordId) DeliveryComplete(long orderId) => _field.DeliveryCompleteResponse(orderId);

    [Serializable] private class OrderField
    {
                public int Count;
        public long CurrentId;
        public List<Model_Do> _preset;
        public string GetOrders()
        {
            return DataBag.Serialize(_preset);
        }

        [Button(ButtonSizes.Medium),GUIColor("cyan")]public void GenerateOrderToPreset()
        {
            var random = new Random();
            var orders = new List<Model_Do>();

            for (int i = 0; i < Count; i++)
            {
                var order = GenerateOrder(random);
                orders.Add(order);
            }

            _preset = orders;
        }

        private static Model_Do GenerateOrder(Random random)
        {
            var order = new Model_Do
            {
                Id = random.Next(1000, 9999),
                UserId = $"User{random.Next(1000, 9999)}",
                User = new Model_User
                {
                    //
                },
                MyState = $"State{random.Next(1, 10)}",
                Status = random.Next(0, 1),
                ItemInfo = new Info_Item
                {
                    Weight = random.Next(1, 10),
                    Length = random.Next(1, 10),
                    Width = random.Next(1, 10),
                    Height = random.Next(1, 10)
                },
                PaymentInfo = new Info_Payment
                {
                    Charge = random.Next(1, 100),
                    Fee = random.Next(1, 100),
                    Method = PaymentMethods.UserCredit.ToString()
                },
                DeliveryInfo = new Info_Delivery
                {
                    Distance = random.Next(1, 100),
                    StartLocation = new Info_Location
                    {
                        Address = $"Address{random.Next(1, 10)}",
                        Latitude = random.NextDouble() * (90.0 - -90.0) + -90.0,
                        Longitude = random.NextDouble() * (180.0 - -180.0) + -180.0
                    },
                    EndLocation = new Info_Location
                    {
                        Address = $"Address{random.Next(1, 10)}",
                        Latitude = random.NextDouble() * (90.0 - -90.0) + -90.0,
                        Longitude = random.NextDouble() * (180.0 - -180.0) + -180.0
                    }
                },
                SenderInfo = new Info_Sender
                {
                    User = new Model_User
                    {
                        //
                    },
                    UserId = $"User{random.Next(1000, 9999)}"
                },
                ReceiverInfo = new Info_Receiver
                {
                    PhoneNumber = $"PhoneNumber{random.Next(1000, 9999)}",
                    Name = $"Name{random.Next(1000, 9999)}"
                },
                RiderId = random.Next(1, 100).ToString(),
                Rider = new Model_Rider
                {
                    //
                }
            };
            return order;
        }

        public void SetNewOrderToList(DeliverOrderModel order)
        {
            CurrentId = order.Id;
            var data = order.JMap<DeliverOrderModel, Model_Do>();
            _preset.Add(data);
            AssetDatabase.Refresh();
        }
        public void CancelOrderToList(long orderId)
        {
            var order = _preset.FirstOrDefault(o => o.Id == orderId);
            _preset.Remove(order);
            CurrentId = -1;
        }
        public void SetPayment(PaymentMethods payM)
        {
            var order = GetOrder(CurrentId);
            //var index = _preset.IndexOf(order);
            //var mo = _preset[index];
            order.PaymentInfo.Method = payM.ToString();
        }

        private Model_Do GetOrder(long currentId)
        {
            var order = _preset.FirstOrDefault(o=> o.Id == currentId);
            return order;
        }

        #region Update Order Status
        public (bool isSuccess, int status, long ordId) OrderAssignedResponse(DeliverOrderModel model)
        {
            var order = model.Map<DeliverOrderModel, Model_Do>();
            var getOrder = GetOrder(order.Id);
            var index = _preset.IndexOf(getOrder);
            _preset[index].RiderId = order.RiderId;
            _preset[index].Rider = order.Rider;
            var assigned = (int)DeliveryOrderStatus.Assigned;
            _preset[index].Status = assigned;
            return (true, _preset[index].Status, order.Id);
        }

        internal (bool isSuccess, int status, long ordId) ItemPickedResponse(long orderId)
        {
            var order = GetOrder(orderId);
            var index = _preset.IndexOf(GetOrder(orderId));
            var assigned = (int)DeliveryOrderStatus.Delivering;
            _preset[index].Status = assigned;
            return (true, _preset[index].Status, order.Id);
        }

        internal (bool isSuccess, int status, long oId) ItemCollectedResponse(long orderId)
        {
            var order = GetOrder(orderId);
            var index = _preset.IndexOf(GetOrder(orderId));
            var assigned = (int)DeliveryOrderStatus.Completed;
            _preset[index].Status = assigned;
            return (true, _preset[index].Status, order.Id);
        }

        internal (bool isSuccess, int status, long ordId) DeliveryCompleteResponse(long orderId)
        {
            var order = GetOrder(orderId);
            var index = _preset.IndexOf(GetOrder(orderId));
            var assigned = (int)DeliveryOrderStatus.Closed;
            _preset[index].Status = assigned;
            return (true, _preset[index].Status, order.Id);
        }
        #endregion

        public (string order, int state) DoState_Update(string order, int stateId)
        {
            return (order, stateId);
        }

        public string GetHistory()
        {
            return DataBag.Serialize(_preset);
        }

        public DeliverOrderModel GetOrderFromList(long orderId)
        {
            var order = GetOrder(orderId);
            var doOrder = order.JMap<Model_Do, DeliverOrderModel>();
            return doOrder;
        }

        public (bool isSuccess, string message) RiderCollectPay()
        {
            var order = GetOrder(CurrentId);
            order.PaymentInfo.Method = PaymentMethods.RiderCollection.ToString();
            return (true, "Paid: Rider Collection!");
        }

        public (bool isSuccess, string message) CreditDeductPay()
        {
            var order = GetOrder(CurrentId);
            order.PaymentInfo.Method = PaymentMethods.UserCredit.ToString();
            return (true, "Paid: User Credit!");
        }
    }


}