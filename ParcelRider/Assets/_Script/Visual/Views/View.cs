using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Views
{
    /// <summary>
    /// 基于Unity的通用Ui整合标准
    /// </summary>
    public interface IView
    {
        IReadOnlyDictionary<string, GameObject> GetMap();
        RectTransform RectTransform { get; }
        GameObject GameObject { get; }
        GameObject[] GetObjects();
        GameObject GetObject(string objName);
        T GetObject<T>(string objName);
        T GetObject<T>(int index);
        Coroutine StartCo(IEnumerator enumerator);
        void StopCo(IEnumerator enumerator);
        void StopAllCo();
        event Action OnDisableEvent;
        event Action OnEnableEvent; 
        string name { get; }
        View GetView();
    }

    public interface IPage : IView
    {

    }

    /// <summary>
    /// 挂在Ui父件的身上的整合插件
    /// </summary>
    public class View : MonoBehaviour, IView
    {
        [SerializeField] private GameObject[] _components;
        private RectTransform _rectTransform;
        public event Action OnDisableEvent;
        public event Action OnEnableEvent;
        public event Action<IView> OnResetUi;
        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform is null)
                {
                    _rectTransform = _rectTransform = GetComponent<RectTransform>();
                }

                return _rectTransform;
            }
        }

        public View GetView() => this;
        public IReadOnlyDictionary<string, GameObject> GetMap() => _components.ToDictionary(c => c.name, c => c);
        public GameObject GameObject => gameObject;
        public GameObject[] GetObjects() => _components.ToArray();
        public GameObject GetObject(string objName)
        {
            var obj = _components.FirstOrDefault(c => c.name == objName);
            if (!obj) throw new NullReferenceException($"View.{name} 找不到物件名：{objName}");
            return obj;
        }
        public T GetObject<T>(string objName)
        {
            var obj = GetObject(objName).GetComponent<T>();
            //if (obj == null)
            //{
            //    obj = GetObject(objName).GetComponent<T>();
            //}
            return CheckNull(obj);
        }

        private static T CheckNull<T>(T obj)
        {
            if (obj == null)
            {
                throw new NullReferenceException($"物件与{typeof(T).Name}不匹配, 请确保控件存在.");
            }
            return obj;
        }

        public T GetObject<T>(int index) => CheckNull(_components[index].GetComponent<T>());
        void OnDisable() => OnDisableEvent?.Invoke();
        void OnEnable() => OnEnableEvent?.Invoke();
        #region ForDebug
        //void OnEnable()
        //{
        //}
        #endregion        
        public Coroutine StartCo(IEnumerator enumerator) => StartCoroutine(enumerator);
        public void StopCo(IEnumerator enumerator) => StopCoroutine(enumerator);
        public void StopAllCo() => StopAllCoroutines();
    }
}
