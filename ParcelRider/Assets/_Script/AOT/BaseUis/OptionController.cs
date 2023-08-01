using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AOT.BaseUis
{
    /// <summary>
    /// 最简单的选项控制器，一般仅仅处理生成与销毁的方法。
    /// </summary>
    public interface IOptionController<T> : IUiBase where T : ButtonUi
    {
        void ClearOptions();
        void AddOption(string text, UnityAction<T> onclickAction);
    }
    /// <summary>
    /// 最简单的处理列表生成<see cref="PrefabController{T}.AddUi"/>与移除<see cref="PrefabController{T}.RemoveList"/>>。
    /// 另外必须调用<see cref="PrefabController{T}.BaseInit"/>来初始化此控件。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PrefabController<T> : BaseUi where T : BaseUi
    {
        [SerializeField] protected T opPrefab;
        [SerializeField] protected Transform content;
        private List<T> _list = new List<T>();
        protected IReadOnlyList<T> List => _list;

        /// <summary>
        /// 所有子类必须初始化这个
        /// </summary>
        protected void BaseInit(bool hide = true)
        {
            var uis = content.GetComponentsInChildren<T>();
            foreach (var ui in uis) ui.Hide();
            if (hide) Hide();
        }
        protected void AddUi(Action<T> initAction)
        {
            var ui = Instantiate(opPrefab, content.transform);
            _list.Add(ui);
            initAction.Invoke(ui);
        }

        protected void RemoveList()
        {
            if (_list.Count > 0) _list.ForEach(DestroyObj);
            _list.Clear();
        }
    }

    public abstract class OptionController<T> : PrefabController<T>, IOptionController<T> where T : ButtonUi
    {
        public virtual void ClearOptions() => RemoveList();

        public void AddOption(string text, UnityAction<T> onclickAction)
        {
            AddUi(ui =>
            {
                ui.Set(text, () => onclickAction?.Invoke(ui), false);
                AdditionalOptionInit(text, ui, content);
            });
        }

        /// <summary>
        /// 子类可以添加特别元素设定
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ui"></param>
        /// <param name="parent"></param>
        protected virtual void AdditionalOptionInit(string text, T ui, Transform parent){}

        public override void ResetUi()
        {
            RemoveList();
            Hide();
        }
    }
}