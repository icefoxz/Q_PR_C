using System;
using UnityEngine.UI;
using Views;

public class PackageConfirmWindow : WinUiBase
{
    private Button btn_cancel { get; }
    private Button btn_confirm { get; }
    private View_info view_Info { get; }

    public PackageConfirmWindow(IView v, UiManager uiManager) : base(v, uiManager)
    {
        view_Info = new View_info(v.GetObject<View>("view_info"));
        btn_cancel = v.GetObject<Button>("btn_cancel");
        btn_confirm = v.GetObject<Button>("btn_confirm");
        btn_cancel.OnClickAdd(() => uiManager.DisplayWindows(false));
    }


    public void Set(float point, float kg, float length, float width, float height, Action onConfirmAction)
    {
        UiManager.DisplayWindows(true);
        view_Info.Set(point, kg, length, width, height);
        btn_confirm.OnClickAdd(() =>
        {
            onConfirmAction?.Invoke();
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
            text_size.text = $"{meter:F} meter";
        }
    }

}