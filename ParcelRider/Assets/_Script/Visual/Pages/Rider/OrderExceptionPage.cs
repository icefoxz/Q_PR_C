using System;
using System.Collections.Generic;
using UnityEngine.UI;
using Views;

namespace Visual.Pages.Rider
{
    public class OrderExceptionPage : PageUiBase
    {
        private ListViewUi<Prefab_option> OptionListView { get; set; }
        private Button btn_close { get; }
        private Action<string,int> OnOptionSelected { get; set; }

        public OrderExceptionPage(IView v, Action<string, int> onOptionSelected, RiderUiManager uiManager) :
            base(v, uiManager)
        {
            OnOptionSelected = onOptionSelected;
            btn_close = v.GetObject<Button>("btn_close");
            btn_close.OnClickAdd(Hide);
            OptionListView = new ListViewUi<Prefab_option>(v, "prefab_option", "scroll_options", true, false);
        }

        public void SetOptions(string orderId, IList<string> options)
        {
            OptionListView.ClearList(ui => ui.Destroy());
            for (var i = 0; i < options.Count; i++)
            {
                var option = options[i];
                var index = i;
                var prefab = OptionListView.Instance(v => new Prefab_option(v, () =>
                {
                    OnOptionSelected(orderId, index);
                    Hide();
                }));
                prefab.Set(option);
            }
            Show();
        }

        private class Prefab_option : UiBase
        {
            private Text text_description { get; }
            private Button btn_option { get; }
            public Prefab_option(IView v, Action onCLickAction, bool display = true) : base(v, display)
            {
                text_description = v.GetObject<Text>("text_description");
                btn_option = v.GetObject<Button>("btn_option");
                btn_option.OnClickAdd(onCLickAction);
            }

            public void Set(string description) => text_description.text = description;
        }
    }
}