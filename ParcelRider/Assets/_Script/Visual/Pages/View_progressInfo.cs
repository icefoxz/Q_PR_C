using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AOT.BaseUis;
using AOT.Controllers;
using AOT.Core;
using AOT.Views;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using UnityEngine;
using UnityEngine.UI;

namespace Visual.Pages
{
    // progress log
    public class View_progressInfo : UiBase
    {
        private ListView_Scroll<Prefab_deliverLog> LogListView { get; }

        public View_progressInfo(IView v, float width, bool display = true) : base(v, display)
        {
            //SetSize
            v.SetWidth(width);

            LogListView = new ListView_Scroll<Prefab_deliverLog>(v, "prefab_deliverLog", "scroll_deliverLog");
        }

        public void Set(DeliverOrderModel order, int textLimit = 55)
        {
            var history = order.StateHistory ?? Array.Empty<StateSegmentModel>();
            var subStates = DoStateMap.GetAllSubStates();
            var logs = history.Join(subStates, h => h.SubState, s => s.StateId,
                    (h, s) => new { history = h, state = s })
                .Select(a => (a.history.Timestamp, $"{a.state.StateName} {a.history.Remark}"))
                .ToList();

            LogListView.ClearList(l => l.Destroy());
            logs.ForEach(arg =>
            {
                var (time, message) = arg;
                LogListView.Instance(v =>
                {
                    var ui = new Prefab_deliverLog(v);
                    ui.SetMessage(time.ToLocalTime().ToString("hh:mm tt d/M/yy"), ResolveCharsLimit(message, textLimit));
                    return ui;
                });
            });
            SetVerticalToBtmAfterSecs();
        }

        protected override void OnUiShow()
        {
            base.OnUiShow();
            SetVerticalToBtmAfterSecs();
        }

        private async void SetVerticalToBtmAfterSecs(float sec = 0.2f)
        {
            await Task.Delay((int)(sec * 1000));
            LogListView.ScrollRect.verticalNormalizedPosition = 0;
        }

        private string ResolveCharsLimit(string message, int limit)
        {
            if (message.Length <= limit) return message;
            return message.Substring(0, limit) + "...";
        }

        private class Prefab_deliverLog : UiBase
        {
            private Text text_time { get; }
            private Text text_message { get; }
            private View_picture view_picture { get; }
            //private RectTransform rectTransform { get; }
            //private ListViewUi<Prefab_picture> PicListView { get; }

            public Prefab_deliverLog(IView v, bool display = true) : base(v, display)
            {
                //rectTransform = v.RectTransform;
                //PicListView = new ListViewUi<Prefab_picture>(v, "prefab_picture", "scroll_picture");
                text_time = v.Get<Text>("text_time");
                text_message = v.Get<Text>("text_message");
                view_picture = new View_picture(v.Get<View>("view_picture"));
            }

            public void SetMessage(string time, string log)
            {
                //PicListView.ClearList(ui => ui.Destroy());
                //PicListView.ScrollRect.gameObject.SetActive(false);
                RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x,
                    text_message.rectTransform.rect.height);
                text_time.text = time;
                text_message.text = log;
                text_message.gameObject.SetActive(true);
                view_picture.Hide();
            }

            public void SetImages(string time, string imgUrl)
            {
                //rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 100);
                //PicListView.ClearList(ui => ui.Destroy());
                //foreach (var url in imgUrls)
                //{
                //    var ui = PicListView.Instance(v => new Prefab_picture(v, ImageWindow.Set));
                //    ui.SetImage(url);
                //}
                //PicListView.ScrollRect.gameObject.SetActive(true);
                RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x,
                    view_picture.RectTransform.rect.height);
                text_time.text = time;
                view_picture.SetImage(imgUrl);
                text_message.gameObject.SetActive(false);
            }

            private class View_picture : UiBase
            {
                private Image img_pic { get; }
                private Button btn_pic { get; }

                public View_picture(IView v, bool display = true) : base(v,
                    display)
                {
                    img_pic = v.Get<Image>("img_pic");
                    btn_pic = v.Get<Button>("btn_pic");
                }

                public void SetImage(string url)
                {
                    var imgCo = App.GetController<ImageController>();
                    imgCo.Req_Image(url, sp => img_pic.sprite = sp);
                    btn_pic.onClick.RemoveAllListeners();
                    btn_pic.onClick.AddListener(() => ImageWindow.Set(img_pic.sprite));
                    Show();
                }
            }
        }
    }
}