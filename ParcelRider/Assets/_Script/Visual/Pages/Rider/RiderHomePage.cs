using System;
using Core;
using DataModel;
using OrderHelperLib.Contracts;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace Visual.Pages.Rider
{
    internal class RiderHomePage : DoListPage
    {
        private GameObject obj_jobGuide { get; }
        private Button btn_jobList { get; }

        public RiderHomePage(IView v, Action<string> onOrderSelectedAction, Action onJobListAction,
            Rider_UiManager uiManager,
            bool display = false) : base(v, onOrderSelectedAction, uiManager, display)
        {
            obj_jobGuide = v.GetObject("obj_jobGuide");
            btn_jobList = v.GetObject<Button>("btn_jobList");
            btn_jobList.OnClickAdd(onJobListAction.Invoke);
        }

        protected override void OnOrderListUpdate(DeliveryOrder[] deliveryOrders)
        {
            var hasJob = deliveryOrders.Length > 0;
            obj_jobGuide.SetActive(!hasJob);
            btn_jobList.gameObject.SetActive(!hasJob);
        }

        protected override bool OrderListFilter(DeliveryOrder o) => ((DeliveryOrderStatus)o.Status).IsOpen() && o.Rider?.Id == App.Models.Rider?.Id;
    }
}