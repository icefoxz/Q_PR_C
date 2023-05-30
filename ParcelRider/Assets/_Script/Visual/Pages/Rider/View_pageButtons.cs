using System;
using UnityEngine.UI;
using Views;

namespace Visual.Pages.Rider
{
    internal class View_pageButtons : UiBase
    {
        private Element_page element_pageHome { get; }
        private Element_page element_pageHistory { get; }
        private Element_page[] Pages => new[] { element_pageHome, element_pageHistory };
        public View_pageButtons(IView v,
            Action onHomePageAction,
            Action onHistoryPageAction
            , bool display = true) : base(v, display)
        {
            element_pageHome = new Element_page(v.GetObject<View>("element_pageHome"), () =>
            {
                SelectedPage(element_pageHome);
                onHomePageAction?.Invoke();
            }, display);
            element_pageHistory = new Element_page(v.GetObject<View>("element_pageHistory"), () =>
            {
                SelectedPage(element_pageHistory);
                onHistoryPageAction?.Invoke();
            }, display);
            SelectedPage(element_pageHome);
        }

        private void SelectedPage(Element_page page) => Array.ForEach(Pages, p => p.SetSelected(page == p));

        private class Element_page : UiBase
        {
            private Button btn_page { get; }
            private Outline outline { get; }
            public Element_page(IView v, Action onClickAction, bool display = true) : base(v, display)
            {
                btn_page = v.GetObject<Button>("btn_page");
                outline = btn_page.gameObject.GetComponent<Outline>();
                btn_page.OnClickAdd(onClickAction);
            }
            public void SetSelected(bool selected) => outline.enabled = selected;
        }
    }
}