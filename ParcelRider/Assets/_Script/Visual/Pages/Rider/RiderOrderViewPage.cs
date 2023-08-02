using System;
using System.Collections.Generic;
using AOT.BaseUis;
using AOT.Controllers;
using AOT.Core;
using AOT.DataModel;
using AOT.Views;
using UnityEngine;
using UnityEngine.UI;

namespace Visual.Pages.Rider
{
    public class RiderOrderViewPage : PageUiBase
    {
        private Text text_orderId { get; }
        private Text text_riderName { get; }
        private Text text_riderPhone { get; }
        private View_states view_states { get; }
        private View_packageInfo view_packageInfo { get; }
        private Element_contact element_contactTo { get; }
        private Element_contact element_contactFrom { get; }
        private View_riderOptions view_riderOptions { get; }
        private View_images ViewImages { get; }
        private Button btn_exception { get; }
        private Button btn_close { get; }
        private string OrderId { get; set; }
        private List<Sprite> images { get; set; } = new List<Sprite>();
        private PictureController PictureController => App.GetController<PictureController>();
        private RiderOrderController RiderOrderController => App.GetController<RiderOrderController>();

        public RiderOrderViewPage(IView v, Action<string> onExceptionAction,
            Rider_UiManager uiManager) : base(v, uiManager)
        {
            text_orderId = v.GetObject<Text>("text_orderId");
            text_riderName = v.GetObject<Text>("text_riderName");
            text_riderPhone = v.GetObject<Text>("text_riderPhone");
            view_packageInfo = new View_packageInfo(v.GetObject<View>("view_packageInfo"));
            view_states = new View_states(v.GetObject<View>("view_states"));
            ViewImages = new View_images(v: v.GetObject<View>("view_images"),
                onImageSelectedAction: ImageSelected_PromptImageWindow,
                onGalleryAction: () => PictureController.OpenGallery(OnPictureTaken),
                onCameraAction: () => PictureController.OpenCamera(OnPictureTaken));
            element_contactTo = new Element_contact(v.GetObject<View>("element_contactTo"));
            element_contactFrom = new Element_contact(v.GetObject<View>("element_contactFrom"));
            view_riderOptions = new View_riderOptions(v.GetObject<View>("view_riderOptions")
                , () => TakeOrder_ApiReq(OrderId)
                , () => PickItem_ApiReq(OrderId)
                , () => Collection_ApiReq(OrderId)
                , () => Complete_ApiReq(OrderId));
            btn_exception = v.GetObject<Button>("btn_exception");
            btn_exception.OnClickAdd(() => onExceptionAction(OrderId));
            btn_close = v.GetObject<Button>("btn_close");
            btn_close.OnClickAdd(() => Hide());

            App.MessagingManager.RegEvent(EventString.CurrentOrder_Update, _ => ShowCurrentOrder());
        }

        private void Complete_ApiReq(string orderId) => ConfirmWindow.Set(() => RiderOrderController.Complete(orderId, ShowCurrentOrder), "Complete?");
        private void Collection_ApiReq(string orderId) => ConfirmWindow.Set(() => RiderOrderController.ItemCollection(orderId), "Collection?");
        private void PickItem_ApiReq(string orderId) => ConfirmWindow.Set(() => RiderOrderController.PickItem(orderId), "Pick Item?");
        private void TakeOrder_ApiReq(string orderId) => ConfirmWindow.Set(() => RiderOrderController.Do_AssignRider(orderId), "Take Order?");

        private void OnPictureTaken(Texture2D texture)
        {
            images.Add(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero));
            UpdateImages();
        }
        private void UpdateImages() => ViewImages.Set(images.ToArray());
        private void ImageSelected_PromptImageWindow(int index) => ImageWindow.Set(images[index]);

        public void ShowCurrentOrder()
        {
            var order = App.Models.OrderCollection.Current ?? throw new Exception("No order set to current!");
            OrderId = order.Id;
            var p = order.Package;
            var f = order.From;
            var t = order.To;
            text_orderId.text = order.Id;
            text_riderName.text = order.Rider?.Name;
            text_riderPhone.text = order.Rider?.Phone;
            view_packageInfo.Set(p.Price, p.Size, p.Weight, p.Distance);
            element_contactFrom.Set(f.Name, f.Phone, f.Address);
            element_contactTo.Set(t.Name, t.Phone, t.Address);
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

                element_stateComplete.SetViewActive(state != DeliveryOrder.States.Exception);
                element_stateException.SetViewActive(state == DeliveryOrder.States.Exception);
            }

            private class Element_state : UiBase
            {
                private Image Img_active { get; }
                public Element_state(IView v) : base(v)
                {
                    Img_active = v.GetObject<Image>("img_active");
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

        private class View_images : UiBase
        {
            private ListViewUi<Prefab_image> ImageListView { get; }
            private Button btn_camera { get; }
            private Button btn_gallery { get; }
            private event Action<int> OnImageSelected;

            public View_images(IView v, 
                Action<int> onImageSelectedAction, 
                Action onGalleryAction,
                Action onCameraAction) : base(v)
            {
                OnImageSelected = onImageSelectedAction;
                btn_camera = v.GetObject<Button>("btn_camera");
                btn_camera.OnClickAdd(onCameraAction);
                btn_gallery = v.GetObject<Button>("btn_gallery");
                btn_gallery.OnClickAdd(onGalleryAction);
                ImageListView = new ListViewUi<Prefab_image>(v, "prefab_image", "scroll_image");
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

                public Prefab_image(IView v,Sprite image ,Action onclickAction, bool display = true) : base(v, display)
                {
                    img_item = v.GetObject<Image>("img_item");
                    btn_image = v.GetObject<Button>("btn_image");
                    img_item.sprite = image;
                    btn_image.OnClickAdd(onclickAction);
                }
            }
        }
    }
}