using System;
using Core;
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
    private Button btn_account { get; }
    private Button btn_logout { get; }
    private View_Lingau view_lingau { get; }
    public View_AccountSect(IView v, Action onAccountAction,Action logoutAction) : base(v, false)
    {
        text_name = v.GetObject<Text>("text_name");
        img_ico = v.GetObject<Image>("img_ico");
        btn_account = v.GetObject<Button>("btn_account");
        btn_logout = v.GetObject<Button>("btn_logout");
        view_lingau = new View_Lingau(v.GetObject<View>("view_lingau"), null);
        btn_account.OnClickAdd(onAccountAction);
        btn_logout.OnClickAdd(logoutAction);
        RegEvents();
    }

    private void RegEvents()
    {
        App.MessagingManager.RegEvent(EventString.User_Update, _ =>
        {
            var user = App.Models.User;
            if (user != null) Set(user.Name, user.Avatar);
        });
    }

    public void Set(string name, Sprite ico = null)
    {
        text_name.text = name;
        if (ico!=null) img_ico.sprite = ico;
        Show();
    }

    public void SetLingau(float lingau) => view_lingau.Set(lingau);
}

public class View_Lingau : UiBase
{
    private Image img_lingau { get; }
    private Button btn_lingau { get; }
    private Text text_value { get; }
    public View_Lingau(IView v,Action onclickAction) : base(v)
    {
        img_lingau = v.GetObject<Image>("img_lingau");
        text_value = v.GetObject<Text>("text_value");
        btn_lingau = v.GetObject<Button>("btn_lingau");
        btn_lingau.OnClickAdd(onclickAction);
    }

    public void Set(float value) => text_value.text = value.ToString("##.##");
}