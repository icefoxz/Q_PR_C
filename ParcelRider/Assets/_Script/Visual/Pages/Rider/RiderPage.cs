using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Core;
using DataModel;
using UnityEngine;
using UnityEngine.UI;
using Utl;
using Views;

namespace Visual.Pages.Rider
{
    internal class RiderPage : PageUiBase
    {
        private View_application view_application { get; }
        private View_doList view_doList { get; }
        private View_deliveryOrder view_deliveryOrder { get; }
        private RiderController RiderController => App.GetController<RiderController>();
        private PackageController PackageController => App.GetController<PackageController>();

        public RiderPage(IView v, UiManager uiManager, bool display = false) : base(v, uiManager, display)
        {
            view_application = new View_application(v.GetObject<View>("view_application"), OnRegisterAction,
                UpdateOrderList);
            view_doList = new View_doList(v.GetObject<View>("view_doList"),
                id => view_deliveryOrder.Set(PackageController.GetOrder(id)));
            view_deliveryOrder =
                new View_deliveryOrder(v.GetObject<View>("view_deliveryOrder"),
                    TackOrder,
                    PickItem,
                    Collection,
                    Complete,
                    OrderException,
                    ExceptionOptionSelected);
            RegEvents();
        }

        private void RegEvents()
        {
            App.MessagingManager.RegEvent(EventString.Orders_Update, _ => UpdateOrderList());
        }

        private void UpdateOrderList()
        {
            view_doList.Set(PackageController.Orders.OrderByDescending(o => int.Parse(o.Id)).ToArray());
        }

        private void ExceptionOptionSelected(string orderId, int optionIndex)
        {
            RiderController.SetException(orderId, optionIndex, () =>
            {
                var o = PackageController.GetOrder(orderId);
                view_deliveryOrder.Set(o);
            });
        }

        private void OrderException(string orderId)
        {
            RiderController.OrderException(orderId, options => view_deliveryOrder.SetExceptionOption(options));
        }

        private void Complete(string orderId)
        {
            RiderController.Complete(orderId, () =>
            {
                var o = PackageController.GetOrder(orderId);
                view_deliveryOrder.Set(o);
            });
        }

        private void Collection(string orderId)
        {
            RiderController.ItemCollection(orderId, () =>
            {
                var o = PackageController.GetOrder(orderId);
                view_deliveryOrder.Set(o);
            });
        }

        private void PickItem(string orderId)
        {
            RiderController.PickItem(orderId, () =>
            {
                var o = PackageController.GetOrder(orderId);
                view_deliveryOrder.Set(o);
            });
        }

        private void TackOrder(string orderId)
        {
            RiderController.TakeOrder(orderId, () =>
            {
                PackageController.AssignRider(orderId, success =>
                {
                    if(success)
                    {
                        var o = PackageController.GetOrder(orderId);
                        view_deliveryOrder.Set(o);
                        return;
                    }

                    Debug.LogError("AssignRider failed");
                });
            });
        }

        private void OnRegisterAction()
        {
            RiderController.RiderApplication(isSuccess =>
            {
                if (!isSuccess) return;
                Auth.IsRider = true;
                Auth.IsRiderMode = true;
                view_application.ShowSuccess();
            });
        }

        public void RegisterRider()
        {
            view_doList.Hide();
            view_deliveryOrder.Hide();
            view_application.ShowApplication();
            Show();
        }

