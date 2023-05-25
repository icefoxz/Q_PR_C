using UnityEngine;
using UnityEngine.UI;

namespace Visual.BaseUis
{
    /// <summary>
    /// 弹窗信息条
    /// </summary>
    public class PopupUi : ButtonUi
    {
        [SerializeField] private Image CircleImage;

        protected override void AdditionInit(Button button, Text text)
        {
            button.onClick.AddListener(() => gameObject.SetActive(false));
        }
    }
}