using UnityEngine;
using UnityEngine.UI;
using Views;

/// <summary>
/// 账号信息板块
/// </summary>
public class View_AccountSect : UiBase
{
    private Text text_name { get; }
    private Image img_ico { get; }
    public View_AccountSect(IView v) : base(v, false)
    {
        text_name = v.GetObject<Text>("text_name");
        img_ico = v.GetObject<Image>("img_ico");
    }

    public void Set(string name, Sprite ico = null)
    {
        text_name.text = name;
        if (ico!=null) img_ico.sprite = ico;
    }
}