using System;
using DataModel;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace Visual.Pages.Rider
{
    public class OrderPage : PageUiBase
    {
        private View_states view_states { get; }
        private View_parcelInfo view_parcelInfo { get; }
        private Element_info element_infoFrom { get; }
        private Element_info element_infoTo { get; }
        private View_riderOptions view_riderOptions { get; }
        private Button btn_exception { get; }
        private Button btn_close { get; }
        private string orderId { get; set; }

        public OrderPage(IView v,
            Action<string> onTakeOrderAction,
            Action<string> onPickItemAction,
            Action<string> onCollectionAction,
            Action<string> onCompletedAction,
            Action<string> onExceptionAction,
            RiderUiManager uiManager) : base(v,uiManager)
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
    }
}