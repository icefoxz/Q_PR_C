using System;
using AOT.BaseUis;
using AOT.Views;
using UnityEngine.UI;

namespace Visual.Pages
{
    public class PackagePaymentWindow : WinUiBase
    {
        private Button btn_cancel { get; }
        private Button btn_riderCollect { get; }
        private Button btn_deductCredit { get; }
        private Button btn_paymentGateway { get; }
        private View_info view_Info { get; }

        public PackagePaymentWindow(IView v, User_UiManager uiManager) : base(v, uiManager)
        {
            view_Info = new View_info(v.Get<View>("view_info"));
            btn_cancel = v.Get<Button>("btn_cancel");
            btn_riderCollect = v.Get<Button>("btn_riderCollect");
            btn_deductCredit = v.Get<Button>("btn_deductCredit");
            btn_paymentGateway = v.Get<Button>("btn_paymentGateway");
            btn_cancel.OnClickAdd(Hide);
        }

        public void Set(float point, float kg, float length, float width, float height, 
            Action onRiderCollectAction,
            Action onDeductFromPoint, 
            Action onPaymentGateway)
        {
            view_Info.Set(point, kg, length, width, height);
            btn_riderCollect.OnClickAdd(onRiderCollectAction);
            btn_deductCredit.OnClickAdd(onDeductFromPoint);
            btn_paymentGateway.OnClickAdd(onPaymentGateway);
            Show();
        }

        private class View_info : UiBase
        {
            private Text text_point { get; }
            private Text text_weight { get; }
            private Text text_size { get; }
            public View_info(IView v, bool display = true) : base(v, display)
            {
                text_point = v.Get<Text>("text_point");
                text_weight = v.Get<Text>("text_weight");
                text_size = v.Get<Text>("text_size");
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
}