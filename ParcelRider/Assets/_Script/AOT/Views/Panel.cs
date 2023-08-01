using UnityEngine;

namespace AOT.Views
{
    public class Panel : MonoBehaviour
    {
        [SerializeField] private GameObject _dark;
        [SerializeField] private GameObject _transparent;
        [SerializeField] private Animation _loadingAnim;

        public void Show(bool transparent, bool displayLoadingImage)
        {
            gameObject.SetActive(true);
            _dark.SetActive(!transparent);
            _transparent.SetActive(transparent);
            _loadingAnim.gameObject.SetActive(displayLoadingImage);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
