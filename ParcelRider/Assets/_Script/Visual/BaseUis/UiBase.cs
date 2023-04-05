using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Views;
using Visual.BaseUis;

public abstract class UiBase : IUiBase
{
    public GameObject GameObject { get; }
    public Transform Transform { get; }
    public RectTransform RectTransform { get; }

    public UiBase(IView v, bool display = true)
    {
        GameObject = v.GameObject;
        Transform = v.GameObject.transform;
        RectTransform = v.RectTransform;
        GameObject.SetActive(display);
    }

    public virtual void Show() => GameObject.SetActive(true);

    public virtual void Hide() => GameObject.SetActive(false);

    public virtual void ResetUi() { }

    public void Destroy() => UnityEngine.Object.Destroy(GameObject);
}
internal class ListViewUi<T> : UiBase
{
    private readonly ScrollRect _scrollRect;
    private List<T> _list { get; } = new List<T>();
    public IReadOnlyList<T> List => _list;
    private View Prefab { get; }

    public ScrollRect ScrollRect
    {
        get
        {
            if (_scrollRect == null)
                throw new InvalidOperationException("如果要调用ScrollRect,请在构造的时候传入scrollrect控件");
            return _scrollRect;
        }
    }

    public ListViewUi(View prefab, ScrollRect scrollRect,IView v ,bool hideChildrenViews = true) : base(v)
    {
        Prefab = prefab;
        _scrollRect = scrollRect;
        if (hideChildrenViews) HideChildren();
    }

    public ListViewUi(IView v, string prefabName, string scrollRectName, bool hideChildrenViews = true) : this(
        v.GetObject<View>(prefabName),
        v.GetObject<ScrollRect>(scrollRectName), v, hideChildrenViews)
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
        Instance(() => UnityEngine.Object.Instantiate(Prefab, _scrollRect.content.transform), func);

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
}
