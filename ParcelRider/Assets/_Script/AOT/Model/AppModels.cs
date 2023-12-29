using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.DataModel;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.Lingaus;
using OrderHelperLib.Dtos.Users;
using UnityEngine;
using UnityEngine.Networking;

namespace AOT.Model
{
    public class AppModels
    {

        public enum PageOrders
        {
            Assigned,
            Unassigned,
            History
        }
        public DoSubState[] SubStates { get; private set; }
        public User User { get; private set; }
        public Rider Rider { get; private set; }

        /// <summary>
        /// Rider = Assigned orders, User = Created orders
        /// </summary>
        private DoPageModel _assigned = new ActiveDoModel();
        private DoPageModel _unassigned = new UnassignedDoModel();
        private DoPageModel _history = new HistoryDoModel();
        public IReadOnlyList<DeliveryOrder> Assigned => _assigned.Orders;
        public IReadOnlyList<DeliveryOrder> Unassigned => _unassigned.Orders;
        public IReadOnlyList<DeliveryOrder> History => _history.Orders;

        public DeliveryOrder? CurrentOrder { get; private set; }

        public IEnumerable<DeliveryOrder> AllOrders => Assigned.Concat(Unassigned).Concat(History);
        public DeliveryOrder? GetOrder(long orderId) => AllOrders.FirstOrDefault(o => o.Id == orderId);
        public DeliveryOrder? TryGetOrder(PageOrders page,long orderId) => GetDoPageModel(page).GetOrder(orderId);
        public DoPageModel GetDoPageModel(PageOrders page)
        {
            var doPage = page switch
            {
                PageOrders.Assigned => _assigned,
                PageOrders.Unassigned => _unassigned,
                PageOrders.History => _history,
                _ => throw new ArgumentOutOfRangeException(nameof(page), page, null)
            };
            return doPage;
        }

        public void SetCurrentOrder(long orderId)=> SetCurrentOrder(GetOrder(orderId));

        private void SetCurrentOrder(DeliveryOrder order)
        {
            CurrentOrder = order;
            App.SendEvent(EventString.Order_Current_Set);
        }

        public void SetPageOrders(PageOrders page, ICollection<DeliveryOrder> list, int pageIndex = -1)
        {
            var current = list.FirstOrDefault(o => o.Id == CurrentOrder?.Id);
            var doPage = GetDoPageModel(page);
            doPage.SetOrders(list, ResolvePageIndex(doPage.PageIndex));
            if (current != null) SetCurrentOrder(current);
            return;

            int ResolvePageIndex(int assignedIndex)
            {
                return pageIndex <= -1 ? assignedIndex : pageIndex;
            }
        }


        public void SetSubStates(DoSubState[] subStates) => SubStates = subStates;

        private void SetRider(UserModel u)
        {
            Rider = new Rider { Id = 0.ToString(), Name = u.Name, Phone = u.Phone };
            App.SendEvent(EventString.Rider_Update);
        }

        private void SetUser(UserModel user)
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
            _assigned.Reset();
            _unassigned.Reset();
            _history.Reset();
        }

        /// <summary>
        /// 自动更新当前的order并自动插入到相应的list
        /// </summary>
        /// <param name="order"></param>
        public void Resolve_Order(DeliveryOrder order, bool isRider)
        {
            var status = (DeliveryOrderStatus)order.Status;
            _assigned.RemoveOrder(order.Id);
            _unassigned.RemoveOrder(order.Id);
            _history.RemoveOrder(order.Id);
            if (status.IsClosed()) _history.AddOrder(order);
            else if (isRider && status == DeliveryOrderStatus.Assigned) _assigned.AddOrder(order);//rider
            else if(!isRider) _assigned.AddOrder(order);//user
            else _unassigned.AddOrder(order);
        }

        public void SetUserLingau(LingauModel lingau)
        {
            User.Lingau = lingau;
            App.SendEvent(EventString.User_Update);
        }

        public void UserLogin(UserModel user)
        {
            SetUser(user);
            App.SendEvent(EventString.User_Login);
        }

        public void RiderLogin(UserModel user)
        {
            SetRider(user);
            App.SendEvent(EventString.Rider_Login);
        }

        public void UserLogout()
        {
            Reset();
            App.SendEvent(EventString.User_Logout);
        }

        public void RiderLogout()
        {
            Reset();
            App.SendEvent(EventString.Rider_Logout);
        }
    }
}