using OrderHelperLib;
using OrderHelperLib.Dtos.Riders;
using OrderHelperLib.Dtos.Users;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "riderLoginControllerApiSo", menuName = "TestServices/RiderLoginApi")]
public class RiderLoginServiceSo : ScriptableObject
{
    [SerializeField] private RiderModelField _loginMode;

    public (bool isSuccess, string Message) GetLoginService(string username) => _loginMode.Response(username);
    [Serializable]private class RiderModelField
    {
        public bool NoSuchRider;
        public bool IsPasswordFailed;
        public string Id;
        public string UserName;
        public string Name;
        public string Phone;

        public (bool isSuccess, string databag) Response(string username)
        {
            if (username != UserName) return (false, "Rider not found!");
            if (IsPasswordFailed) return (false, "Password is wrong");
            return (true, DataBag.Serialize(new UserModel
            {
                Id = Id,
                Username = UserName,
                Name = Name,
                Phone = Phone,
            }));
        }
    }
}
