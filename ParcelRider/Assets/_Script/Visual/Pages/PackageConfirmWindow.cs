using System;
using UnityEngine.UI;
using Views;

public class PackageConfirmWindow : WinUiBase
{
    private Button btn_cancel { get; }
    private Button btn_riderCollect { get; }
    private Button btn_deductCredit { get; }
    private Button btn_paymentGateway { get; }
    private View_info view_Info { get; }

    public PackageConfirmWindow(IView v, User_UiManager uiManager) : base(v, uiManager)
    {
        view_Info = new View_info(v.GetObject<View>("view_info"));
        btn_cancel = v.GetObject<Button>("btn_cancel");
        btn_riderCollect = v.GetObject<Button>("btn_riderCollect");
        btn_deductCredit = v.GetObject<Button>("btn_deductCredit");
        btn_paymentGateway = v.GetObject<Button>("btn_paymentGateway");
        btn_cancel.OnClickAdd(() => Hide());
    }


    public void Set(float point, float kg, float length, float width, float height, 
        Action onRiderCollectAction,
        Action onDeductFromPoint, 
        Action onPaymentGateway)
    {
        UiManager.DisplayWindows(true);
        view_Info.Set(point, kg, length, width, height);
        btn_riderCollect.OnClickAdd(() =>
        {
            onRiderCollectAction?.Invoke();
            Hide();
        });
        btn_deductCredit.OnClickAdd(() =>
        {
            onDeductFromPoint?.Invoke();
            Hide();
        });
        btn_paymentGateway.OnClickAdd(() =>
        {
            onPaymentGateway?.Invoke();
            Hide();
        });

        Show();
    }

    private class View_info : UiBase
    {
        private Text text_point { get; }
        private Text text_weight { get; }
        private Text text_size { get; }
        public View_info(IView v, bool display = true) : base(v, display)
        {
            text_point = v.GetObject<Text>("text_point");
            text_weight = v.GetObject<Text>("text_weight");
            text_size = v.GetObject<Text>("text_size");
        }

        public void Set(float point, float kg, float length, float width, float height)
        {
            var meter = MathF.Pow(length * width * height, 1 / 3f);
            text_point.text = point.ToString("##.##");
            text_weight.text = $"{kg:F} kg";
            text_size.text = $"{meter:F} m³";
        }
    }

}