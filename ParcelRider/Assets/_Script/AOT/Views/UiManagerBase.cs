using System;
using System.Collections;
using UnityEngine;

namespace AOT.Views
{
    public interface IUiManager
    {
        void ShowPanel(bool transparent, bool displayLoadingImage = true);
        void HidePanel();
        void PlayCoroutine(IEnumerator co, bool transparentPanel, Action callback);
    }
    public abstract class UiManagerBase : MonoBehaviour, IUiManager
    {
        private const string UiManagerName = "UiManager";
        [SerializeField] private Panel _panel;
        protected Panel Panel => _panel;

        public abstract void Init(bool startUi);

        public void ShowPanel(bool transparent, bool displayLoadingImage = true) =>
            _panel.StartCall(UiManagerName, transparent, displayLoadingImage);

        public void HidePanel() => _panel.EndCall(UiManagerName);

        public void PlayCoroutine(IEnumerator co, bool transparentPanel, Action callback)
        {
            StartCoroutine(WaitForCo());

            IEnumerator WaitForCo()
            {
                ShowPanel(transparentPanel);
                yield return co;
                callback?.Invoke();
                HidePanel();
            }
        }
    }
}