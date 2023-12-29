using OrderHelperLib;
using OrderHelperLib.Dtos.DeliveryOrders;
using OrderHelperLib.Dtos.Riders;
using OrderHelperLib.Dtos.Users;
using System;
using AOT.Utl;
using UnityEngine;

[CreateAssetMenu(fileName = "riderLoginControllerApiSo", menuName = "TestServices/RiderLoginApi")]
public class RiderLoginServiceSo : ScriptableObject
{
    [SerializeField] private RiderModelField _loginMode;

    public (bool isSuccess, string Message) GetLoginService(string username) => _loginMode.Response(username);

    public string SetRiderInfo(DeliverOrderModel order) => _loginMode.GetRiderInfo(order);

    [Serializable]private class RiderModelField
    {
        public bool NoSuchRider;
        public bool IsPasswordFailed;
        public int R_Id;
        public string UserId;
        public string UserName;
        public string Name;
        public string Phone;

        public (bool isSuccess, string databag) Response(string username)
        {
            if (username != UserName) return (false, "Rider not found!");
            if (IsPasswordFailed) return (false, "Password is wrong");
            return (true, DataBag.Serialize(new UserModel
            {
                Id = UserId,
                Username = UserName,
                Name = Name,
                Phone = Phone,
            }));
        }

        public string GetRiderInfo(DeliverOrderModel order)
        {
            var newOrder = order with
            {
                RiderId = 0001.ToString(),
                Rider = new RiderModel
                {
                    Name = null,
                    Phone = null,
                    Id = 0.ToString()
                }
            };
            return(DataBag.Serialize(newOrder));
        }
    }
}
