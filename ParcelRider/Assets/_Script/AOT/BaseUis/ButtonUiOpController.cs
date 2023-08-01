namespace AOT.BaseUis
{
    /// <summary>
    /// 最基础的<see cref="ButtonUi"/>控制器
    /// </summary>
    public class ButtonUiOpController : OptionController<ButtonUi>
    {
        public void Init() => BaseInit(false);
    }
}