using System;
using UnityEngine;
using UnityEngine.UI;
using Views;

public class ConfirmWindow : WinUiBase
{
    private const string ConfirmText = "Confirm";
    private static Button btn_yes { get; set; }
    private static Button btn_no { get; set; }
    private static Text text_title { get; set; }
    private static Text text_content { get; set; }
    private static GameObject obj_content { get; set; }
    private static ConfirmWindow Instance { get; set; }

    public ConfirmWindow(IView v, UiManager uiManager, bool display = false) : base(v, uiManager, display)
    {
        btn_yes = v.GetObject<Button>("btn_yes");
        btn_no = v.GetObject<Button>("btn_no");
        text_title = v.GetObject<Text>("text_title");
        text_content = v.GetObject<Text>("text_content");
        obj_content = v.GetObject("obj_content");
        Instance = this;
        btn_no.OnClickAdd(Hide);
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
    }
}