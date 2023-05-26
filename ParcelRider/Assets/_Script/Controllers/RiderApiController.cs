using System;
using Core;
using OrderHelperLib.DtoModels.DeliveryOrders;
using Utl;

namespace Controllers
{
    public class RiderApiController : IController
    {
        public void RequestLogin(string username, string password, Action<(bool isSuccess, string message)> callback)
        {
            App.Models.SetRider(new RiderDto
            {
                Name = "Test",
                Phone = "0123456789"
            });
            callback.Invoke((true, string.Empty));
            return;
            ApiPanel.RiderLogin(username, password, dto =>
            {
                App.Models.SetRider(dto);
                callback?.Invoke((true, string.Empty));
            }, msg =>
                callback?.Invoke((false, msg)));
        }
    }
}