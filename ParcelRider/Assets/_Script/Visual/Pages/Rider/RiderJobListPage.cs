using System;
using System.Linq;
using AOT.Core;
using AOT.DataModel;
using AOT.Views;

namespace Visual.Pages.Rider
{
    internal class RiderJobListPage : RiderOrderListPage
    {
        protected override string SubscribeDoUpdateEventName => EventString.Orders_Unassigned_Update;

        public RiderJobListPage(IView v, Action<long> onOrderSelectedAction, Rider_UiManager uiManager,
            bool display = false) : base(v, onOrderSelectedAction, uiManager, display)
        {
        }

        protected override DeliveryOrder[] OnOrderListUpdate()=>App.Models.UnassignedOrders.Orders.ToArray();
    }
}