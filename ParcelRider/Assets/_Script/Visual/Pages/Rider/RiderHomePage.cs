using System;
using System.Linq;
using Controllers;
using Core;
using DataModel;
using OrderHelperLib.Contracts;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace Visual.Pages.Rider
{
    internal class RiderHomePage : PageUiBase
    {
        private View_doList view_doList { get; }
        private GameObject obj_jobGuide { get; }
        private Button btn_jobList { get; }
        private OrderController OrderController => App.GetController<OrderController>();
        public RiderHomePage(IView v, Action<string> onOrderSelectedAction, RiderUiManager uiManager,
            bool display = false) : base(v, uiManager, display)
        {
            obj_jobGuide = v.GetObject("obj_jobGuide");
            btn_jobList = v.GetObject<Button>("btn_jobList");
            view_doList = new View_doList(v.GetObject<View>("view_doList"), onOrderSelectedAction);
        }

        public override void Show()
        {
            var hasJob = OrderController.Current != null;
            obj_jobGuide.SetActive(!hasJob);
            btn_jobList.gameObject.SetActive(!hasJob);
            view_doList.Set(OrderController.Orders
                .Where(o => ((DeliveryOrderStatus)o.Status).IsOpen() && o.Rider?.Id == Auth.RiderId).ToArray());
            base.Show();
        }
    }
}