using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.DataModel;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using OrderHelperLib.Dtos.Users;
using UnityEngine;
using UnityEngine.Networking;

namespace AOT.Model
{
    public class AppModels
    {
        public DoSubState[] SubStates { get; private set; }
        public User User { get; private set; }
        public Rider Rider { get; private set; }
        /// <summary>
        /// Rider = Assigned orders, User = Created orders
        /// </summary>
        public DoDataModel AssignedOrders { get; private set; } = new ActiveDoModel();
        public DoDataModel UnassignedOrders { get; private set; } = new UnassignedDoModel();
        public DoDataModel History { get; private set; } = new HistoryDoModel();

        public DeliveryOrder? CurrentOrder { get; private set; }
        public IEnumerable<DeliveryOrder> AllOrders => AssignedOrders.Orders.Concat(UnassignedOrders.Orders).Concat(History.Orders);
        public DeliveryOrder? GetOrder(long orderId) => AllOrders.FirstOrDefault(o => o.Id == orderId);

        public void SetCurrentOrder(long orderId)=> SetCurrentOrder(GetOrder(orderId));
        public void SetCurrentOrder(DeliveryOrder order)
        {
            CurrentOrder = order;
            App.SendEvent(EventString.Order_Current_Set);
        }

        public void SetSubStates(DoSubState[] subStates) => SubStates = subStates;

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

        public void Reset()
        {
            SubStates = Array.Empty<DoSubState>();
            User = null;
            Rider = null;
            AssignedOrders.Reset();
            UnassignedOrders.Reset();
            History.Reset();
        }
    }
}