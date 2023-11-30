using System;
using AOT.BaseUis;
using AOT.Controllers;
using AOT.Core;
using AOT.Views;
using UnityEngine.UI;

namespace Visual.Pages
{
    public class RiderLoginPage : PageUiBase
    {
        private View_loginSect view_loginSect { get; }
        private RiderLoginController RiderLoginController => App.GetController<RiderLoginController>();
        private event Action OnLoggedInEvent;
        public RiderLoginPage(IView v, Action onLoggedInAction, UiManagerBase uiManager) : base(v, uiManager)
        {
            OnLoggedInEvent += onLoggedInAction;
            OnLoggedInEvent += Hide;
            view_loginSect = new View_loginSect(v: v.Get<View>("view_loginSect"), onLoginAction: arg =>
            {
                var (username, password) = arg;
                RiderLoginController.Rider_RequestLogin(username, password, OnLoginCallback);
            });
            App.RegEvent(EventString.Rider_Logout, _ => Show());
        }

        //login
        private void OnLoginCallback(bool isSuccess)
        {
            var message = isSuccess ? "Login Success!" : "Login failed";
            OnLoginCallback((isSuccess, message));
        }
        private void OnLoginCallback((bool isSuccess, string message) obj)
        {
            var (isSuccess, message) = obj;
            if (isSuccess)
            {
                OnLoggedInEvent?.Invoke();
                return;
            }
            view_loginSect.SetMessage(message);
        }

        private class View_loginSect : UiBase
        {
            private InputField input_username { get; }
            private InputField input_password { get; }
            private Button btn_login { get; }
            private Text text_errMsg { get; }

            private string Username => input_username.text;
            private string Password => input_password.text;

            public View_loginSect(IView v, Action<(string username, string password)> onLoginAction) : base(v)
            {
                input_username = v.Get<InputField>("input_username");
                input_password = v.Get<InputField>("input_password");
                btn_login = v.Get<Button>("btn_login");
                text_errMsg = v.Get<Text>("text_errMsg");
                btn_login.OnClickAdd(() =>
                {
                    SetMessage();
                    onLoginAction((input_username.text, input_password.text));
                });
                input_password.onValueChanged.AddListener(_ => UpdateLoginButtonInteractable());
                input_username.onValueChanged.AddListener(_ => UpdateLoginButtonInteractable());
                SetMessage();
                UpdateLoginButtonInteractable();
            }

            public void SetMessage(string message = null)
            {
                message ??= string.Empty;
                text_errMsg.text = message;
            }

            public void UpdateLoginButtonInteractable() =>
                btn_login.interactable =
                    Username?.Length > 4 &&
                    Password?.Length > 4;
        }
    }
}