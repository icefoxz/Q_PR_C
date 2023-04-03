using System;
using System.Runtime.CompilerServices;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Views
{
    public static class UnityEventExtension
    {
        public static void OnClickAdd(this Button btn, Action action, bool removeAllListener = true)
        {
            CheckNull(btn);
            if (removeAllListener) btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action == null ? default : new UnityAction(action));
        }

        private static void CheckNull<T>(T obj, [CallerMemberName] string method = null)
        {
            if (obj == null) throw new NullReferenceException($"{method}(): 物件=null!");
        }

        public static void AddAction(this UnityEvent unityEvent, Action action, bool removeAllListener = true)
        {
            CheckNull(unityEvent);
            if (removeAllListener) unityEvent.RemoveAllListeners();
            unityEvent.AddListener(action == null ? default : new UnityAction(action));
        }

        public static void AddAction(this UnityEvent<string> unityEvent, Action<string> action, bool removeAllListener = true)
        {
            CheckNull(unityEvent);
            if (removeAllListener) unityEvent.RemoveAllListeners();
            unityEvent.AddListener(action == null ? default : new UnityAction<string>(action));
        }
        public static void AddAction(this UnityEvent<int> unityEvent, Action<int> action, bool removeAllListener = true)
        {
            CheckNull(unityEvent);
            if (removeAllListener) unityEvent.RemoveAllListeners();
            unityEvent.AddListener(action == null ? default : new UnityAction<int>(action));
        }
        public static void AddAction(this UnityEvent<float> unityEvent, Action<float> action, bool removeAllListener = true)
        {
            CheckNull(unityEvent);
            if (removeAllListener) unityEvent.RemoveAllListeners();
            unityEvent.AddListener(action == null ? default : new UnityAction<float>(action));
        }

        //public static void AddAction<T0, T1>(this UnityEvent<T0, T1> unityEvent, Action<T0, T1> action,
        //    bool removeAllListener = true)
        //{
        //    if (removeAllListener) unityEvent.RemoveAllListeners();
        //    unityEvent.AddListener(action == null ? default : new UnityAction<T0, T1>(action));
        //}

        //public static void AddAction<T0, T1, T2>(this UnityEvent<T0, T1, T2> unityEvent, Action<T0, T1, T2> action,
        //    bool removeAllListener = true)
        //{
        //    if (removeAllListener) unityEvent.RemoveAllListeners();
        //    unityEvent.AddListener(action == null ? default : new UnityAction<T0, T1, T2>(action));
        //}

        //public static void AddAction<T0, T1, T2, T3>(this UnityEvent<T0, T1, T2, T3> unityEvent,
        //    Action<T0, T1, T2, T3> action, bool removeAllListener = true)
        //{
        //    if (removeAllListener) unityEvent.RemoveAllListeners();
        //    unityEvent.AddListener(action == null ? default : new UnityAction<T0, T1, T2, T3>(action));
        //}
    }
}