using Views;

public abstract class PageUiBase : UiBase
{
    protected IUiManager UiManager { get; }
    protected PageUiBase(IView v, UiManager uiManager) : base(v)
    {
        UiManager = uiManager;
    }
}