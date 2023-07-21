using System;
using UnityEngine.UI;
using Views;

public class PaymentPage : PageUiBase
{
    private Button btn_pay { get; }
    private Button btn_cancel { get; }

    public PaymentPage(IView v, User_UiManager uiManager) : base(v, uiManager)
    {
        btn_pay = v.GetObject<Button>("btn_pay");
        btn_cancel = v.GetObject<Button>("btn_cancel");
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