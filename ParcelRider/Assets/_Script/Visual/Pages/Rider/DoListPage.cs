using System;
using System.Collections.Generic;
using System.Linq;
using AOT.BaseUis;
using AOT.Controllers;
using AOT.Core;
using AOT.DataModel;
using AOT.Extensions;
using AOT.Views;
using UnityEngine.UI;

namespace Visual.Pages.Rider
{
    internal abstract class DoListPage : PageUiBase
    {
        protected View_doList view_doList { get; }
        protected UserOrderController UserOrderController => App.GetController<UserOrderController>();

        protected DoListPage(IView v, Action<long> onOrderSelectedAction, Rider_UiManager uiManager, bool display = false)
            : base(v, uiManager, display)
        {
            view_doList = new View_doList(v.Get<View>("view_doList"), onOrderSelectedAction);
            RegEvents();
        }

        private void RegEvents()
        {
            App.MessagingManager.RegEvent(EventString.Orders_Update, _ => UpdateOrderList());
        }

        private void UpdateOrderList()
        {
            var orders = App.Models.ActiveOrders.Orders.Where(OrderListFilter).OrderByDescending(o => o.Id)
                .ToArray();
            OnOrderListUpdate(orders);
            view_doList.Set(orders);
        }

        protected abstract void OnOrderListUpdate(DeliveryOrder[] deliveryOrders);

        protected abstract bool OrderListFilter(DeliveryOrder order);

        protected override void OnUiShow() => UpdateOrderList();
    }

    #region Do_list 列表页面

    internal class View_doList : UiBase
    {
        private ListViewUi<Prefab_do> DoListView { get; }
        private Action<long> OnOrderSelected { get; }
        public int Count => DoListView.List.Count;

        public View_doList(IView v, Action<long> onOrderSelected, bool display = true) : base(v, display)
        {
            OnOrderSelected = onOrderSelected;
            DoListView = new ListViewUi<Prefab_do>(v, "prefab_do", "scroll_do");
        }

        public void Set(IReadOnlyCollection<DeliveryOrder> orders)
        {
            DoListView.ClearList(ui => ui.Destroy());
            foreach (var order in orders)
            {
                var ui = DoListView.Instance(v => new Prefab_do(v, () => OnOrderSelected(order.Id)));
                var sender = order.SenderInfo.User;
                var receiver = order.ReceiverInfo;
                var payment = order.PaymentInfo;
                var deliver = order.DeliveryInfo;
                var item = order.ItemInfo;
                ui.SetId(order.Id);
                ui.SetFrom(sender.Name, sender.Phone, deliver.StartLocation.Address);
                ui.SetTo(receiver.Name, receiver.PhoneNumber, deliver.EndLocation.Address);
                ui.SetParcelInfo(payment.Charge, item.Size(), item.Weight, deliver.Distance);
            }

            Show();
        }

        private class Prefab_do : UiBase
        {
            private Text text_id { get; }
            private Element_info element_infoFrom { get; }
            private Element_info element_infoTo { get; }
            private View_parcelInfo view_parcelInfo { get; }
            private Button btn_select { get; }

            public Prefab_do(IView v, Action onSelectAction, bool display = true) : base(v, display)
            {
                text_id = v.Get<Text>("text_id");
                element_infoFrom = new Element_info(v.Get<View>("element_infoFrom"));
                element_infoTo = new Element_info(v.Get<View>("element_infoTo"));
                view_parcelInfo = new View_parcelInfo(v.Get<View>("view_parcelInfo"));
                btn_select = v.Get<Button>("btn_select");
                btn_select.OnClickAdd(onSelectAction);
            }

            public void SetId(long id) => text_id.text = id.ToString();

            public void SetFrom(string name, string phone, string address) =>
                element_infoFrom.Set(name, phone, address);

            public void SetTo(string name, string phone, string address) =>
                element_infoTo.Set(name, phone, address);

            public void SetParcelInfo(float point, double size, float weight, float distance) =>
                view_parcelInfo.Set(point, size, weight, distance);

            private class Element_info : UiBase
            {
                private Text text_name { get; }
                private Text text_phone { get; }
                private Text text_address { get; }

                public Element_info(IView v, bool display = true) : base(v, display)
                {
                    text_name = v.Get<Text>("text_name");
                    text_phone = v.Get<Text>("text_phone");
                    text_address = v.Get<Text>("text_address");
                }

                public void Set(string name, string phone, string address)
                {
                    text_name.text = name;
                    text_phone.text = phone;
                    text_address.text = address;
                }
            }

            private class View_parcelInfo : UiBase
            {
                private Text text_point { get; }
                private Text text_size { get; }
                private Text text_weight { get; }
                private Text text_distance { get; }

                public View_parcelInfo(IView v, bool display = true) : base(v, display)
                {
                    text_point = v.Get<Text>("text_point");
                    text_size = v.Get<Text>("text_size");
                    text_weight = v.Get<Text>("text_weight");
                    text_distance = v.Get<Text>("text_distance");
                }

                public void Set(float point, double size, float weight, float distance)
                {
                    text_point.text = point.ToString("F");
                    text_size.text = size.ToString("F");
                    text_weight.text = weight.ToString("F");
                    text_distance.text = distance.ToString("F");
                }
            }
        }
    }

    #endregion
}
