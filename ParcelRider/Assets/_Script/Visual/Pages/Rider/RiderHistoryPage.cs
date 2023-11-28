using System;
using System.Linq;
using AOT.Core;
using AOT.DataModel;
using AOT.Views;

namespace Visual.Pages.Rider
{
    internal class RiderHistoryPage : DoListPage
    {
        protected override string SubscribeDoUpdateEventName => EventString.Orders_History_Update;

        public RiderHistoryPage(IView v, Action<long> onOrderSelectedAction, Rider_UiManager uiManager,
            bool display = false) : base(v, onOrderSelectedAction, uiManager, display)
        {
        }

        protected override DeliveryOrder[] OnOrderListUpdate() => App.Models.History.Orders.ToArray();
    }
}