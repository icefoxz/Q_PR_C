using UnityEngine;
using UnityEngine.Events;

namespace AOT.BaseUis
{
    public class ListBoard : PrefabController<ButtonUi>
    {
        [SerializeField] private bool _alwaysDisplayElements;
        public void Init() => BaseInit(_alwaysDisplayElements);
        public void AddUi(string label,UnityAction onClickAction) => AddUi(ui => ui.Set(label, onClickAction, false));
        public override void ResetUi()
        {
            if (_alwaysDisplayElements) return;
            RemoveList();
        }
    }
}