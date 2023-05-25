using Views;

/// <summary>
/// 窗口Ui
/// </summary>
public abstract class WinUiBase : PageUiBase
{
    protected WinUiBase(IView v, UiManager uiManager, bool display = false) : base(v, uiManager, display)
    {
    }

    public override void Hide()
    {
        UiManager.DisplayWindows(false);
        base.Hide();
    }

    public override void Show()
    {
        UiManager.DisplayWindows(true);
        base.Show();
    }
}
/// <summary>
/// 页面类
/// </summary>
public abstract class PageUiBase : UiBase
{
    protected IUiManager UiManager { get; }

    protected PageUiBase(IView v, UiManager uiManager, bool display = false) : base(v, display)
    {
        UiManager = uiManager;
    }
}