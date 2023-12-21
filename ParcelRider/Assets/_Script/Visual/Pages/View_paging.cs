using System;
using AOT.BaseUis;
using AOT.Views;
using UnityEngine.UI;

namespace Visual.Pages
{
    public class View_paging : UiBase
    {
        private ListView_Scroll<Prefab_page> PageListView { get; }
        public View_paging(IView v, bool display = true) : base(v, display)
        {
            PageListView = new ListView_Scroll<Prefab_page>(v, "prefab_page", "scroll_paging");
        }

        public void Set(int pageCount, Action<int> onclickAction)
        {
            PageListView.ClearList(ui => ui.Destroy());
            for (int i = 0; i < pageCount; i++)
            {
                var page = i + 1;
                var pageIndex = i;
                PageListView.Instance(v => new Prefab_page(v, page, () => SetSelected(pageIndex)));
            }
            SetSelected(0);
            Show();

            void SetSelected(int index)
            {
                for (var i = 0; i < PageListView.List.Count; i++)
                {
                    var page = PageListView.List[i];
                    page.SetSelected(i == index);
                }
                onclickAction?.Invoke(index);
            }
        }


        private class Prefab_page : UiBase
        {
            private Button btn_page { get; }
            private Text text_page { get; }
            private Image img_selected { get; }

            public Prefab_page(IView v, int pageNum, Action onclickAction, bool display = true) : base(v, display)
            {
                btn_page = v.Get<Button>("btn_page");
                text_page = v.Get<Text>("text_page");
                img_selected = v.Get<Image>("img_selected");
                btn_page.OnClickAdd(onclickAction);
                text_page.text = pageNum.ToString();
            }

            public void SetSelected(bool selected) => img_selected.gameObject.SetActive(selected);
        }
    }
}