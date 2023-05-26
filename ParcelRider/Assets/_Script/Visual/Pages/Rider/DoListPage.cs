using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Core;
using DataModel;
using UnityEngine.UI;
using Views;

namespace Visual.Pages.Rider
{
    internal class DoListPage : PageUiBase
    {
        private View_doList view_doList { get; }
        private OrderController OrderController => App.GetController<OrderController>();

        public DoListPage(IView v, Action<string> onOrderSelectedAction,RiderUiManager uiManager, bool display = false) : base(v, uiManager, display)
        {
            view_doList = new View_doList(v.GetObject<View>("view_doList"),onOrderSelectedAction);
            RegEvents();
        }

        private void RegEvents()
        {
            App.MessagingManager.RegEvent(EventString.Orders_Update, _ => UpdateOrderList());
        }

        private void UpdateOrderList()
        {
            view_doList.Set(OrderController.Orders.OrderByDescending(o => int.Parse(o.Id)).ToArray());
        }

        public override void Show()
        {
            view_doList.Set(OrderController.Orders);
            base.Show();
        }

        #region 申请rider页面
        private class View_application : UiBase
        {
            private View_successWindow view_successWindow { get; }
            private Button btn_regRider { get; }

            public View_application(IView v, Action onRegAction,Action onConfirmAction, bool display = true) : base(v, display)
            {
                view_successWindow = new View_successWindow(v.GetObject<View>("view_successWindow"), ()=>
                {
                    Hide();
                    onConfirmAction();
                });
                btn_regRider = v.GetObject<Button>("btn_regRider");
                btn_regRider.OnClickAdd(onRegAction);
            }

            public void ShowSuccess() => view_successWindow.Show();

            private class View_successWindow : UiBase
            {
                private Button btn_confirm { get; }

                public View_successWindow(IView v,Action onConfirmAction) : base(v, false)
                {
                    btn_confirm = v.GetObject<Button>("btn_confirm");
                    btn_confirm.OnClickAdd(onConfirmAction);
                }
            }

            public void ShowApplication()
            {
                view_successWindow.Hide();
                Show();
            }
        }
        #endregion

        #region Do_list 列表页面

        private class View_doList : UiBase
        {
            private ListViewUi<Prefab_do> DoListView { get; }
            private Action<string> OnOrderSelected { get; }

            public View_doList(IView v, Action<string> onOrderSelected, bool display = true) : base(v, display)
            {
                OnOrderSelected = onOrderSelected;
                DoListView = new ListViewUi<Prefab_do>(v, "prefab_do", "scroll_do");
            }

            public void Set(IReadOnlyCollection<DeliveryOrder> orders)
            {
                DoListView.ClearList(ui => ui.Destroy());
                foreach (var order in orders.Where(o => o.Status == (int)DeliveryOrder.States.None))
                {
                    var ui = DoListView.Instance(v => new Prefab_do(v, () => OnOrderSelected(order.Id)));
                    var f = order.From;
                    var t = order.To;
                    var p = order.Package;
                    ui.SetId(order.Id);
                    ui.SetFrom(f.Name, f.Phone, f.Address);
                    ui.SetTo(t.Name, t.Phone, t.Address);
                    ui.SetParcelInfo(p.Price, p.Size, p.Weight, p.Distance);
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
                    text_id = v.GetObject<Text>("text_id");
                    element_infoFrom = new Element_info(v.GetObject<View>("element_infoFrom"));
                    element_infoTo = new Element_info(v.GetObject<View>("element_infoTo"));
                    view_parcelInfo = new View_parcelInfo(v.GetObject<View>("view_parcelInfo"));
                    btn_select = v.GetObject<Button>("btn_select");
                    btn_select.OnClickAdd(onSelectAction);
                }

                public void SetId(string id) => text_id.text = id;

                public void SetFrom(string name, string phone, string address) =>
                    element_infoFrom.Set(name, phone, address);

                public void SetTo(string name, string phone, string address) =>
                    element_infoTo.Set(name, phone, address);

                public void SetParcelInfo(float point, float size, float weight, float distance) =>
                    view_parcelInfo.Set(point, size, weight, distance);

                private class Element_info : UiBase
                {
                    private Text text_name { get; }
                    private Text text_phone { get; }
                    private Text text_address { get; }

                    public Element_info(IView v, bool display = true) : base(v, display)
                    {
                        text_name = v.GetObject<Text>("text_name");
                        text_phone = v.GetObject<Text>("text_phone");
                        text_address = v.GetObject<Text>("text_address");
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
                        text_point = v.GetObject<Text>("text_point");
                        text_size = v.GetObject<Text>("text_size");
                        text_weight = v.GetObject<Text>("text_weight");
                        text_distance = v.GetObject<Text>("text_distance");
                    }

                    public void Set(float point, float size, float weight, float distance)
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
}
