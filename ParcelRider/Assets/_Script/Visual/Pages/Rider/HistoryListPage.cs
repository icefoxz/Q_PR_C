using System;
using DataModel;
using OrderHelperLib.Contracts;
using Views;

namespace Visual.Pages.Rider
{
    internal class HistoryListPage : DoListPage
    {
        public HistoryListPage(IView v, Action<string> onOrderSelectedAction, RiderUiManager uiManager, bool display = false) : base(v, onOrderSelectedAction, uiManager, display)
        {
        }

        protected override bool OrderListFilter(DeliveryOrder order) => ((DeliveryOrderStatus)order.Status).IsClosed();
    }
}