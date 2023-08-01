using System;
using AOT.BaseUis;
using AOT.DataModel;
using AOT.Views;
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
            img_waitState = v.GetObject<Image>("img_waitState");
            img_deliveryState = v.GetObject<Image>("img_deliveryState");
            img_dropState = v.GetObject<Image>("img_dropState");
            img_errState = v.GetObject<Image>("img_errState");
            img_completeState = v.GetObject<Image>("img_completeState");
            view_spots = new View_spots(v.GetObject<View>("view_spots"));
        }

        public void SetState(DeliveryOrder.States state)
        {
            img_waitState.gameObject.SetActive(state is DeliveryOrder.States.None or DeliveryOrder.States.Wait);
            img_deliveryState.gameObject.SetActive(state == DeliveryOrder.States.Delivering);
            img_dropState.gameObject.SetActive(state == DeliveryOrder.States.Collection);
            img_errState.gameObject.SetActive(state == DeliveryOrder.States.Exception);
            img_completeState.gameObject.SetActive(state == DeliveryOrder.States.Complete);
            var spots = state switch
            {
                DeliveryOrder.States.None => 0,
                DeliveryOrder.States.Wait => 1,
                DeliveryOrder.States.Delivering => 3,
                DeliveryOrder.States.Collection => 5,
                DeliveryOrder.States.Complete => 7,
                DeliveryOrder.States.Exception => 0,
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
                img_spot_0 = v.GetObject<Image>("img_spot_0");
                img_spot_1 = v.GetObject<Image>("img_spot_1");
                img_spot_2 = v.GetObject<Image>("img_spot_2");
                img_spot_3 = v.GetObject<Image>("img_spot_3");
                img_spot_4 = v.GetObject<Image>("img_spot_4");
                img_spot_5 = v.GetObject<Image>("img_spot_5");
                img_spot_6 = v.GetObject<Image>("img_spot_6");
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