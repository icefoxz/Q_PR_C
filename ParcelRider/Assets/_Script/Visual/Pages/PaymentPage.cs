using System;
using UnityEngine.UI;
using Views;

public class PaymentPage : PageUiBase
{
    private Button btn_pay { get; }
    private Button btn_cancel { get; }

    public PaymentPage(IView v, Action<bool> onPayCallbackAction, UiManager uiManager) : base(v, uiManager)
    {
        btn_pay = v.GetObject<Button>("btn_pay");
        btn_cancel = v.GetObject<Button>("btn_cancel");
        btn_pay.OnClickAdd(() =>
        {
            //暂时都付款成功!
            var isSuccess = true;
            onPayCallbackAction?.Invoke(isSuccess);
            Hide();
        });
        btn_cancel.OnClickAdd(() => GameObject.SetActive(false));
    }
}