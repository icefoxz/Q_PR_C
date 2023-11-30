using System;
using System.Linq;
using AOT.Core;
using AOT.DataModel;
using AOT.Views;
using UnityEngine;
using UnityEngine.UI;

namespace Visual.Pages.Rider
{
    internal class RiderHomePage : DoListPage
    {
        private GameObject obj_jobGuide { get; }
        private Button btn_jobList { get; }

        public RiderHomePage(IView v, Action<long> onOrderSelectedAction, Action onJobListAction,
            Rider_UiManager uiManager,
            bool display = false) : base(v, onOrderSelectedAction, uiManager, display)
        {
            obj_jobGuide = v.Get("obj_jobGuide");
            btn_jobList = v.Get<Button>("btn_jobList");
            btn_jobList.OnClickAdd(onJobListAction.Invoke);
        }

        //protected override void OnOrderListUpdate(DeliveryOrder[] deliveryOrders)
        //{
        //    var hasJob = deliveryOrders.Length > 0;
        //    obj_jobGuide.SetActive(!hasJob);
        //    btn_jobList.gameObject.SetActive(!hasJob);
        //}
        //
        //protected override bool OrderListFilter(DeliveryOrder o) => (o.State).IsOnProgressing() && o.Rider?.Id == App.Models.Rider?.Id;
        protected override string SubscribeDoUpdateEventName => EventString.Orders_Unassigned_Update;
        protected override DeliveryOrder[] OnOrderListUpdate() => App.Models.UnassignedOrders.Orders.ToArray();
    }
}