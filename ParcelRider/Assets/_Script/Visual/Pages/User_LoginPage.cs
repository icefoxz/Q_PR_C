using System;
using System.Linq;
using Controllers;
using Core;
using OrderHelperLib.DtoModels.Users;
using UnityEngine.UI;
using Views;

public class User_LoginPage : PageUiBase
{
    private View_loginSect view_loginSect { get; }
    private View_RegSect view_regSect { get; }
    private LoginController LoginController => App.GetController<LoginController>();
    private event Action OnLoggedInEvent;
    public User_LoginPage(IView v, Action onLoggedInAction, UiManagerBase uiManager) : base(v, uiManager)
    {
        OnLoggedInEvent += onLoggedInAction;
        OnLoggedInEvent += Hide;
        view_regSect = new View_RegSect(v.GetObject<View>("view_regSect"),
            () => LoginController.RequestRegister(view_regSect.GetRegisterModel(), OnRegisterCallback)
        );
        view_loginSect = new View_loginSect(v: v.GetObject<View>("view_loginSect"), onLoginAction: arg =>
            {
                var (username, password) = arg;
                LoginController.RequestLogin(username, password, OnLoginCallback);
            },
            onGoogleAction: () => LoginController.RequestGoogle(OnLoginCallback),
            onFacebookAction: () => LoginController.RequestFacebook(OnLoginCallback),
            onRegAction: () => view_regSect.Show());
    }

