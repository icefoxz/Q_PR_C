using System;
using Core;
using OrderHelperLib.DtoModels.Users;
using UnityEngine;
using Utl;

namespace Controllers
{
    public class LoginController : IController
    {
        public void RequestLogin(string username, string password,
            Action<(bool isSuccess, string message)> callback)
        {
            //App.Models.SetUser(new UserDto
            //{
            //    Username = username,
            //    Name = "Test User",
            //});
            //callback?.Invoke((true, string.Empty));
            ApiPanel.Login(username, password, obj =>
            {
                App.Models.SetUser(obj.User);
                callback?.Invoke((true, string.Empty));
            }, msg =>
                callback?.Invoke((false, msg)));
        }

        public void RequestGoogle(Action<bool> callback)
        {
            App.Models.SetUser(new UserDto
            {
                Username = "username",
                Name = "Test User",
            });

            var isSuccess = true;
            var message = string.Empty;
            callback?.Invoke(isSuccess);
        }
        public void RequestFacebook(Action<bool> callback)
        {
            App.Models.SetUser(new UserDto
            {
                Username = "username",
                Name = "Test User",
            });
            var isSuccess = true;
            var message = string.Empty;
            callback?.Invoke(isSuccess);
        }

        public void RequestRegister(RegisterDto registerModel,
            Action<(bool isSuccess, string message)> onCallbackAction)
        {
            App.Models.SetUser(new UserDto
            {
                Username = registerModel.Username,
                Name = registerModel.Name,
                Phone = registerModel.Phone,
            });
            //onCallbackAction?.Invoke((true, "Login success!"));

            ApiPanel.Register(registerModel, obj =>
            {
                App.Models.SetUser(obj.User);
                onCallbackAction?.Invoke((true, "Login success!"));
            }, msg =>
                onCallbackAction?.Invoke((false, msg)));
        }

        public void CheckLoginStatus(Action<bool> onLoginAction)
        {
            var hasUserIdentity = App.Models.User != null;
            onLoginAction?.Invoke(hasUserIdentity);
        }
    }
}
