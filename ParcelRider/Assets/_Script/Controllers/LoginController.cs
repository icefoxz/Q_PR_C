using System;
using Core;
using UnityEngine;

namespace Controllers
{
    internal class LoginController : IController
    {
        public async void RequestLogin(string username, string password,
            Action<(bool isSuccess, string message)> callback)
        {
            var isSuccess = true;
            var message = string.Empty;
            callback?.Invoke((isSuccess, message));
        }
        public async void RequestGoogle(Action<bool> callback)
        {
            var isSuccess = true;
            var message = string.Empty;
            callback?.Invoke(isSuccess);
        }
        public async void RequestFacebook(Action<bool> callback)
        {
            var isSuccess = true;
            var message = string.Empty;
            callback?.Invoke(isSuccess);
        }

        public async void RequestRegister(Action<(bool isSuccess,string message)> onCallbackAction)
        {
            var isSuccess = true;
            var message = string.Empty;
            onCallbackAction?.Invoke((isSuccess, message));
        }

        public string GetAccountName() => "Test";

        public Sprite GetUserAvatar() => null;

        public async void CheckLoginStatus(Action<bool> onLoginAction)
        {
            onLoginAction?.Invoke(true);//暂时都是success
        }
    }
}
