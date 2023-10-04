using System;
using AOT.Core;
using AOT.Test;
using AOT.Utl;
using OrderHelperLib;
using OrderHelperLib.Dtos.Users;
using OrderHelperLib.Req_Models.Users;

namespace AOT.Controllers
{
    public class LoginController : ControllerBase
    {
        private GoogleSignInManager GoogleSignInManager { get; } = new GoogleSignInManager();
        private FacebookSignInManager FacebookSignInManager { get; } = new FacebookSignInManager();

        public LoginController()
        {
            GoogleSignInManager.Init();
            FacebookSignInManager.Init();
        }

        public void RequestLogin(string username, string password,
            Action<(bool isSuccess, string message)> callback)
        {
            Call(testConvertFunc: args => ((bool)args[0], (string)args[1]),
                arg =>
                {
                    var (isSuccess, message) = arg;
                    if (isSuccess)
                    {
                        DeserializeSetModel(message);
                        message = string.Empty;
                    }

                    callback((isSuccess, message));
                },
                () =>
                {
                    #region ServerRequest

                    ApiPanel.User_Login(username, password,
                        successCallbackAction: result =>
                        {
                            OnSuccessSetModel(result.User);
                            callback?.Invoke((true, string.Empty));
                        }, failedCallbackAction: msg => callback?.Invoke((false, msg)));

                    #endregion
                });

        }

        private void DeserializeSetModel(string text)
        {
            var bag = DataBag.Deserialize(text);
            var user = bag.Get<UserModel>(0);
            OnSuccessSetModel(user);
        }

        private void OnSuccessSetModel(UserModel user) => App.Models.SetUser(user);

        public void RequestGoogle(Action<bool> callback)
        {
            Call(args => ((bool)args[0], (string)args[1]), arg =>
            {
                var (isSuccess, message) = arg;
                if (isSuccess)
                {
                    DeserializeSetModel(message);
                }

                callback(isSuccess);
            }, () =>
            {
                if (!GoogleSignInManager.IsInit)
                    GoogleSignInManager.Init();
                GoogleSignInManager.GoogleSignInClick(user =>
                {
                    if (user == null)
                    {
                        callback?.Invoke(false);
                        return;
                    }

                    OnSuccessSetModel(new UserModel()
                    {
                        Username = user.Email,
                        Name = user.DisplayName,
                        AvatarUrl = user.PhotoUrl.ToString(),
                    });
                    callback?.Invoke(true);
                });
            });
        }

        public void RequestFacebook(Action<bool> callback)
        {
            Call(args => ((bool)args[0], (string)args[1]), arg =>
            {
                var (isSuccess, message) = arg;
                if (isSuccess)
                {
                    DeserializeSetModel(message);
                }
                callback(isSuccess);
            }, () =>
            {
                FacebookSignInManager.OnLoginButtonClicked(arg =>
                {
                    var (success, name, email, avatarUrl) = arg;
                    if (!success)
                    {
                        callback?.Invoke(false);
                        return;
                    }

                    OnSuccessSetModel(new UserModel()
                    {
                        Username = email,
                        Name = name,
                        AvatarUrl = avatarUrl,
                    });
                    callback?.Invoke(true);
                });
            });
        } 

        public void RequestRegister(User_RegDto registerModel,
            Action<(bool isSuccess, string message)> onCallbackAction)
        {
            #region TestMode
            if (TestMode)
            {
                App.Models.SetUser(new UserModel()
                {
                    Username = registerModel.Username,
                    Name = registerModel.Name,
                    Phone = registerModel.Phone,
                });
                onCallbackAction?.Invoke((true, "Login success!"));
                return;
            }
            #endregion
            //onCallbackAction?.Invoke((true, "Login success!"));

            ApiPanel.User_Register(registerModel, obj =>
            {
                App.Models.SetUser(obj.User);
                onCallbackAction?.Invoke((true, "Login success!"));
            }, msg =>
                onCallbackAction?.Invoke((false, msg)));
        }

        public void CheckLoginStatus(Action<bool> onLoginAction)
        {
            #region TestMode
            if(TestMode)
            {
                onLoginAction(true);
                return;
            }
            #endregion
            var hasUserIdentity = App.Models.User != null;
            onLoginAction?.Invoke(hasUserIdentity);
        }

    }
}
