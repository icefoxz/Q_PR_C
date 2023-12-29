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

        private bool _shouldShow = false;//这个设计是为了延迟调用,如果在同一帧内调用了StartCall和EndCall,那么就不会显示

        private List<string> _pendingMethods = new List<string>();
        private int _ongoingRequests = 0;

        public void StartCall(string methodName,bool transparent, bool displayLoadingImage)
        {
            _ongoingRequests++;
            _pendingMethods.Add(methodName);
            gameObject.SetActive(true);
            _dark.SetActive(!transparent);
            _transparent.SetActive(transparent);
            _loadingAnim.gameObject.SetActive(displayLoadingImage);
            _shouldShow = true;
        }

        public void EndCall(string methodName)
        {
            _ongoingRequests--;
            _pendingMethods.Remove(methodName);
#if UNITY_EDITOR
            if (_ongoingRequests < 0)
                throw new Exception("Ongoing requests cannot be less than 0");
#else
            _ongoingRequests = 0;
            Debug.Log($"Ongoing requests less than 0. ={_ongoingRequests}");
#endif
            if (_ongoingRequests != 0) return;
            _shouldShow = false;
            StopAllCoroutines();
            StartCoroutine(HidePanelAfterSec(1));
            return;

            IEnumerator HidePanelAfterSec(int secs)
            {
                yield return new WaitForSeconds(secs);
                gameObject.SetActive(_shouldShow);
            }
        }

    }
}
