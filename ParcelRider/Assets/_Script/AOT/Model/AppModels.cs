using System.Collections;
using System.Collections.Generic;
using AOT.Core;
using AOT.DataModel;
using OrderHelperLib.Dtos.DeliveryOrders;
using OrderHelperLib.Dtos.Users;
using UnityEngine;
using UnityEngine.Networking;

namespace AOT.Model
{
    public class AppModels
    {
        public User User { get; private set; }
        public Rider Rider { get; private set; }
        public DoDataModel ActiveOrders { get; private set; } = new ActiveDoModel();
        public DoDataModel History { get; private set; } = new HistoryDoModel();

        public void SetOrderList(List<DeliveryOrder> orders)
        {
            ActiveOrders.SetOrders(orders);
        }

        public void SetRider(UserModel u)
        {
            Rider = new Rider { Id = 0.ToString(), Name = u.Name, Phone = u.Phone };
            App.SendEvent(EventString.Rider_Update);
        }

        public void SetUser(UserModel user)
        {
            User = new User(user);
            App.SendEvent(EventString.User_Update);
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