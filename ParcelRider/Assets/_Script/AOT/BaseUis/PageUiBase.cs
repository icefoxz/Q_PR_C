using AOT.Views;

namespace AOT.BaseUis
{
    /// <summary>
    /// 窗口Ui
    /// </summary>
    public abstract class WinUiBase : PageUiBase
    {
        protected WinUiBase(IView v, UiManagerBase uiManager, bool display = false) : base(v, uiManager, display)
        {
        }
    }
    /// <summary>
    /// 页面类
    /// </summary>
    public abstract class PageUiBase : UiBase
    {
        protected UiManagerBase UiManager { get; }

        protected PageUiBase(IView v, UiManagerBase uiManager, bool display = false) : base(v, display)
        {
            UiManager = uiManager;
        }
    }
}