using OrderHelperLib;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using OrderHelperLib.Dtos.Riders;
using OrderHelperLib.Dtos.Users;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HistorySo", menuName ="TestServices/HistorySo")]
public class HistoryOrderSo : ScriptableObject
{
    [SerializeField] private HistoryOrderField _historyModel;
    public string GetHistoryList() => _historyModel.GetHistoryOrderList();

    [Serializable]private class HistoryOrderField
    {
        public int Count;
        public string GetHistoryOrderList()
        {
            var hOrder = GenerateRandomHistory(Count);

            List<DeliverOrderModel> GenerateRandomHistory(int count)
            {
                var random = new System.Random();
                var orders = new List<DeliverOrderModel>();

                for(int i = 0; i < count; i++)
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
                        Status = -1,
                        ItemInfo = new ItemInfoDto
                        {
                            Weight = random.Next(1, 10),
                            Length = random.Next(1, 10),
                            Width = random.Next(1, 10),
                            Height = random.Next(1, 10)
                        },
                        PaymentInfo = new PaymentInfo
                        {
                            Charge = random.Next(1,100),
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
                        Tags = new List<TagDto>(),
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
            return DataBag.Serialize(hOrder);
        }
    }
}
