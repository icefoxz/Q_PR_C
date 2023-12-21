using System;
using System.Collections.Generic;
using System.Linq;
using AOT.BaseUis;
using AOT.Controllers;
using AOT.Core;
using AOT.DataModel;
using AOT.Extensions;
using AOT.Views;
using OrderHelperLib.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace Visual.Pages.Rider
{
    public class RiderOrderViewPage : PageUiBase
    {
        private Text text_orderId { get; }
        private Text text_state { get; }
        private View_states view_states { get; }
        private View_tab view_tab { get; }
        private View_packageInfo view_packageInfo { get; }
        private View_riderOptions view_riderOptions { get; }
        private View_images ViewImages { get; }
        private Button btn_exception { get; }
        private Button btn_close { get; }
        private long OrderId { get; set; }
        private List<Sprite> images { get; set; } = new List<Sprite>();
        private PictureController PictureController => App.GetController<PictureController>();
        private RiderOrderController RiderOrderController => App.GetController<RiderOrderController>();

        public RiderOrderViewPage(IView v,Action onPossibleExceptionAction ,Rider_UiManager uiManager) : base(v)
        {
            text_orderId = v.Get<Text>("text_orderId");
            text_state = v.Get<Text>("text_state");
            view_packageInfo = new View_packageInfo(v.Get<View>("view_packageInfo"));
            view_states = new View_states(v.Get<View>("view_states"));
            view_tab = new View_tab(v.Get<View>("view_tab"));
            ViewImages = new View_images(v: v.Get<View>("view_images"),
                onImageSelectedAction: ImageSelected_PromptImageWindow,
                onGalleryAction: () => PictureController.OpenGallery(OnPictureTaken),
                onCameraAction: () => PictureController.OpenCamera(OnPictureTaken));
            view_riderOptions = new View_riderOptions(v.Get<View>("view_riderOptions"), OnStateChange);
            btn_exception = v.Get<Button>("btn_exception");
            btn_exception.OnClickAdd(onPossibleExceptionAction);
            btn_close = v.Get<Button>("btn_close");
            btn_close.OnClickAdd(Hide);

            App.MessagingManager.RegEvent(EventString.Order_Current_Set, _ => ShowCurrentOrder());
        }

        private void OnStateChange(int stateId)
        {
            var current = App.Models.CurrentOrder;
            if (current.State == DeliveryOrderStatus.Created)
            {
                ConfirmWindow.Set("Confirm", "Take order?", RiderOrderController.Do_AssignRider);
                return;
            }
            var state = DoStateMap.GetState(stateId);
            ConfirmWindow.Set("Confirm", (state?.StateName ?? "Stage Change") + "?",
                () => RiderOrderController.Do_State_Update(stateId));
        }


        private void OnPictureTaken(Texture2D texture)
        {
            images.Add(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero));
            UpdateImages();
        }
        private void UpdateImages() => ViewImages.Set(images.ToArray());
        private void ImageSelected_PromptImageWindow(int index) => ImageWindow.Set(images[index]);

        private void ShowCurrentOrder()
        {
            var order = App.Models.CurrentOrder;
            //throw new Exception("No order set to current!");
            if (order == null) return;
            OrderId = order.Id;
            var payment = order.PaymentInfo;
            var deliver = order.DeliveryInfo;
            var item = order.ItemInfo;
            text_orderId.text = order.Id.ToString();
            view_tab.Set(order);
            view_packageInfo.Set(payment.Charge, deliver.Distance, item.Weight, item.Size());
            ViewImages.SetActive(order.State.IsInProgress());
            UpdateState();
            Show();

            void UpdateState()
            {
                var state = DoStateMap.GetState(order.SubState);
                if (state == null)
                {
                    text_state.text = "State not found!";
                    btn_exception.interactable = false;
                    view_states.SetState(DeliveryOrderStatus.Closed);
                    view_riderOptions.SetState(Array.Empty<DoSubState>());
                    return;
                }
                text_state.text = state.GetStatus + $"({state.Status})\n" + state.StateName + $"({order.SubState})";
                var status = order.State;
                var possibleStates = DoStateMap.GetPossibleStates(TransitionRoles.Rider, order.SubState);
                btn_exception.interactable = possibleStates.Any(p => p.GetStatus == DeliveryOrderStatus.Exception);
                view_states.SetState(status);
                view_riderOptions.SetState(possibleStates.Where(s => s.GetStatus != DeliveryOrderStatus.Exception)
                    .ToArray());
            }
        }

        private class View_tab : UiBase
        {
            private enum Tabs
            {
                DeliverInfo,
                ProgressInfo
            }
            private Button btn_progress { get; }
            private Button btn_info { get; }
            private View_progressInfo view_progressInfo { get; }
            private View_deliverInfo view_deliverInfo { get; }

            public View_tab(IView v, bool display = true) : base(v, display)
            {
                btn_progress = v.Get<Button>("btn_progress");
                btn_info = v.Get<Button>("btn_info");
                view_progressInfo = new View_progressInfo(v.Get<View>("view_progressInfo"), v.RectTransform.rect.width);
                view_deliverInfo = new View_deliverInfo(v.Get<View>("view_deliverInfo"));
                btn_progress.OnClickAdd(() => SetTab(Tabs.ProgressInfo));
                btn_info.OnClickAdd(() => SetTab(Tabs.DeliverInfo));
            }

            private void SetTab(Tabs mode)
            {
                view_progressInfo.Display(mode == Tabs.ProgressInfo);
                view_deliverInfo.Display(mode == Tabs.DeliverInfo);
            }

            public void Set(DeliveryOrder order)
            {
                view_deliverInfo.Set(order);
                view_progressInfo.Set(order, 110);
                SetTab(Tabs.ProgressInfo);
            }

            private class View_deliverInfo : UiBase
            {
                private Text text_riderName { get; }
                private Text text_riderPhone { get; }
                private Element_contact element_contactTo { get; }
                private Element_contact element_contactFrom { get; }

                public View_deliverInfo(IView v, bool display = true) : base(v, display)
                {
                    text_riderName = v.Get<Text>("text_riderName");
                    text_riderPhone = v.Get<Text>("text_riderPhone");
                    element_contactTo = new Element_contact(v.Get<View>("element_contactTo"));
                    element_contactFrom = new Element_contact(v.Get<View>("element_contactFrom"));
                }

                public void Set(DeliveryOrder order)
                {
                    var sender = order.SenderInfo.User;
                    var receiver = order.ReceiverInfo;
                    var deliver = order.DeliveryInfo;
                    text_riderName.text = order.Rider?.Name;
                    text_riderPhone.text = order.Rider?.Phone;
                    element_contactFrom.Set(sender.Name, sender.Phone, deliver.StartLocation.Address);
                    element_contactTo.Set(receiver.Name, receiver.PhoneNumber, deliver.EndLocation.Address);
                }
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
                element_stateWait = new Element_state(v.Get<View>("element_stateWait"));
                element_stateDelivering = new Element_state(v.Get<View>("element_stateDelivering"));
                element_stateCollection = new Element_state(v.Get<View>("element_stateCollection"));
                element_stateComplete = new Element_state(v.Get<View>("element_stateComplete"));
                element_stateException = new Element_state(v.Get<View>("element_stateException"));
            }

            public void SetState(DeliveryOrderStatus state)
            {
                element_stateWait.SetActive(state == DeliveryOrderStatus.Created);
                element_stateDelivering.SetActive(state == DeliveryOrderStatus.Assigned);
                element_stateCollection.SetActive(state == DeliveryOrderStatus.Delivering);
                element_stateComplete.SetActive(state == DeliveryOrderStatus.Completed);

                element_stateComplete.SetViewActive(state != DeliveryOrderStatus.Exception);
                element_stateException.SetViewActive(state == DeliveryOrderStatus.Exception);
            }

            private class Element_state : UiBase
            {
                private Image Img_active { get; }
                public Element_state(IView v) : base(v)
                {
                    Img_active = v.Get<Image>("img_active");
                }

                public void SetActive(bool active)
                {
                    Img_active.gameObject.SetActive(active);
                }

                public void SetViewActive(bool active) => GameObject.SetActive(active);
            }

        }

        private class View_riderOptions : UiBase
        {
            private ListView_Trans<Prefab_option> OptionList { get; }
            private event Action<int> OnStateSelected;

            public View_riderOptions(IView v,
                Action<int> onStateSelected, bool display = true) : base(v, display)
            {
                OnStateSelected = onStateSelected;
                OptionList = new ListView_Trans<Prefab_option>(v, "prefab_option");
            }

            public void SetState(DoSubState[] subStates)
            {
                OptionList.ClearList(ui=>ui.Destroy());
                foreach (var state in subStates)
                {
                    var ui = OptionList.Instance(v => new Prefab_option(v));
                    ui.Set(state.StateName, () => OnStateSelected?.Invoke(state.StateId));
                }
            }

            private class Prefab_option : UiBase
            {
                private Text text_option { get; }
                private Button btn_option { get; }
                public Prefab_option(IView v, bool display = true) : base(v, display)
                {
                    text_option = v.Get<Text>("text_option");
                    btn_option = v.Get<Button>("btn_option");
                }

                public void Set(string option, Action onclickAction)
                {
                    text_option.text = option;
                    btn_option.OnClickAdd(onclickAction);
                } 
            }
        }

        private class Element_contact : UiBase
        {
            private Text text_name { get; }
            private Text text_phone { get; }
            private Text text_address { get; }

            public Element_contact(IView v) : base(v)
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

        private class View_images : UiBase
        {
            private ListView_Scroll<Prefab_image> ImageListView { get; }
            private Button btn_camera { get; }
            private Button btn_gallery { get; }
            private event Action<int> OnImageSelected;

            public View_images(IView v, 
                Action<int> onImageSelectedAction, 
                Action onGalleryAction,
                Action onCameraAction) : base(v)
            {
                OnImageSelected = onImageSelectedAction;
                btn_camera = v.Get<Button>("btn_camera");
                btn_camera.OnClickAdd(onCameraAction);
                btn_gallery = v.Get<Button>("btn_gallery");
                btn_gallery.OnClickAdd(onGalleryAction);
                ImageListView = new ListView_Scroll<Prefab_image>(v, "prefab_image", "scroll_image");
            }

            public void Set(Sprite[] images)
            {
                ImageListView.ScrollRect.enabled = images.Length > 0;
                ImageListView.ClearList(ui => ui.Destroy());
                for (var i = 0; i < images.Length; i++)
                {
                    var index = i;
                    var sprite = images[i];
                    ImageListView.Instance(v =>
                        new Prefab_image(v, sprite, () => OnImageSelected?.Invoke(index)));
                }
            }

            private class Prefab_image : UiBase
            {
                private Image img_item { get; }
                private Button btn_image { get; }

                public Prefab_image(IView v,Sprite image ,Action onclickAction, bool display = true) : base(v, display)
                {
                    img_item = v.Get<Image>("img_item");
                    btn_image = v.Get<Button>("btn_image");
                    img_item.sprite = image;
                    btn_image.OnClickAdd(onclickAction);
                }
            }

            public void SetActive(bool active)
            {
                btn_camera.interactable = active;
                btn_gallery.interactable = active;
            }
        }
    }
}