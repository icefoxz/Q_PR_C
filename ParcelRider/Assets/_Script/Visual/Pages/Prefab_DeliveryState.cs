using System;
using AOT.BaseUis;
using AOT.DataModel;
using AOT.Views;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using UnityEngine;
using UnityEngine.UI;

namespace Visual.Pages
{
    public class Prefab_DeliveryState : UiBase
    {
        private Image img_waitState { get; }
        private Image img_deliveryState { get; }
        private Image img_dropState { get; }
        private Image img_errState { get; }
        private Image img_completeState { get; }
        private View_spots view_spots { get; }

        public Prefab_DeliveryState(IView v) : base(v)
        {
            img_waitState = v.Get<Image>("img_waitState");
            img_deliveryState = v.Get<Image>("img_deliveryState");
            img_dropState = v.Get<Image>("img_dropState");
            img_errState = v.Get<Image>("img_errState");
            img_completeState = v.Get<Image>("img_completeState");
            view_spots = new View_spots(v.Get<View>("view_spots"));
        }

        public void SetState(DeliveryOrderStatus state)
        {
            img_waitState.gameObject.SetActive(state is DeliveryOrderStatus.Created);
            img_deliveryState.gameObject.SetActive(state is DeliveryOrderStatus.Assigned);
            img_dropState.gameObject.SetActive(state is DeliveryOrderStatus.Delivering);
            img_errState.gameObject.SetActive(state == DeliveryOrderStatus.Exception);
            img_completeState.gameObject.SetActive(state == DeliveryOrderStatus.Completed);
            var spots = state switch
            {
                DeliveryOrderStatus.Created => 0,
                DeliveryOrderStatus.Exception => 0,
                DeliveryOrderStatus.Canceled => 0,
                DeliveryOrderStatus.Assigned => 3,
                DeliveryOrderStatus.Delivering => 4,
                DeliveryOrderStatus.Completed => 7,
                DeliveryOrderStatus.Closed => 7,
                DeliveryOrderStatus.PostDelivery => 5,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
            view_spots.SetSpots(spots);
        }

        private class View_spots : UiBase
        {
            private Image img_spot_0 { get; }
            private Image img_spot_1 { get; }
            private Image img_spot_2 { get; }
            private Image img_spot_3 { get; }
            private Image img_spot_4 { get; }
            private Image img_spot_5 { get; }
            private Image img_spot_6 { get; }
            private Image[] Spots => new[] {img_spot_0, img_spot_1, img_spot_2, img_spot_3, img_spot_4, img_spot_5, img_spot_6};
            private Material spot_yellow { get; }
            public View_spots(IView v) : base(v)
            {
                img_spot_0 = v.Get<Image>("img_spot_0");
                img_spot_1 = v.Get<Image>("img_spot_1");
                img_spot_2 = v.Get<Image>("img_spot_2");
                img_spot_3 = v.Get<Image>("img_spot_3");
                img_spot_4 = v.Get<Image>("img_spot_4");
                img_spot_5 = v.Get<Image>("img_spot_5");
                img_spot_6 = v.Get<Image>("img_spot_6");
                spot_yellow = v.GetRes<Material>("spot_yellow");
            }

            public void SetSpots(int count)
            {
                for (int i = 0; i < Spots.Length; i++)
                    Spots[i].material = count > i ? spot_yellow : null;
            }
        }
    }
}