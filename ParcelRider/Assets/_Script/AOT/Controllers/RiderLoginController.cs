using System;
using AOT.Core;
using AOT.Test;
using AOT.Utl;
using OrderHelperLib;
using OrderHelperLib.Dtos.Users;

namespace AOT.Controllers
{
    public class RiderLoginController : ControllerBase
    {
        public void Rider_RequestLogin(string username, string password, Action<(bool isSuccess, string message)> callback)
        {
            Call(new object[] { username }, args => ((bool)args[0], (string)args[1]), arg =>
            {
                var (isSuccess, message) = arg;
                if (isSuccess)
                {
                    var bag = DataBag.Deserialize(message);
                    var rider = bag.Get<UserModel>(0);
                    App.Models.RiderLogin(rider);
                    message = string.Empty;
                }
                callback((isSuccess, message));
            }, () =>
            {
                ApiPanel.Rider_Login(username, password, result =>
                {
                    App.Models.RiderLogin(result.User);
                    App.SignalRClient.Connect();
                    callback?.Invoke((true, string.Empty));
                }, msg =>
                    callback?.Invoke((false, msg)));
            });
        }

        public void Logout() => App.Models.RiderLogout();
    }
}