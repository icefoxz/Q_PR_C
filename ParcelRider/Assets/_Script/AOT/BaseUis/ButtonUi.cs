using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AOT.BaseUis
{
    /// <summary>
    /// 一般按键的控制器，主要是统一设定<see cref="Set"/>方式，子类可以透过<see cref="AdditionInit"/>追加初始化
    /// </summary>
    public class ButtonUi : BaseUi
    {
        [SerializeField] private Button Button;
        [SerializeField] private Text Text;

        public void Set(string text, UnityAction onclickAction, bool onceInvocation)
        {
            Show();
            Button.onClick.AddListener(() =>
            {
                if (onceInvocation) Button.onClick.RemoveAllListeners();
                onclickAction?.Invoke();
            });
            Text.text = text;
            AdditionInit(Button, Text);
        }
        /// <summary>
        /// 如果原来的初始化不满足，可以透过之类叠加初始方法
        /// </summary>
        /// <param name="button"></param>
        /// <param name="text"></param>
        protected virtual void AdditionInit(Button button, Text text){}
        public void SetInteraction(bool enable) => Button.interactable = enable;
        public override void ResetUi()
        {
            Button.onClick.RemoveAllListeners();
            Text.text = string.Empty;
        }
    }
}