using System;
using System.Linq;
using AOT.Core;
using AOT.DataModel;
using AOT.Views;
using UnityEngine.UI;

namespace Visual.Pages.Rider
{
    internal class RiderHomePage : RiderOrderListPage
    {
        private Button btn_jobList { get; }

        public RiderHomePage(IView v, Action<long> onOrderSelectedAction, Action onJobListAction,
            Rider_UiManager uiManager,
            bool display = false) : base(v, onOrderSelectedAction, uiManager, display)
        {
            btn_jobList = v.Get<Button>("btn_jobList");
            btn_jobList.OnClickAdd(onJobListAction.Invoke);
        }

        protected override string SubscribeDoUpdateEventName => EventString.Orders_Assigned_Update;
        protected override DeliveryOrder[] OnOrderListUpdate()
        {
            var assigned = App.Models.Assigned.Orders
                //.Where(o => !DoStateMap.GetState(o.SubState)?.IsRiderJobDone() ?? true)
                .ToArray();
            btn_jobList.gameObject.SetActive(assigned.Length == 0);
            return assigned;
        }
    }
}