using Core;

public class ControllerBase : IController
{
    public void SetTestMode(bool isTestMode)
    {
        TestMode = isTestMode;
    }

    protected bool TestMode { get; private set; }
}