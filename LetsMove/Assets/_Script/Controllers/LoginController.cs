using System;
using System.Threading.Tasks;
using Core;
using OrderHelperLib.DtoModels.Users;
using Utl;

namespace Controllers
{
    public class LoginController : IController
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
            if (!GoogleSignInManager.IsInit)
                GoogleSignInManager.Init();
            GoogleSignInManager.GoogleSignInClick(user =>
            {
                if (user == null)
                {
                    callback?.Invoke(false);
                    return;
                }

                App.Models.SetUser(new UserDto
                {
                    Username = user.Email,
                    Name = user.DisplayName,
                    AvatarUrl = user.PhotoUrl.ToString(),
                });
                callback?.Invoke(true);
            });
        }

        public void RequestFacebook(Action<bool> callback)
        {
            FacebookSignInManager.OnLoginButtonClicked(arg =>
            {
                var (success, name, email, avatarUrl) = arg;
                if (!success)
                {
                    callback?.Invoke(false);
                    return;
                }

                App.Models.SetUser(new UserDto
                {
                    Username = email,
                    Name = name,
                    AvatarUrl = avatarUrl,
                });
                callback?.Invoke(true);
            });
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
