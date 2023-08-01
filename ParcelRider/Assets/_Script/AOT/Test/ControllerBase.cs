using AOT.Core;

namespace AOT.Test
{
    public class ControllerBase : IController
    {
        public void SetTestMode(bool isTestMode)
        {
            TestMode = isTestMode;
        }

        protected bool TestMode { get; private set; }
    }
}