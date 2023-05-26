using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.EventSystems;
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
        //private bool IsSuggestionTaken { get; set; }
        private float YPosAlign { get; set; }

        public Sect_Autofill(IView v,
            Action<string> onAddressInputAction,
            Action<(string placeId, string address)> onAddressConfirmAction,
            Action onCloseAction,
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
                onAddressInputAction(input);
            });
            btn_x.OnClickAdd(onCloseAction);
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

        public void SetInputField(string input)
        {
            input_autofill.text = input;
            input_autofill.caretPosition = input.Length;
            input_autofill.ActivateInputField();
            App.MonoService.StartCoroutine(SelectInputFieldWithDelay(input_autofill));
            
            IEnumerator SelectInputFieldWithDelay(InputField inputField)
            {
                // 等待一帧
                yield return null;

                // 现在设置选中的 GameObject
                EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            }
        }

        private void ClearList() => AutofillListView.ClearList(p => p.Destroy());

        private void OnAddressSelected(string placeId, string address)
        {
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