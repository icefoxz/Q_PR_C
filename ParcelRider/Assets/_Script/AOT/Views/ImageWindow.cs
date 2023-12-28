using AOT.BaseUis;
using UnityEngine;
using UnityEngine.UI;

namespace AOT.Views
{
    public class ImageWindow : WinUiBase
    {
        private static Image img_image { get; set; }
        private static Button btn_x { get; set; }
        private static Button btn_close { get; set; }
        private static ImageWindow Instance { get; set; }

        public ImageWindow(IView v, bool display = false) : base(v, display)
        {
            img_image = v.Get<Image>("img_image");
            btn_x = v.Get<Button>("btn_x");
            btn_close = v.Get<Button>("btn_close");
            btn_x.OnClickAdd(Hide);
            btn_close.OnClickAdd(Hide);
        }

        public static void Set(Sprite sprite)
        {
            img_image.sprite = sprite;
            Instance.Show();
        }
    }
}