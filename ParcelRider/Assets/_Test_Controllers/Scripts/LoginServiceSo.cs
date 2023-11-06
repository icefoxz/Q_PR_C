using System;
using OrderHelperLib;
using OrderHelperLib.Dtos.Users;
using UnityEngine;

[CreateAssetMenu(fileName = "loginControllerApiSo", menuName = "TestServices/LoginControllerApi")]
public class LoginServiceSo : ScriptableObject
{
    [SerializeField] private UserModelField _loginModel;

    public (bool isSuccess, string message) ThirdPartyLoginService() => _loginModel.ThirdPartyResponse();
    public (bool isSuccess, string message) UserLoginService(string username) => _loginModel.UserLoginResponse(username);

    [Serializable]private class UserModelField
    {
        public bool NoSuchUser;
        public bool IsPasswordFailed;
        public string Username;
        public string Name;
        public string Email;
        public string Phone;

        public (bool isSuccess,string databag) ThirdPartyResponse()
        {
            if (IsPasswordFailed) return (false, "Password is wrong!");
            return (true, DataBag.Serialize(new UserModel
            {
                Username = Username,
                Name = Name,
                Email = Email,
                Phone = Phone
            }));
        }
        internal (bool isSuccess, string message) UserLoginResponse(string username)
        {
            if (!username.Equals(Username,StringComparison.InvariantCultureIgnoreCase)) return (false, "User not found!");
            if(IsPasswordFailed) return (false, "Password is wrong!");
            return (true, DataBag.Serialize(new UserModel
            {
                Username = Username,
                Name = Name,
                Email = Email,
                Phone = Phone
            }));
        }
    }
}