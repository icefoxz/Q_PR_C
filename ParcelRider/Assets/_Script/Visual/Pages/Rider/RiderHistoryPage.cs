using System;
using System.Linq;
using AOT.Core;
using AOT.DataModel;
using AOT.Views;

namespace Visual.Pages.Rider
{
    internal class RiderHistoryPage : RiderOrderListPage
    {
        protected override string SubscribeDoUpdateEventName => EventString.Orders_History_Update;

        public RiderHistoryPage(IView v, Action<long> onOrderSelectedAction, Rider_UiManager uiManager,
            bool display = false) : base(v, onOrderSelectedAction, uiManager, display)
        {
            //App.RegEvent(EventString.Orders_Assigned_Update, _ => OnOrderListUpdate());
        }

        protected override DeliveryOrder[] OnOrderListUpdate()
        {
            var model = App.Models;
            //var jobDoneList = model.AssignedOrders.Orders
            //    .Where(o => DoStateMap.GetState(o.SubState)?.IsRiderJobDone()?? false)
            //    .ToArray();
            return model.History.Orders.ToArray();
        }
    }
}