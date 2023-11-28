using System;
using System.Collections.Generic;
using UnityEngine;

namespace AOT.Views
{
    public class Panel : MonoBehaviour
    {
        [SerializeField] private GameObject _dark;
        [SerializeField] private GameObject _transparent;
        [SerializeField] private Animation _loadingAnim;

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
        }

        public void EndCall(string methodName)
        {
            _ongoingRequests--;
            _pendingMethods.Remove(methodName);
            if (_ongoingRequests == 0)
                gameObject.SetActive(false);
#if UNITY_EDITOR
            if (_ongoingRequests < 0)
                throw new Exception("Ongoing requests cannot be less than 0");
#else
            _ongoingRequests = 0;
#endif
        }
    }
}
