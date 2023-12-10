using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.DataModel;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using OrderHelperLib.Dtos.Lingaus;
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
        public DoPageModel AssignedOrders { get; private set; } = new ActiveDoModel();
        public DoPageModel UnassignedOrders { get; private set; } = new UnassignedDoModel();
        public DoPageModel History { get; private set; } = new HistoryDoModel();

        public DeliveryOrder? CurrentOrder { get; private set; }
        public DoSubState[] CurrentStateOptions { get; private set; } = Array.Empty<DoSubState>();

        public IEnumerable<DeliveryOrder> AllOrders => AssignedOrders.Orders.Concat(UnassignedOrders.Orders).Concat(History.Orders);
        public DeliveryOrder? GetOrder(long orderId) => AllOrders.FirstOrDefault(o => o.Id == orderId);

        public void SetCurrentOrder(long orderId)=> SetCurrentOrder(GetOrder(orderId));

        private void SetCurrentOrder(DeliveryOrder order)
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

        public void SetStateOptions(DoSubState[] subStates)
        {
            CurrentStateOptions = subStates;
            App.SendEvent(EventString.Order_Current_OptionsUpdate);
        }

        /// <summary>
        /// 自动更新当前的order并自动插入到相应的list
        /// </summary>
        /// <param name="order"></param>
        public void Resolve_Order(DeliveryOrder order, bool isRider)
        {
            var status = (DeliveryOrderStatus)order.Status;
            AssignedOrders.RemoveOrder(order.Id);
            UnassignedOrders.RemoveOrder(order.Id);
            History.RemoveOrder(order.Id);
            if (status.IsClosed()) History.AddOrder(order);
            else if (isRider && status.IsAssigned()) AssignedOrders.AddOrder(order);//rider
            else if(!isRider) AssignedOrders.AddOrder(order);//user
            else UnassignedOrders.AddOrder(order);
        }

        public void SetUserLingau(LingauModel lingau)
        {
            User.Lingau = lingau;
            App.SendEvent(EventString.User_Update);
        }
    }
}