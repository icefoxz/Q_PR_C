using System;
using System.Collections;
using UnityEngine;

namespace AOT.Views
{
    public interface IUiManager
    {
        void ShowPanelWithCall(bool transparent, bool displayLoadingImage = true);
        void HidePanelWithCall();
        void PlayCoroutine(IEnumerator co, bool transparentPanel, Action callback);
    }
    public abstract class UiManagerBase : MonoBehaviour, IUiManager
    {
        private const string UiManagerName = "UiManager";
        [SerializeField] private Panel _panel;
        protected Panel Panel => _panel;

        public virtual void Init(bool startUi)
        {
            _panel.gameObject.SetActive(false);
        }

        public void ShowPanelWithCall(bool transparent, bool displayLoadingImage = true) =>
            _panel.StartCall(UiManagerName, transparent, displayLoadingImage);

        public void HidePanelWithCall() => _panel.EndCall(UiManagerName);

        public void PlayCoroutine(IEnumerator co, bool transparentPanel, Action callback)
        {
            StartCoroutine(WaitForCo());

            IEnumerator WaitForCo()
            {
                ShowPanelWithCall(transparentPanel);
                yield return co;
                callback?.Invoke();
                HidePanelWithCall();
            }
        }
    }
}