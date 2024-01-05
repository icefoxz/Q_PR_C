using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AOT.Views
{
    public class Panel : MonoBehaviour
    {
        [SerializeField] private GameObject _dark;
        [SerializeField] private GameObject _transparent;
        [SerializeField] private Animation _loadingAnim;

        private bool ShouldShow => _pendingMethods.Count > 0;

        private List<string> _pendingMethods = new List<string>();

        public void StartCall(string methodName, bool transparent, bool displayLoadingImage)
        {
            if (_pendingMethods.Contains(methodName)) return; //避免重复的调用
            _pendingMethods.Add(methodName);
            _dark.SetActive(!transparent);
            _transparent.SetActive(transparent);
            _loadingAnim.gameObject.SetActive(displayLoadingImage);
            gameObject.SetActive(ShouldShow);
        }

        public void EndCall(string methodName)
        {
            _pendingMethods.Remove(methodName);
            StopAllCoroutines();
            StartCoroutine(HidePanelAfterSec(1));
            return;

            IEnumerator HidePanelAfterSec(int secs)
            {
                yield return new WaitForSeconds(secs);
                gameObject.SetActive(ShouldShow);
            }
        }
    }
}
