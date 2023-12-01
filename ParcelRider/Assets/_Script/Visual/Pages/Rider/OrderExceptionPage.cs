using System;
using System.Collections.Generic;
using AOT.BaseUis;
using AOT.Controllers;
using AOT.Core;
using AOT.Views;
using OrderHelperLib;
using UnityEngine.UI;

namespace Visual.Pages.Rider
{
    public class OrderExceptionPage : PageUiBase
    {
        private ListViewUi<Prefab_option> OptionListView { get; set; }
        private Button btn_close { get; }
        private RiderOrderController RiderOrderController => App.GetController<RiderOrderController>();

        public OrderExceptionPage(IView v, Rider_UiManager uiManager) :
            base(v, uiManager)
        {
            btn_close = v.Get<Button>("btn_close");
            btn_close.OnClickAdd(Hide);
            OptionListView = new ListViewUi<Prefab_option>(v, "prefab_option", "scroll_options", true, false);
            App.RegEvent(EventString.Order_Current_OptionsUpdate, OptionsUpdate);
        }

        private void OptionsUpdate(DataBag b)
        {
            var options = App.Models.CurrentStateOptions;
            OptionListView.ClearList(ui => ui.Destroy());
            for (var i = 0; i < options.Length; i++)
            {
                var op = options[i];
                var index = i;
                var prefab = OptionListView.Instance(v => new Prefab_option(v, onCLickAction: () =>
                {
                    ConfirmWindow.Set(() =>
                    {
                        RiderOrderController.Do_State_Update(op.StateId);
                        Hide();
                    }, $"{op}?");
                }));
                prefab.Set(op.StateName);
            }
            Show();
        }

        private class Prefab_option : UiBase
        {
            private Text text_description { get; }
            private Button btn_option { get; }
            public Prefab_option(IView v, Action onCLickAction, bool display = true) : base(v, display)
            {
                text_description = v.Get<Text>("text_description");
                btn_option = v.Get<Button>("btn_option");
                btn_option.OnClickAdd(onCLickAction);
            }

            public void Set(string description) => text_description.text = description;
        }
    }
}