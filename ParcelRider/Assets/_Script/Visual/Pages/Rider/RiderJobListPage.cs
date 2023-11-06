using System;
using AOT.DataModel;
using AOT.Views;
using OrderHelperLib.Contracts;

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

        protected override bool OrderListFilter(DeliveryOrder order) => order.State == DeliveryOrderStatus.Created;
    }
}