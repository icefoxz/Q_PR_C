using System;
using DataModel;
using Views;

namespace Visual.Pages.Rider
{
    internal class JobListPage : DoListPage
    {
        public JobListPage(IView v, Action<string> onOrderSelectedAction, RiderUiManager uiManager, bool display = false) : base(v, onOrderSelectedAction, uiManager, display)
        {
        }

        protected override bool OrderListFilter(DeliveryOrder order) => order.Status == (int)DeliveryOrder.States.None;
    }
}