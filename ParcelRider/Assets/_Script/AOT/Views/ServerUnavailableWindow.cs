using System.Threading.Tasks;
using AOT.BaseUis;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace AOT.Views
{
    public class ReqRetryWindow : WinUiBase
    {
        private static Text text_content { get; set; }
        private static Button btn_retry { get; set; }
        private static Button btn_cancel { get; set; }
        private static ReqRetryWindow _instance;

        public ReqRetryWindow(IView v, bool display = false) : base(v, display)
        {
            _instance = this;
            text_content = v.Get<Text>("text_content");
            btn_retry = v.Get<Button>("btn_retry");
            btn_cancel = v.Get<Button>("btn_cancel");
        }

        public static async Task<bool> Await(string message)
        {
            var isWaiting = true;
            var retry = false;
            btn_retry.onClick.AddListener(() => SetRetry(true));
            btn_cancel.onClick.AddListener(() => SetRetry(false));
            text_content.text = message;
            _instance.Show();
            await UniTask.WaitWhile(() => isWaiting);
            return retry;

            void SetRetry(bool value)
            {
                text_content.text = string.Empty;
                btn_retry.onClick.RemoveAllListeners();
                btn_cancel.onClick.RemoveAllListeners();
                retry = value; //必须先设置,否则会导致await的时候,还没设置就返回了
                isWaiting = false;
                _instance.Hide();
            }
        }
    }
}