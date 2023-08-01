using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AOT.BaseUis
{
    public class ConfirmationWindow : BaseUi
    {
        [SerializeField]private Button confirmButton;
        [SerializeField]private Button cancelButton;
        [SerializeField]private Text confirmText;
        public void Init() => Hide();

        public void Set(string confirmationText,UnityAction onConfirmAction, UnityAction onCancelAction)
        {
            confirmText.text = confirmationText;
            confirmButton.onClick.AddListener(() =>
            {
                ResetUi();
                onConfirmAction?.Invoke();
            });
            cancelButton.onClick.AddListener(() =>
            {
                ResetUi();
                onCancelAction?.Invoke();
            });
            Show();
        }

        public override void ResetUi()
        {
            confirmText.text = string.Empty;
            confirmButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            Hide();
        }
    }
}