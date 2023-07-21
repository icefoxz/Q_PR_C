using System;
using DataModel;
using Views;

namespace Visual.Pages.Rider
{
    internal class RiderJobListPage : DoListPage
    {
        public RiderJobListPage(IView v, Action<string> onOrderSelectedAction, Rider_UiManager uiManager, bool display = false) : base(v, onOrderSelectedAction, uiManager, display)
        {
        }

        protected override void OnOrderListUpdate(DeliveryOrder[] deliveryOrders)
        {
        }

        protected override bool OrderListFilter(DeliveryOrder order) => order.Status == (int)DeliveryOrder.States.None;
    }
}