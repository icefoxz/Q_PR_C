using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Views;
using Visual.BaseUis;
using Object = UnityEngine.Object;

public abstract class UiBase : IUiBase
{
    public GameObject GameObject { get; }
    public Transform Transform { get; }
    public RectTransform RectTransform { get; }
    private IView _v;

    public UiBase(IView v, bool display = true)
    {
        if (v == null) throw new ArgumentNullException($"{GetType().Name}: view = null!");
        _v = v;
        GameObject = v.GameObject;
        Transform = v.GameObject.transform;
        RectTransform = v.RectTransform;
        GameObject.SetActive(display);
    }

    public virtual void Show() => GameObject.SetActive(true);

    public virtual void Hide() => GameObject.SetActive(false);

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
