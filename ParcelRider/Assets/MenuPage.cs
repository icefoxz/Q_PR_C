using System;
using DataModel;
using UnityEngine.UI;
using Views;

public class MenuPage : UiBase
{
    private ListViewUi<Prefab_Order> OrderListView { get; }
    private Action<int> OrderBtnClick { get; set; }
    public MenuPage(View v,Action<int> orderBtnClick) : base(v)
    {
        OrderBtnClick = orderBtnClick;
        OrderListView = new ListViewUi<Prefab_Order>(v, "prefab_order", "scroll_orders");
    }

    public void SetOrders(DeliveryOrder[] orders)
    {
        OrderListView.ClearList(ui=>ui.Destroy());
        foreach (var o in orders)
        {
            var ui = OrderListView.Instance(v => new Prefab_Order(v, id => OrderBtnClick?.Invoke(id)));
            ui.Set(o);
        }
    }

    private class Prefab_Order : UiBase
    {
        private enum States
        {
            WaitToPick ,
            Delivering ,
            Drop ,
            Completed,
            Err ,
        }

        private Image Img_waitState { get; }
        private Image Img_deliverState { get; }
        private Image Img_dropState { get; }
        private Image Img_errState { get; }
        private Image Img_completedState { get; }
        private Text Text_orderId { get; }
        private Text Text_from { get; }
        private Text Text_to { get; }
        private Text Text_cost { get; }
        private Text Text_km { get; }
        private Button Btn_order { get; }
        private int SelectedOrderId { get; set; }

        public Prefab_Order(IView v, Action<int> onBtnClick) : base(v)
        {
            Img_waitState = v.GetObject<Image>("img_waitState");
            Img_deliverState = v.GetObject<Image>("img_deliveryState");
            Img_dropState = v.GetObject<Image>("img_dropState");
            Img_errState = v.GetObject<Image>("img_errState");
            Img_completedState = v.GetObject<Image>("img_completedState");
            Text_orderId = v.GetObject<Text>("text_orderId");
            Text_from = v.GetObject<Text>("text_from");
            Text_to = v.GetObject<Text>("text_to");
            Text_cost = v.GetObject<Text>("text_cost");
            Text_km = v.GetObject<Text>("text_km");
            Btn_order = v.GetObject<Button>("btn_order");
            Btn_order.OnClickAdd(() => onBtnClick?.Invoke(SelectedOrderId));
        }

        public void Set(DeliveryOrder deliveryOrder)
        {
            SelectedOrderId = deliveryOrder.Id;
            var state = (States)deliveryOrder.Status;
            SetState(state);
            Text_orderId.text = deliveryOrder.Id.ToString();
            Text_from.text = ConvertText("from: ",deliveryOrder.StartPoint,15);
            Text_to.text = ConvertText("to: ",deliveryOrder.EndPoint,15);
            Text_cost.text = deliveryOrder.Price.ToString("F");
            Text_km.text = deliveryOrder.Distance.ToString();
        }

        private string ConvertText(string prefix, string text,int maxChars)
        {
            var t = prefix + text;
            return t[..maxChars] + "...";
        }

        private void SetState(States state)
        {
            Img_waitState.gameObject.SetActive(state == States.WaitToPick);
            Img_deliverState.gameObject.SetActive(state == States.Delivering);
            Img_dropState.gameObject.SetActive(state == States.Drop);
            Img_errState.gameObject.SetActive(state == States.Err);
            Img_completedState.gameObject.SetActive(state == States.Completed);
        }
    }
}