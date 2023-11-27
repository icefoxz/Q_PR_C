using System;
using System.Collections;
using System.Collections.Generic;
using AOT.Views;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AOT.BaseUis
{
    public abstract class UiBase : IUiBase
    {
        public GameObject GameObject { get; }
        public Transform Transform { get; }
        public RectTransform RectTransform { get; }
        private IView _v;

        public UiBase(IView v, bool display = true)
        {
            _v = v ?? throw new ArgumentNullException($"{GetType().Name}: view = null!");
            GameObject = v.GameObject;
            Transform = v.GameObject.transform;
            RectTransform = v.RectTransform;
            GameObject.SetActive(display);
        }

        /// <summary>
        /// 当ui显示触发器
        /// </summary>
        protected virtual void OnUiShow() { }
        /// <summary>
        /// 当ui隐藏触发器
        /// </summary>
        protected virtual void OnUiHide() { }
        public void Show() => Display(true);
        public void Hide() => Display(false);
        private void Display(bool display)
        {
            if (display) OnUiShow();
            else OnUiHide();
            GameObject.SetActive(display);
        }
        public virtual void ResetUi() { }

        public Coroutine StartCoroutine(IEnumerator enumerator) => _v.StartCo(enumerator);
        public void StopCoroutine(IEnumerator coroutine) => _v.StopCo(coroutine);
        public void StopAllCoroutines() => _v.StopAllCo();

        public void Destroy() => Object.Destroy(GameObject);
    }
    public class ListViewUi<T> : UiBase
    {
        private readonly ScrollRect _scrollRect;
        private List<T> _list { get; } = new List<T>();
        public IReadOnlyList<T> List => _list;
        public View Prefab { get; }

        public ScrollRect ScrollRect
        {
            get
            {
                if (_scrollRect == null)
                    throw new InvalidOperationException("如果要调用ScrollRect,请在构造的时候传入scrollrect控件");
                return _scrollRect;
            }
        }

        public ListViewUi(View prefab, ScrollRect scrollRect,IView v ,bool hideChildrenViews = true,bool display = true) : base(v,display)
        {
            Prefab = prefab;
            _scrollRect = scrollRect;
            if (hideChildrenViews) HideChildren();
        }

        public ListViewUi(IView v, string prefabName, string scrollRectName, bool hideChildrenViews = true,
            bool display = true) : this(
            v.Get<View>(prefabName),
            v.Get<ScrollRect>(scrollRectName), v, hideChildrenViews, display)
        {
        }

        public void HideChildren()
        {
            foreach (Transform tran in _scrollRect.content.transform)
                tran.gameObject.SetActive(false);
        }

        public T Instance(Func<View> onCreateView, Func<View, T> func)
        {
            var obj = onCreateView();
            obj.gameObject.SetActive(true);
            var ui = func.Invoke(obj);
            _list.Add(ui);
            return ui;
        }
        public T Instance(Func<View, T> func) =>
            Instance(() => Object.Instantiate(Prefab, _scrollRect.content.transform), func);

        public void ClearList(Action<T> onRemoveFromList)
        {
            foreach (var ui in _list) onRemoveFromList(ui);
            _list.Clear();
        }
        public void Remove(T obj) => _list.Remove(obj);

        public void SetVerticalScrollPosition(float value)
        {
            ScrollRect.verticalNormalizedPosition = value;
        }

        public void SetHorizontalScrollPosition(float value)
        {
            ScrollRect.horizontalNormalizedPosition = value;
        }

        public void ScrollRectSetSize(Vector2 size) => ((RectTransform)_scrollRect.transform).sizeDelta = size;

        public void ScrollRectSetSizeX(float x)
        {
            var rect = ((RectTransform)_scrollRect.transform);
            rect.sizeDelta = new Vector2(x, rect.sizeDelta.y);
        }

        public void ScrollRectSetSizeY(float y)
        {
            var rect = ((RectTransform)_scrollRect.transform);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, y);
        }

        public void HideOptions()
        {
            ScrollRect.gameObject.SetActive(false);
        }

        public void ShowOptions() => ScrollRect.gameObject.SetActive(true);
        public override void ResetUi() => HideOptions();
    }
}