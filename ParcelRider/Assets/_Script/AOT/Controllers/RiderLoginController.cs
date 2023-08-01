using System;
using AOT.Core;
using AOT.Test;
using AOT.Utl;
using OrderHelperLib.DtoModels.Users;

namespace AOT.Controllers
{
    public class RiderLoginController : ControllerBase
    {
        public void Rider_RequestLogin(string username, string password, Action<(bool isSuccess, string message)> callback)
        {
            #region TestMode
            if(TestMode)
            {
                App.Models.SetRider(new UserDto
                {
                    Id = "1",
                    Name = "Test",
                    Phone = "0123456789"
                });
                callback.Invoke((true, string.Empty));
                return;
            }
            #endregion
            ApiPanel.Rider_Login(username, password, result =>
            {
                App.Models.SetRider(result.User);
                callback?.Invoke((true, string.Empty));
            }, msg =>
                callback?.Invoke((false, msg)));
        }
    }
}