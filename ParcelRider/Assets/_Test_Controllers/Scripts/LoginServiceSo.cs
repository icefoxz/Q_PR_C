using System;
using OrderHelperLib;
using OrderHelperLib.Dtos.Users;
using UnityEngine;

[CreateAssetMenu(fileName = "loginControllerApiSo", menuName = "TestServices/LoginControllerApi")]
public class LoginServiceSo : ScriptableObject
{
    [SerializeField] private UserModelField _loginModel;

    public (bool isSuccess, string message) GetLoginService() => _loginModel.Response();
    [Serializable]private class UserModelField
    {
        public bool NoSuchUser;
        public bool IsPasswordFailed;
        public string Username;
        public string Name;
        public string Email;
        public string Phone;

        public (bool isSuccess,string databag) Response()
        {
            if (NoSuchUser) return (false, "User not found!");
            if (IsPasswordFailed) return (false, "Password is wrong!");
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