        public override void Show()
        {
            if(Auth.IsRider) view_doList.Set(PackageController.Orders);
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
                foreach (var order in orders.Where(o =>
                             o.Rider?.Id == Auth.RiderId || o.Status == (int)DeliveryOrder.States.None))
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

        #region DeliveryOrder 运送页面
        private class View_deliveryOrder : UiBase
        {
            private View_states view_states { get; }
            private View_parcelInfo view_parcelInfo { get; }
            private Element_info element_infoFrom { get; }
            private Element_info element_infoTo { get; }
            private View_riderOptions view_riderOptions { get; }
            private View_exception view_exception { get; }
            private Button btn_exception { get; }
            private Button btn_close { get; }
            private string orderId { get; set; }

            public View_deliveryOrder(IView v,
                Action<string> onTakeOrderAction,
                Action<string> onPickItemAction,
                Action<string> onCollectionAction,
                Action<string> onCompletedAction,
                Action<string> onExceptionAction,
                Action<string,int> onExceptionOpSelectedAction,
                bool display = true) : base(v, display)
            {
                view_states = new View_states(v.GetObject<View>("view_states"));
                view_parcelInfo = new View_parcelInfo(v.GetObject<View>("view_parcelInfo"));
                element_infoFrom = new Element_info(v.GetObject<View>("element_infoFrom"));
                element_infoTo = new Element_info(v.GetObject<View>("element_infoTo"));
                view_riderOptions = new View_riderOptions(v.GetObject<View>("view_riderOptions")
                    , () => onTakeOrderAction(orderId)
                    , () => onPickItemAction(orderId)
                    , () => onCollectionAction(orderId)
                    , () => onCompletedAction(orderId));
                view_exception = new View_exception(v.GetObject<View>("view_exception"),
                    index => onExceptionOpSelectedAction(orderId, index));
                btn_exception = v.GetObject<Button>("btn_exception");
                btn_exception.OnClickAdd(() => onExceptionAction(orderId));
                btn_close = v.GetObject<Button>("btn_close");
                btn_close.OnClickAdd(() => Hide());
            }

            public void Set(DeliveryOrder order)
            {
                orderId = order.Id;
                var p = order.Package;
                var f = order.From;
                var t = order.To;
                view_parcelInfo.Set(p.Price, p.Size, p.Weight, p.Distance);
                element_infoFrom.Set(f.Name, f.Phone, f.Address);
                element_infoTo.Set(t.Name, t.Phone, t.Address);
                UpdateState((DeliveryOrder.States)order.Status);
                Show();
                void UpdateState(DeliveryOrder.States state)
                {
                    btn_exception.gameObject.SetActive(state is DeliveryOrder.States.Wait 
                        or DeliveryOrder.States.Delivering 
                        or DeliveryOrder.States.Collection);
                    view_states.SetState(state);
                    view_riderOptions.SetState(state);
                }
            }
            public void SetExceptionOption(string[] options) => view_exception.SetOptions(options);

            private class View_states : UiBase
            {

                private Element_state element_stateWait { get; }
                private Element_state element_stateDelivering { get; }
                private Element_state element_stateCollection { get; }
                private Element_state element_stateComplete { get; }
                private Element_state element_stateException { get; }

                public View_states(IView v, bool display = true) : base(v, display)
                {
                    element_stateWait = new Element_state(v.GetObject<View>("element_stateWait"));
                    element_stateDelivering = new Element_state(v.GetObject<View>("element_stateDelivering"));
                    element_stateCollection = new Element_state(v.GetObject<View>("element_stateCollection"));
                    element_stateComplete = new Element_state(v.GetObject<View>("element_stateComplete"));
                    element_stateException = new Element_state(v.GetObject<View>("element_stateException"));
                }

                public void SetState(DeliveryOrder.States state)
                {
                    element_stateWait.SetActive(state == DeliveryOrder.States.Wait);
                    element_stateDelivering.SetActive(state == DeliveryOrder.States.Delivering);
                    element_stateCollection.SetActive(state == DeliveryOrder.States.Collection);
                    element_stateComplete.SetActive(state == DeliveryOrder.States.Complete);
                    element_stateException.SetActive(state == DeliveryOrder.States.Exception);
                }

                private class Element_state : UiBase
                {
                    private Image img_state { get; }
                    private Image img_active { get; }

                    public Element_state(IView v, bool display = true) : base(v, display)
                    {
                        img_state = v.GetObject<Image>("img_state");
                        img_active = v.GetObject<Image>("img_active");
                    }

                    public void SetActive(bool active) => img_active.gameObject.SetActive(active);
                    public void SetIcon(Sprite icon) => img_state.sprite = icon;
                }
            }
            private class View_parcelInfo : UiBase
            {
                private Text text_point { get; }
                private Text text_meter { get; }
                private Text text_kg{ get; }
                private Text text_km { get; }

                public View_parcelInfo(IView v, bool display = true) : base(v, display)
                {
                    text_point = v.GetObject<Text>("text_point");
                    text_meter = v.GetObject<Text>("text_meter");
                    text_kg = v.GetObject<Text>("text_kg");
                    text_km = v.GetObject<Text>("text_km");
                }

                public void Set(float point, float meter, float kg, float km)
                {
                    text_point.text = point.ToString("F");
                    text_meter.text = meter.ToString("F");
                    text_kg.text = kg.ToString("F");
                    text_km.text = km.ToString("F");
                }
            }
            private class Element_info : UiBase
            {
                private Text text_contactName { get; }
                private Text text_contactPhone { get; }
                private Text text_address { get; }

                public Element_info(IView v, bool display = true) : base(v, display)
                {
                    text_contactName = v.GetObject<Text>("text_contactName");
                    text_contactPhone = v.GetObject<Text>("text_contactPhone");
                    text_address = v.GetObject<Text>("text_address");
                }

                public void Set(string name, string phone, string address)
                {
                    text_contactName.text = name;
                    text_contactPhone.text = phone;
                    text_address.text = address;
                }
            }
            private class View_riderOptions :UiBase
            {
                private Button btn_takeOrder { get; }
                private Button btn_pickItem { get; }
                private Button btn_collection { get; }
                private Button btn_complete { get; }

                public View_riderOptions(IView v, 
                    Action onTakeOrderAction, 
                    Action onPickItemAction,
                    Action onCollectionAction,
                    Action onCompleteAction, bool display = true) : base(v, display)
                {
                    btn_takeOrder = v.GetObject<Button>("btn_takeOrder");
                    btn_pickItem = v.GetObject<Button>("btn_pickItem");
                    btn_collection = v.GetObject<Button>("btn_collection");
                    btn_complete = v.GetObject<Button>("btn_complete");
                    btn_takeOrder.OnClickAdd(onTakeOrderAction);
                    btn_pickItem.OnClickAdd(onPickItemAction);
                    btn_collection.OnClickAdd(onCollectionAction);
                    btn_complete.OnClickAdd(onCompleteAction);
                }

                public void SetState(DeliveryOrder.States state)
                {
                    btn_takeOrder.gameObject.SetActive(state == DeliveryOrder.States.None);
                    btn_pickItem.gameObject.SetActive(state == DeliveryOrder.States.Wait);
                    btn_collection.gameObject.SetActive(state == DeliveryOrder.States.Delivering);
                    btn_complete.gameObject.SetActive(state == DeliveryOrder.States.Collection);
                }
            }
            private class View_exception : UiBase
            {
                private ListViewUi<Prefab_option> OptionListView { get; set; }
                private Button btn_close { get; }
                private  Action<int> OnOptionSelected { get; set; }

                public View_exception(IView v,Action<int> onOptionSelected, bool display = true) :
                    base(v, display)
                {
                    OnOptionSelected = onOptionSelected;
                    btn_close = v.GetObject<Button>("btn_close");
                    btn_close.OnClickAdd(Hide);
                    OptionListView = new ListViewUi<Prefab_option>(v, "prefab_option", "scroll_options");
                    Hide();
                }

                public void SetOptions(IList<string> options)
                {
                    OptionListView.ClearList(ui=>ui.Destroy());
                    for (var i = 0; i < options.Count; i++)
                    {
                        var option = options[i];
                        var index = i;
                        var prefab = OptionListView.Instance(v => new Prefab_option(v, () =>
                        {
                            OnOptionSelected(index);
                            Hide();
                        }));
                        prefab.Set(option);
                    }
                    Show();
                }

                private class Prefab_option : UiBase
                {
                    private Text text_description { get; }
                    private Button btn_option { get; }
                    public Prefab_option(IView v, Action onCLickAction ,bool display = true) : base(v, display)
                    {
                        text_description = v.GetObject<Text>("text_description");
                        btn_option = v.GetObject<Button>("btn_option");
                        btn_option.OnClickAdd(onCLickAction);
                    }

                    public void Set(string description) => text_description.text = description;
                }
                
            }
        }
        #endregion

    }
}
