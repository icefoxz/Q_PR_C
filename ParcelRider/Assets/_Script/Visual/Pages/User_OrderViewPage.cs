namespace Visual.Pages
{
    using System;
    using System.Linq;
    using AOT.BaseUis;
    using AOT.Core;
    using AOT.DataModel;
    using AOT.Extensions;
    using AOT.Views;
    using DG.Tweening;
    using OrderHelperLib.Contracts;
    using OrderHelperLib.Dtos.DeliveryOrders;
    using UnityEngine;
    using UnityEngine.UI;

    namespace Visual.Pages.Rider
    {
        public class User_OrderViewPage : PageUiBase
        {
            private Text text_orderId { get; }
            private Button btn_close { get; }
            private Button btn_cancel { get; }

            private View_deliverItemInfo view_deliverItemInfo { get; }
            private View_tabs view_tabs { get; }

            public User_OrderViewPage(IView v, Action onCancelAction, User_UiManager uiManager) : base(v)
            {
                text_orderId = v.Get<Text>("text_orderId");

                view_deliverItemInfo = new View_deliverItemInfo(v.Get<View>("view_deliverItemInfo"));
                view_tabs = new View_tabs(v.Get<View>("view_tabs"), v.RectTransform);

                //ViewImages = new View_images(v: v.Get<View>("view_images"),
                //    onImageSelectedAction: ImageSelected_PromptImageWindow,
                //    onGalleryAction: () => PictureController.OpenGallery(OnPictureTaken),
                //    onCameraAction: () => PictureController.OpenCamera(OnPictureTaken));
                btn_cancel = v.Get<Button>("btn_cancel");
                btn_cancel.OnClickAdd(onCancelAction);
                btn_close = v.Get<Button>("btn_close");
                btn_close.OnClickAdd(Hide);

                App.MessagingManager.RegEvent(EventString.Order_Current_Set, _ => ShowCurrentOrder());
            }

            protected override void OnUiShow()
            {
                base.OnUiShow();
                var subState = App.Models.CurrentOrder.SubState;
                var isAssignable = (DoStateMap.IsAssignableSubState(TransitionRoles.User, subState, DoSubState.SenderCancelState));
                btn_cancel.gameObject.SetActive(isAssignable);
            }

            private void ShowCurrentOrder()
            {
                var order = App.Models.CurrentOrder;
                if (order == null) return;
                text_orderId.text = order.Id.ToString();
                view_tabs.SetOrder(order);
                view_deliverItemInfo.SetOrder(order);
                Show();
            }

            private class View_deliverItemInfo : UiBase
            {
                private View_states view_state { get; }
                private View_packageInfo view_packageInfo { get; }

                public View_deliverItemInfo(IView v, bool display = true) : base(v, display)
                {
                    view_packageInfo = new View_packageInfo(v.Get<View>("view_packageInfo"));
                    view_state = new View_states(v.Get<View>("view_state"));
                }

                public void SetOrder(DeliverOrderModel order)
                {
                    var payment = order.PaymentInfo;
                    var deliver = order.DeliveryInfo;
                    var item = order.ItemInfo;
                    var status = (DeliveryOrderStatus)order.Status;
                    view_packageInfo.Set(payment.Charge, deliver.Distance, item.Weight, item.Size());
                    view_state.SetState(status);
                }

                private class View_states : UiBase
                {
                    private Image img_created { get; }
                    private Image img_assigned { get; }
                    private Image img_delivering { get; }
                    private Image img_postDelivery { get; }
                    private Image img_completed { get; }
                    private Image img_exception { get; }
                    private Text text_state { get; }

                    public View_states(IView v, bool display = true) : base(v, display)
                    {
                        img_created = v.Get<Image>("img_created");
                        img_assigned = v.Get<Image>("img_assigned");
                        img_delivering = v.Get<Image>("img_delivering");
                        img_postDelivery = v.Get<Image>("img_postDelivery");
                        img_completed = v.Get<Image>("img_completed");
                        img_exception = v.Get<Image>("img_exception");
                        text_state = v.Get<Text>("text_state");
                    }

                    public void SetState(DeliveryOrderStatus state)
                    {
                        img_created.gameObject.SetActive(state == DeliveryOrderStatus.Created);
                        img_assigned.gameObject.SetActive(state == DeliveryOrderStatus.Assigned);
                        img_delivering.gameObject.SetActive(state == DeliveryOrderStatus.Delivering);
                        img_postDelivery.gameObject.SetActive(state == DeliveryOrderStatus.PostDelivery);
                        img_completed.gameObject.SetActive(state == DeliveryOrderStatus.Completed);
                        img_exception.gameObject.SetActive(state == DeliveryOrderStatus.Exception);
                        text_state.text = state.ToString();
                    }
                }
            }

            private class View_tabs : UiBase
            {
                private Element_tabBtn element_tabBtn_progress { get; }
                private Element_tabBtn element_tabBtn_deliverInfo { get; }

                private Element_tabBtn[] _tabBtns;

                private Element_tabBtn[] TabBtns =>
                    _tabBtns ??= new[] { element_tabBtn_progress, element_tabBtn_deliverInfo };

                private RectTransform trans_content { get; }
                private View_progressInfo view_progressInfo { get; }
                private View_deliveryInfo view_deliveryInfo { get; }

                public View_tabs(IView v,RectTransform parentRect ,bool display = true) : base(v, display)
                {
                    element_tabBtn_progress =
                        new Element_tabBtn(v.Get<View>("element_tabBtn_progress"), () => SetSelected(0));
                    element_tabBtn_deliverInfo =
                        new Element_tabBtn(v.Get<View>("element_tabBtn_deliverInfo"), () => SetSelected(1));
                    trans_content = v.Get<RectTransform>("trans_content");
                    view_progressInfo = new View_progressInfo(v.Get<View>("view_progressInfo"), parentRect.rect.width);
                    view_deliveryInfo = new View_deliveryInfo(v.Get<View>("view_deliveryInfo"), parentRect.rect.width);
                }

                private void SetSelected(int tabIndex)
                {
                    var rectWidthPerContent = trans_content.rect.width / TabBtns.Length;
                    for (var index = 0; index < TabBtns.Length; index++)
                    {
                        var tab = TabBtns[index];
                        tab.SetSelected(index == tabIndex);
                    }
                    trans_content.transform.DOLocalMoveX(rectWidthPerContent * -tabIndex, 0.2f);
                }

                public void SetOrder(DeliveryOrder order)
                {
                    view_deliveryInfo.SetOrder(order);
                    view_progressInfo.Set(order);
                    SetSelected(0);
                }

                private class Element_tabBtn : UiBase
                {
                    private Image img_selected { get; }
                    private Button btn_click { get; }

                    public Element_tabBtn(IView v, Action onButtonClickFunction, bool display = true) : base(v, display)
                    {
                        img_selected = v.Get<Image>("img_selected");
                        btn_click = v.Get<Button>("btn_click");
                        btn_click.OnClickAdd(onButtonClickFunction);
                    }

                    public void SetSelected(bool selected) => img_selected.gameObject.SetActive(selected);
                }

                private class View_deliveryInfo : UiBase
                {
                    private Element_contact element_contactTo { get; }
                    private Element_contact element_contactFrom { get; }
                    private Text text_riderPhone { get; }
                    private Text text_riderName { get; }

                    public View_deliveryInfo(IView v, float width, bool display = true) : base(v, display)
                    {
                        //SetSize
                        v.SetWidth(width);

                        text_riderName = v.Get<Text>("text_riderName");
                        text_riderPhone = v.Get<Text>("text_riderPhone");
                        element_contactTo = new Element_contact(v.Get<View>("element_contactTo"));
                        element_contactFrom = new Element_contact(v.Get<View>("element_contactFrom"));
                    }

                    public void SetOrder(DeliverOrderModel order)
                    {
                        var deliver = order.DeliveryInfo;
                        var sender = order.SenderInfo;
                        var receiver = order.ReceiverInfo;
                        element_contactFrom.Set(sender.Name, sender.PhoneNumber, deliver.StartLocation.Address);
                        element_contactTo.Set(receiver.Name, receiver.PhoneNumber, deliver.EndLocation.Address);
                        text_riderName.text = order.Rider?.Name;
                        text_riderPhone.text = order.Rider?.Phone;
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

                    public Prefab_image(IView v, Sprite image, Action onclickAction, bool display = true) : base(v, display)
                    {
                        img_item = v.Get<Image>("img_item");
                        btn_image = v.Get<Button>("btn_image");
                        img_item.sprite = image;
                        btn_image.OnClickAdd(onclickAction);
                    }
                }
            }
        }
    }
}