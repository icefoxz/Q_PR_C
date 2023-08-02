using System.Collections;
using System.Collections.Generic;
using AOT.Core;
using AOT.DataModel;
using OrderHelperLib.DtoModels.Users;
using UnityEngine;
using UnityEngine.Networking;

namespace AOT.Model
{
    public class AppModels : ModelBase
    {
        public UserModel User { get; private set; }
        public RiderModel Rider { get; private set; }
        public DoDataModel OrderCollection { get; private set; } = new DoDataModel();

        public void SetOrderList(List<DeliveryOrder> orders)
        {
            OrderCollection.ClearOrders();
            OrderCollection.SetOrders(orders);
        }

        public void SetCurrentOrder(DeliveryOrder order) => OrderCollection.SetCurrent(order.Id);

        public void SetRider(UserDto u)
        {
            Rider = new RiderModel
            {
                Id = u.Id,
                Name = u.Name,
                Phone = u.Phone
            };
            SendEvent(EventString.Rider_Update);
        }

        public void SetUser(UserDto user)
        {
            User = new UserModel(user);
            SendEvent(EventString.User_Update);
            if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
                App.MonoService.StartCoroutine(LoadImage(user.AvatarUrl));
        }

        private IEnumerator LoadImage(string imageUrl)
        {
            if (imageUrl == null) yield break;

            var www = UnityWebRequestTexture.GetTexture(imageUrl);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                Debug.Log(www.error);
            else
            {
                var texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                var avatar = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                User.SetAvatar(avatar);
            }
        }
    }
}