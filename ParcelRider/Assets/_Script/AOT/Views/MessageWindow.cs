using AOT.BaseUis;
using UnityEngine.UI;

namespace AOT.Views
{
    public class MessageWindow : WinUiBase
    {
        private static Text text_title { get; set; }
        private static Text text_content { get; set; }
        private static Button btn_ok { get; set; }
        private static MessageWindow Instance { get; set; }

        public MessageWindow(IView v, UiManagerBase uiManager, bool display = false) : base(v, uiManager, display)
        {
            text_title = v.GetObject<Text>("text_title");
            text_content = v.GetObject<Text>("text_content");
            btn_ok = v.GetObject<Button>("btn_ok");
            Instance = this;
            btn_ok.OnClickAdd(Hide);
        }

        public static void Set(string title, string content)
        {
            text_title.text = title;
            text_content.text = content;
            Instance.Show();
        }
    }
}