using System;
using System.Collections;
using System.Collections.Generic;
using AOT.Views;
using UnityEngine;
using UnityEngine.Events;
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

        public event UnityAction OnDestroyEvent;
        public void Show() => Display(true);
        public void Hide() => Display(false);
        public void Display(bool display)
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

    /// <summary>
    /// 用于管理列表的ui, 但是不包含滚动条<br/>
    /// 要使用滚动条, 请使用<see cref="ListView_Scroll{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListView_Trans<T> : UiBase
    {
        private List<T> _list { get; } = new List<T>();
        public IReadOnlyList<T> List => _list;
        public View Prefab { get; }
        public Transform Content { get; }

        public ListView_Trans(IView v, View prefab, Transform content, bool hideChildrenViews = true, bool display = true) : base(v, display)
        {
            Prefab = prefab;
            Content = content;
            if (hideChildrenViews) HideChildren();
        }

        public ListView_Trans(IView v, string prefabName, string contentName, bool display = true) : this(v,
            v.Get<View>(prefabName), v.Get<Transform>(contentName), display)
        {

        }

        public ListView_Trans(IView v, string prefabName, bool display = true) : this(v,
            v.Get<View>(prefabName), v.RectTransform, display)
        {
        }

        public void HideChildren()
        {
            foreach (Transform tran in Content)
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
            Instance(() => Object.Instantiate(Prefab, Content), func);

        public void ClearList(Action<T> onRemoveFromList)
        {
            foreach (var ui in _list) onRemoveFromList(ui);
            _list.Clear();
        }

        public void Remove(T obj) => _list.Remove(obj);
    }

    /// <summary>
    /// 滚动条列表ui, 用于管理列表的ui, 包含滚动条<br/>
    /// 如果不用滚动条, 请使用<see cref="ListView_Trans{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListView_Scroll<T> : ListView_Trans<T>
    {
        private readonly ScrollRect _scrollRect;
        public ScrollRect ScrollRect
        {
            get
            {
                if (_scrollRect == null)
                    throw new InvalidOperationException("如果要调用ScrollRect,请在构造的时候传入scrollrect控件");
                return _scrollRect;
            }
        }

        public ListView_Scroll(View prefab, ScrollRect scrollRect, IView v, bool hideChildrenViews = true,
            bool display = true) : base(v, prefab, scrollRect.content, display)
        {
            _scrollRect = scrollRect;
            if (hideChildrenViews) HideChildren();
        }

        public ListView_Scroll(IView v, string prefabName, string scrollRectName, bool hideChildrenViews = true,
            bool display = true) : this(
            v.Get<View>(prefabName),
            v.Get<ScrollRect>(scrollRectName), v, hideChildrenViews, display)
        {
        }

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