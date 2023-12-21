using System;
using AOT.BaseUis;
using UnityEngine;
using UnityEngine.UI;

namespace AOT.Views
{
    public class ConfirmWindow : WinUiBase
    {
        private const string ConfirmText = "Confirm";
        private static Button btn_yes { get; set; }
        private static Button btn_no { get; set; }
        private static Text text_title { get; set; }
        private static Text text_content { get; set; }
        private static GameObject obj_content { get; set; }
        private static ConfirmWindow Instance { get; set; }

        public ConfirmWindow(IView v, UiManagerBase uiManager, bool display = false) : base(v, display)
        {
            btn_yes = v.Get<Button>("btn_yes");
            btn_no = v.Get<Button>("btn_no");
            text_title = v.Get<Text>("text_title");
            text_content = v.Get<Text>("text_content");
            obj_content = v.Get("obj_content");
            Instance = this;
            btn_no.OnClickAdd(Hide);//包一层
        }
        public static void Set(Action onConfirmAction)=> Set(ConfirmText, onConfirmAction);
        public static void Set(Action onConfirmAction, string content) => Set(ConfirmText, content, onConfirmAction);
        public static void Set(string title, Action onConfirmAction) => Set(title, string.Empty, onConfirmAction);
        public static void Set(string title, string content, Action onConfirmAction)
        {
            text_title.text = title;
            text_content.text = content;
            obj_content.SetActive(!string.IsNullOrWhiteSpace(content));
            btn_yes.OnClickAdd(() =>
            {
                onConfirmAction?.Invoke();
                Instance.Hide();
            });
            Instance.Show();
        }
    }
}