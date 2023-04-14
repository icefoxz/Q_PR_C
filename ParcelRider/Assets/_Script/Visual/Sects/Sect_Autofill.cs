using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Core;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace Visual.Sects
{
    public class Sect_Autofill : UiBase
    {
        private InputField input_autofill { get; }
        private ListViewUi<Prefab_Address> AutofillListView { get; }
        public string PlaceId { get; private set; }
        public string Input => input_autofill.text;
        private string SelectedAddress { get; set; }
        private float ContentPadding { get; }
        private float ContentHeight { get; }
        private float Charlimit { get; }
        public Transform Content => AutofillListView.ScrollRect.content;
        private float ScrollRectMaxHeight { get; }
        private const float PosAlign = 25f;
        private event Action<(string placeId,string address)> OnAddressConfirmAction;
        //如果选中了建议地址,则不再触发OnEndEdit
        private bool IsSuggestionTaken { get; set; }

        public Sect_Autofill(IView v,
            Action<string> onAddressInputAction,
            Action<(string placeId, string address)> onAddressConfirmAction,
            float charlimit,
            float contentHeight,
            float contentPadding) : base(v)
        {
            OnAddressConfirmAction = onAddressConfirmAction;
            input_autofill = v.GetObject<InputField>("input_autofill");
            AutofillListView = new ListViewUi<Prefab_Address>(v, "prefab_autofill", "scroll_autofill");
            ContentHeight = contentHeight;
            ContentPadding = contentPadding;
            Charlimit = charlimit;
            input_autofill.onValueChanged.AddListener(input =>
            {
                if (SelectedAddress == input) return;
                IsSuggestionTaken = false;//重置地址内容(是否是自己输入或是选择建议)
                onAddressInputAction(input);
            });
            input_autofill.onEndEdit.AddListener(OnFinishEdit);
            AutofillListView.HideOptions();
            ScrollRectMaxHeight = ((RectTransform)AutofillListView.ScrollRect.transform).sizeDelta.y;
        }


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
            var pos = contentHeight / 2 + PosAlign;
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
                yield return new WaitForSeconds(0.2f);
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

        public override void ResetUi()
        {
            PlaceId = string.Empty;
            input_autofill.text = string.Empty;
            AutofillListView.ClearList(v => v.Destroy());
            AutofillListView.HideOptions();
            IsSuggestionTaken = false;
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