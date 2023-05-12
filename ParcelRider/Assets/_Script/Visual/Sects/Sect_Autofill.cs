using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace Visual.Sects
{
    public class Sect_Autofill : UiBase
    {
        private InputField input_autofill { get; }
        private ListViewUi<Prefab_Address> AutofillListView { get; }
        private Button btn_x { get; }
        public string PlaceId { get; private set; }
        public string Input => input_autofill.text;
        private string SelectedAddress { get; set; }
        private float ContentPadding { get; }
        private float ContentHeight { get; }
        private float Charlimit { get; }
        public Transform Content => AutofillListView.ScrollRect.content;
        private float ScrollRectMaxHeight { get; }
        private event Action<(string placeId,string address)> OnAddressConfirmAction;
        //如果选中了建议地址,则不再触发OnEndEdit
        private bool IsSuggestionTaken { get; set; }
        private float YPosAlign { get; set; }

        public Sect_Autofill(IView v,
            Action<string> onAddressInputAction,
            Action<(string placeId, string address)> onAddressConfirmAction,
            float charlimit,
            float contentHeight,
            float contentPadding,
            float yPosAlign) : base(v)
        {
            OnAddressConfirmAction = onAddressConfirmAction;
            input_autofill = v.GetObject<InputField>("input_autofill");
            AutofillListView = new ListViewUi<Prefab_Address>(v, "prefab_autofill", "scroll_autofill");
            btn_x = v.GetObject<Button>("btn_x");
            ContentHeight = contentHeight;
            ContentPadding = contentPadding;
            Charlimit = charlimit;
            YPosAlign = yPosAlign;
            input_autofill.onValueChanged.AddListener(input =>
            {
                if (SelectedAddress == input) return;
                IsSuggestionTaken = false;//重置地址内容(是否是自己输入或是选择建议)
                onAddressInputAction(input);
            });
            //去掉自动关闭,让建议地址一直存在,直到用户选择或其它控件调用关闭
            //input_autofill.onEndEdit.AddListener(OnFinishEdit);
            btn_x.OnClickAdd(HideOptions);
            AutofillListView.HideOptions();
            ScrollRectMaxHeight = ((RectTransform)AutofillListView.ScrollRect.transform).sizeDelta.y;
        }

        public void HideOptions() => AutofillListView.HideOptions();

        public void Set(ICollection<(string placeId, string address)> arg)
        {
            ClearList();
            var contentHeight = 0f;
            foreach (var (placeId, address) in arg)
            {
                var ui = AutofillListView.Instance(v => new Prefab_Address(v, () => OnAddressSelected(placeId,address)));
                var size = ui.RectTransform.sizeDelta;
                var line = (address.Length / Charlimit) + 1;
                ui.RectTransform.sizeDelta = new Vector2(size.x, line * ContentHeight + ContentPadding);
                contentHeight += ui.RectTransform.sizeDelta.y;
                ui.Set(address);
            }

            if (contentHeight > ScrollRectMaxHeight) contentHeight = ScrollRectMaxHeight;
            AutofillListView.ScrollRectSetSizeY(contentHeight);
            var local = AutofillListView.ScrollRect.transform.localPosition;
            var pos = contentHeight / 2 + YPosAlign;
            AutofillListView.ScrollRect.transform.localPosition = new Vector3(local.x, pos, local.z);
            AutofillListView.ShowOptions();
        }


        private void ClearList() => AutofillListView.ClearList(p => p.Destroy());

        //OnFinishEdit与OnAddressSelected基本上一样,但是为了避免OnFinishEdit是因为点击了listView element触发的
        //所以特别作处理不让它clearList,否则点击的button根本不会执行
        private void OnFinishEdit(string address)
        {
            App.MonoService.StartCoroutine(AdjustmentAfterNewFrame(address));

            IEnumerator AdjustmentAfterNewFrame(string input)
            {
                yield return new WaitForSeconds(1f);
                if (IsSuggestionTaken) yield break; //选中建议地址,所以离开编辑框的时候不用触发
                if (SelectedAddress == input) yield break;
                PlaceId = string.Empty;
                SelectedAddress = input;
                input_autofill.text = input;
                AutofillListView.ScrollRect.gameObject.SetActive(false);
                OnAddressConfirmAction?.Invoke((string.Empty, input));
            }
        }

        private void OnAddressSelected(string placeId, string address)
        {
            IsSuggestionTaken = true;
            SelectedAddress = address;
            PlaceId = placeId;
            input_autofill.text = address;
            AutofillListView.ScrollRect.gameObject.SetActive(false);
            OnAddressConfirmAction?.Invoke((placeId, address));
            ClearList();
        }

        //private bool _isResetting;

        public override void ResetUi()
        {
            //if (_isResetting) return;
            //App.MonoService.StartCoroutine(ResetAfterHalfSeconds());
            Reset();

            void Reset()
            {
                PlaceId = string.Empty;
                input_autofill.text = string.Empty;
                AutofillListView.ClearList(v => v.Destroy());
                AutofillListView.HideOptions();
                IsSuggestionTaken = false;
                //_isResetting = false;
            }

            IEnumerator ResetAfterHalfSeconds()
            {
                yield return new WaitForSeconds(1f);
                Reset();
            }
        }


        private class Prefab_Address : UiBase
        {
            private Text text_address { get; }
            private Button btn_address { get; }

            public Prefab_Address(IView v, Action onclickAction) : base(v)
            {
                text_address = v.GetObject<Text>("text_address");
                btn_address = v.GetObject<Button>("btn_address");
                btn_address.OnClickAdd(onclickAction);
            }

            public void Set(string address)
            {
                text_address.text = address;
            }
        }
    }
}