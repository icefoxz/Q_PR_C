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
            ApiPanel.Login(username, password, obj =>
            {
                App.Models.SetUser(obj.User);
                callback?.Invoke((true, string.Empty));
            }, msg =>
                callback?.Invoke((false, msg)));
        }

        public void RequestGoogle(Action<bool> callback)
        {
            var isSuccess = true;
            var message = string.Empty;
            callback?.Invoke(isSuccess);
        }
        public void RequestFacebook(Action<bool> callback)
        {
            var isSuccess = true;
            var message = string.Empty;
            callback?.Invoke(isSuccess);
        }

        public void RequestRegister(RegisterDto registerModel,
            Action<(bool isSuccess, string message)> onCallbackAction)
        {
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
