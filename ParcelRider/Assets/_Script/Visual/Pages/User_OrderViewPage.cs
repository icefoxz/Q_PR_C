using System;
using AOT.BaseUis;
using AOT.Core;
using AOT.DataModel;
using AOT.Extensions;
using AOT.Views;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using UnityEngine.UI;

namespace Visual.Pages
{
    public class User_OrderViewPage : PageUiBase
    {
        private Text text_orderId { get; }
        private Text text_riderName { get; }
        private Text text_riderPhone { get; }
        private Button btn_close { get; }
        private Button btn_cancel { get; }
        private Element_State elemnent_waitState { get; }
        private Element_State elemnent_DeliverState { get; }
        private Element_State elemnent_dropState { get; }
        private Element_State elemnent_completedState { get; }
        private Element_State elemnent_errState { get; }
        private Element_contact element_contactTo { get; }
        private Element_contact element_contactFrom { get; }
        private View_packageInfo view_packageInfo { get; }

        public User_OrderViewPage(IView v, UiManagerBase uiManager) : base(v, uiManager)
        {
            text_orderId = v.GetObject<Text>("text_orderId");
            btn_close = v.GetObject<Button>("btn_close");
            btn_cancel = v.GetObject<Button>("btn_cancel");
            text_riderName = v.GetObject<Text>("text_riderName");
            text_riderPhone = v.GetObject<Text>("text_riderPhone");

            elemnent_waitState = new Element_State(v.GetObject<View>("element_waitState"));
            elemnent_DeliverState = new Element_State(v.GetObject<View>("element_deliverState"));
            elemnent_dropState = new Element_State(v.GetObject<View>("element_dropState"));
            elemnent_completedState = new Element_State(v.GetObject<View>("element_completedState"));
            elemnent_errState = new Element_State(v.GetObject<View>("element_errState"));

            element_contactTo = new Element_contact(v.GetObject<View>("element_contactTo"));
            element_contactFrom = new Element_contact(v.GetObject<View>("element_contactFrom"));

            view_packageInfo = new View_packageInfo(v.GetObject<View>("view_packageInfo"));

            btn_close.OnClickAdd(Hide);
            App.MessagingManager.RegEvent(EventString.CurrentOrder_Update, _ => UpdateOrder());
        }

        public void DisplayCurrentOrder(Action onCancelRequestAction)
        {
            btn_cancel.gameObject.SetActive(onCancelRequestAction != null);
            if (onCancelRequestAction == null)
                btn_cancel.onClick.RemoveAllListeners();
            else
                btn_cancel.OnClickAdd(onCancelRequestAction);
            UpdateOrder();
            Show();
        }

        private void UpdateOrder()
        {
            var o = App.Models.OrderCollection.Current;
            if (o == null) return;
            text_orderId.text = o.Id.ToString();
            view_packageInfo.Set(o.PaymentInfo.Charge, o.DeliveryInfo.Distance, o.ItemInfo.Weight, o.ItemInfo.Size());
            element_contactTo.Set(o.ReceiverInfo.Name, o.ReceiverInfo.PhoneNumber, o.DeliveryInfo.EndLocation.Address);
            element_contactFrom.Set(o.SenderInfo.User.Name, o.SenderInfo.User.Phone, o.DeliveryInfo.StartLocation.Address);
            text_riderName.text = o.Rider?.Name;
            text_riderPhone.text = o.Rider?.Phone;
            SetState(o.State);
        }

        private void SetState(DeliveryOrderStatus state)
        {
            btn_cancel.gameObject.SetActive(state == DeliveryOrderStatus.Created);
            elemnent_waitState.SetActive(state == DeliveryOrderStatus.Created);
            elemnent_DeliverState.SetActive(state == DeliveryOrderStatus.Assigned);
            elemnent_dropState.SetActive(state == DeliveryOrderStatus.Delivering);
            elemnent_completedState.SetActive(state == DeliveryOrderStatus.Completed);

            elemnent_completedState.SetViewActive(state != DeliveryOrderStatus.Exception);
            elemnent_errState.SetViewActive(state == DeliveryOrderStatus.Exception);
        }

        private class Element_State : UiBase
        {
            private Image Img_active { get; }
            public Element_State(IView v) : base(v)
            {
                Img_active = v.GetObject<Image>("img_active");
            }

            public void SetActive(bool active)
            {
                Img_active.gameObject.SetActive(active);
            }

            public void SetViewActive(bool active) => GameObject.SetActive(active);
        }

        private class Element_contact : UiBase
        {
            private Text text_name { get; }
            private Text text_phone { get; }
            private Text text_address { get; }

            public Element_contact(IView v) : base(v)
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

    }
}