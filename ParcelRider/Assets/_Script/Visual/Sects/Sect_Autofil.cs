using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace Visual.Sects
{
    public class Sect_Autofil : UiBase
    {
        private InputField input_autofill { get; }
        private ListViewUi<Prefab_Address> AutofillListView { get; }
        public string Input => input_autofill.text;
        private string SelectedAddress { get; set; }
        private float ContentPadding { get; }
        private float ContentHeight { get; }
        private float ContentLimit { get; }
        public Transform Content => AutofillListView.ScrollRect.content;

        public Sect_Autofil(IView v, Action<string> onAddressInputAction, float contentLimit, float contentHeight,
            float contentPadding) : base(v)
        {
            input_autofill = v.GetObject<InputField>("input_autofill");
            AutofillListView = new ListViewUi<Prefab_Address>(v, "prefab_autofill", "scroll_autofill");
            ContentHeight = contentHeight;
            ContentPadding = contentPadding;
            ContentLimit = contentLimit;
            input_autofill.onValueChanged.AddListener(input =>
            {
                if (SelectedAddress == input) return;
                onAddressInputAction(input);
            });
        }

        public void Set(ICollection<string> addresses)
        {
            AutofillListView.Show();
            ClearList();
            var contentHeight = 0f;
            foreach (var address in addresses)
            {
                var ui = AutofillListView.Instance(v => new Prefab_Address(v, () => OnAddressSelected(address)));
                var size = ui.RectTransform.sizeDelta;
                var line = (address.Length / ContentLimit) + 1;
                ui.RectTransform.sizeDelta = new Vector2(size.x, line * ContentHeight + ContentPadding);
                contentHeight += ui.RectTransform.sizeDelta.y;
                ui.Set(address);
            }
            AutofillListView.ScrollRectSetSizeY(contentHeight);
        }

        private void ClearList() => AutofillListView.ClearList(p => p.Destroy());

        private void OnAddressSelected(string address)
        {
            SelectedAddress = address;
            input_autofill.text = address;
            AutofillListView.ScrollRect.gameObject.SetActive(false);
            ClearList();
        }

        public override void ResetUi()
        {
            input_autofill.text = string.Empty;
            AutofillListView.ClearList(v => v.Destroy());
            AutofillListView.Hide();
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

            public void Set(string address) => text_address.text = address;
        }
    }
}