    //register
    private void OnRegisterCallback((bool isSuccess, string message) obj)
    {
        var (isSuccess, message) = obj;
        if (isSuccess)
        {
            view_regSect.Hide();
            OnLoggedInEvent?.Invoke();
            return;
        }
        view_regSect.SetErrorMessage(message);
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
        private Button btn_google { get; }
        private Button btn_facebook { get; }
        private Button btn_register { get; }
        private Text text_errMsg { get; }

        private string Username => input_username.text;
        private string Password => input_password.text;

        public View_loginSect(IView v,Action<(string username, string password)> onLoginAction,
            Action onGoogleAction,
            Action onFacebookAction,
            Action onRegAction) : base(v)
        {
            input_username = v.GetObject<InputField>("input_username");
            input_password = v.GetObject<InputField>("input_password");
            btn_login = v.GetObject<Button>("btn_login");
            btn_google = v.GetObject<Button>("btn_google");
            btn_facebook = v.GetObject<Button>("btn_facebook");
            btn_register = v.GetObject<Button>("btn_register");
            text_errMsg = v.GetObject<Text>("text_errMsg");
            btn_login.OnClickAdd(() =>
            {
                SetMessage();
                onLoginAction((input_username.text, input_password.text));
            });
            btn_google.OnClickAdd(() =>
            {
                SetMessage();
                onGoogleAction?.Invoke();
            });
            btn_facebook.OnClickAdd(() =>
            {
                SetMessage();
                onFacebookAction?.Invoke();
            });
            btn_register.OnClickAdd(() =>
            {
                SetMessage();
                onRegAction?.Invoke();
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

    private class View_RegSect : UiBase
    {
        private enum Inputs
        {
            Username,
            Phone,
            Name,
            Email,
            Password,
            RePassword,
        }
        private Element_input element_input_username { get; }
        private Element_input element_input_phone { get; }
        private Element_input element_input_name { get; }
        private Element_input element_input_email { get; }
        private Element_input element_input_password { get; }
        private Element_input element_input_rePassword { get; }
        private Text text_errMessage { get; }
        private Button btn_register { get; }
        private Button btn_x { get; }

        private Element_input[] Elements { get; }

        public View_RegSect(IView v,Action onRegisterAction) : base(v, false)
        {
            element_input_username = new Element_input(v.GetObject<View>("element_input_username"), value => OnValueChange(value,Inputs.Username));
            element_input_phone = new Element_input(v.GetObject<View>("element_input_phone"), value => OnValueChange(value,Inputs.Phone));
            element_input_name = new Element_input(v.GetObject<View>("element_input_name"), value => OnValueChange(value, Inputs.Name));
            element_input_email = new Element_input(v.GetObject<View>("element_input_email"), value => OnValueChange(value, Inputs.Email));
            element_input_password = new Element_input(v.GetObject<View>("element_input_password"), value => OnValueChange(value, Inputs.Password));
            element_input_rePassword = new Element_input(v.GetObject<View>("element_input_rePassword"), value => OnValueChange(value, Inputs.RePassword));
            text_errMessage = v.GetObject<Text>("text_errMessage");
            btn_register = v.GetObject<Button>("btn_register");
            btn_x = v.GetObject<Button>("btn_x");
            btn_register.onClick.AddAction(onRegisterAction);
            btn_x.onClick.AddAction(Hide);

            Elements = new[]
            {
                element_input_username,
                element_input_phone,
                element_input_name,
                element_input_email,
                element_input_password,
                element_input_rePassword,
            };
            SetErrorMessage();
            UpdateRegButton();
        }

        private void OnValueChange(string value, Inputs input)
        {
            switch (input)
            {
                case Inputs.Username:
                {
                    var isValid = IsValid(input, value);
                    element_input_username.SetValid(isValid);
                    break;
                }
                case Inputs.Phone:
                {
                    var isValid = IsValid(input, value);
                    element_input_phone.SetValid(isValid);
                    break;
                }
                case Inputs.Name:
                {
                    var isValid = IsValid(input, value);
                    element_input_name.SetValid(isValid);
                    break;
                }
                case Inputs.Email:
                {
                    var isValid = IsValid(input, value);
                    element_input_email.SetValid(isValid);
                    break;
                }
                case Inputs.Password:
                {
                    var isValid = IsValid(input, value);
                    element_input_password.SetValid(isValid);
                    break;
                }
                case Inputs.RePassword:
                {
                    var isValid = IsValid(input, value);
                    var isRePwdValid = isValid 
                                       && element_input_password.Value 
                                       == element_input_rePassword.Value;
                    element_input_rePassword.SetValid(isRePwdValid);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(input), input, null);
            }
            UpdateRegButton();
        }

        private void UpdateRegButton() => btn_register.interactable = Elements.All(e => e.IsValid);

        private bool IsValid(Inputs input, string value)
        {
            return input switch
            {
                Inputs.Username => !string.IsNullOrEmpty(value) && value.Length > 4,
                Inputs.Phone => !string.IsNullOrEmpty(value) && value.Length > 8,
                Inputs.Name => !string.IsNullOrEmpty(value),
                Inputs.Email => true,
                Inputs.Password => !string.IsNullOrEmpty(value) && value.Length > 4,
                Inputs.RePassword => !string.IsNullOrEmpty(value) && value.Length > 4,
                _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
            };
        }

        public void SetErrorMessage(string message = null) =>
            text_errMessage.text = message ?? string.Empty;

        private class Element_input : UiBase
        {
            private InputField input_value { get; }
            private Image img_selected { get; }

            public string Value => input_value.text;
            public bool IsValid { get; private  set; }
            public Element_input(IView v,Action<string> onValueChanged) : base(v)
            {
                input_value = v.GetObject<InputField>("input_value");
                img_selected = v.GetObject<Image>("img_selected");
                input_value.onValueChanged.AddListener(value => onValueChanged(value));
                IsValid = false;
            }

            private void SetHighlight(bool selected) => img_selected.gameObject.SetActive(selected);

            public void SetValid(bool isValid)
            {
                IsValid = isValid;
                SetHighlight(!isValid);
            }
        }

        public RegisterDto GetRegisterModel()
        {
            return new ()
            {
                Username = element_input_username.Value,
                Phone = element_input_phone.Value,
                Name = element_input_name.Value,
                Email = element_input_email.Value,
                Password = element_input_password.Value,
            };
        }
    }
}
