using System;
using DataModel;
using OrderHelperLib.Contracts;
using Views;

namespace Visual.Pages.Rider
{
    internal class RiderHistoryPage : DoListPage
    {
        public RiderHistoryPage(IView v, Action<string> onOrderSelectedAction, Rider_UiManager uiManager, bool display = false) : base(v, onOrderSelectedAction, uiManager, display)
        {
        }

        protected override void OnOrderListUpdate(DeliveryOrder[] deliveryOrders)
        {
        }

        protected override bool OrderListFilter(DeliveryOrder order) => ((DeliveryOrderStatus)order.Status).IsClosed();
    }
}