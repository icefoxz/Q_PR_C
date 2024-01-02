using OrderHelperLib.Contracts;
using OrderHelperLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AOT.Utl;
using OrderHelperLib.Dtos.DeliveryOrders;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;
using DoStateMap = OrderHelperLib.Contracts.DoStateMap;
using Random = System.Random;

[CreateAssetMenu(fileName = "OrderSimSo", menuName ="TestServices/OrderSimSo")]
public class OrderSimSo : ScriptableObject
{
    [FormerlySerializedAs("_activeModel")][SerializeField] private OrderField _field;
    
    public string GetOrders() => _field.GetOrders();
    public DeliverOrderModel GetOrder(long orderId) => _field.GetOrderFromList(orderId);
    public void SetNewOrder(DeliverOrderModel order) => _field.SetNewOrderToList(order);
    public void CancelOrder(long orderId) => _field.CancelOrderToList(orderId);
    public (bool isSuccess, string message) PaymentRiderCollect() => _field.RiderCollectPay();
    public (bool isSuccess, string message) PaymentCreditDeduction() => _field.CreditDeductPay();
    public (string order, string state) DoStateUpdate(DeliverOrderModel order, string stateId) => _field.DoState_Update(order, stateId);

    //Update order status
    public (bool isSuccess, string order) OrderAssigned(DeliverOrderModel order) => _field.OrderAssignedResponse(order);

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
        }
        public void CancelOrderToList(long orderId)
        {
            var order = _preset.FirstOrDefault(o => o.Id == orderId);
            _preset.Remove(order);
            CurrentId = -1;
        }

        private Model_Do GetOrder(long currentId)
        {
            var order = _preset.FirstOrDefault(o=> o.Id == currentId);
            return order;
        }

        public (bool isSuccess, string order) OrderAssignedResponse(DeliverOrderModel model)
        {
            var order = model.JMap<DeliverOrderModel, Model_Do>();
            var o = GetOrder(order.Id);
            var assigned = (int)DeliveryOrderStatus.Assigned;
            var state = DoStateMap.GetState(DoSubState.AssignState);
            o.RiderId = order.RiderId;
            o.Rider = order.Rider;
            o.Status = assigned;
            o.SubState = state.StateId;
            var histories = o.StateHistory.ToList();
            histories.Add(new Info_StageSegment{SubState = state.StateId, Timestamp = DateTime.Now});
            o.StateHistory = histories.ToArray();
            var bag = DataBag.Serialize(o);
            return (true, bag);
        }

        public (string order, string state) DoState_Update(DeliverOrderModel order, string stateId)
        {
            var o = GetOrder(order.Id);
            var state = DoStateMap.GetState(stateId);
            o.SubState = state.StateId;
            o.Status = state.Status;
            var histories = o.StateHistory.ToList();
            histories.Add(new Info_StageSegment{SubState =  stateId, Timestamp = DateTime.Now});
            o.StateHistory = histories.ToArray();
            var bag = DataBag.Serialize(o);
            return (bag, stateId);
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