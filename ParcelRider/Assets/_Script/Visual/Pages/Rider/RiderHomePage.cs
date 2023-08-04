using System;
using AOT.Core;
using AOT.DataModel;
using AOT.Views;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using UnityEngine;
using UnityEngine.UI;

namespace Visual.Pages.Rider
{
    internal class RiderHomePage : DoListPage
    {
        private GameObject obj_jobGuide { get; }
        private Button btn_jobList { get; }

        public RiderHomePage(IView v, Action<int> onOrderSelectedAction, Action onJobListAction,
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

        protected override bool OrderListFilter(DeliveryOrder o) => (o.State).IsOnProgressing() && o.Rider?.Id == App.Models.Rider?.Id;
    }
}