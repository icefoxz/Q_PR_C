using System;
using AOT.BaseUis;
using AOT.Views;
using UnityEngine.UI;

namespace Visual.Pages
{
    public class PaymentPage : PageUiBase
    {
        private Button btn_pay { get; }
        private Button btn_cancel { get; }

        public PaymentPage(IView v, User_UiManager uiManager) : base(v)
        {
            btn_pay = v.Get<Button>("btn_pay");
            btn_cancel = v.Get<Button>("btn_cancel");
            btn_cancel.OnClickAdd(() => GameObject.SetActive(false));
        }

        public void Set(Action<bool> onPaymentAction)
        {
            btn_pay.OnClickAdd(() =>
            {
                //暂时都付款成功!
                var isSuccess = true;
                onPaymentAction?.Invoke(isSuccess);
                Hide();
            });
            Show();
        }
    }
}