using DataModel;
using UnityEngine.UI;
using Views;

public class OrderViewPage : PageUiBase
{
    private enum States
    {
        Waiting ,
        Delivery ,
        Drop ,
        Completed,
        Error 
    }
    private Text Text_orderId { get; }
    private Text Text_from { get; }
    private Text Text_to { get; }
    private Text Text_cost { get; }
    private Text Text_km { get; }
    private Button btn_close { get; }
    private Element_State Elemnent_waitState { get; }
    private Element_State Elemnent_DeliverState { get; }
    private Element_State Elemnent_dropState { get; }
    private Element_State Elemnent_completedState { get; }
    private Element_State Elemnent_errState { get; }

    public OrderViewPage(IView v, UiManager uiManager) : base(v, uiManager)
    {
        Text_orderId = v.GetObject<Text>("text_orderId");
        Text_from = v.GetObject<Text>("text_from");
        Text_to = v.GetObject<Text>("text_to");
        Text_cost = v.GetObject<Text>("text_cost");
        Text_km = v.GetObject<Text>("text_km");
        btn_close = v.GetObject<Button>("btn_close");

        Elemnent_waitState = new Element_State(v.GetObject<View>("element_waitState"));
        Elemnent_DeliverState = new Element_State(v.GetObject<View>("element_deliverState"));
        Elemnent_dropState = new Element_State(v.GetObject<View>("element_dropState"));
        Elemnent_completedState = new Element_State(v.GetObject<View>("element_completedState"));
        Elemnent_errState = new Element_State(v.GetObject<View>("element_errState"));

        btn_close.OnClickAdd(Hide);
    }

    public void Set(DeliveryOrder o)
    {
        Text_orderId.text = o.Id.ToString();
        Text_from.text = o.StartPoint;
        Text_to.text = o.EndPoint;
        Text_cost.text = o.Price.ToString("F");
        Text_km.text = o.Distance.ToString();
        SetState((States)o.Status);
        Show();
    }

    private void SetState(States state)
    {
        Elemnent_waitState.SetActive(state == States.Waiting);
        Elemnent_DeliverState.SetActive(state == States.Delivery);
        Elemnent_dropState.SetActive(state == States.Drop);
        Elemnent_errState.SetActive(state == States.Error);
        Elemnent_completedState.SetActive(state == States.Completed);
    }

    private class Element_State : UiBase
    {
        private Image Img_active { get; }
        public Element_State(IView v) : base(v)
        {
            Img_active = v.GetObject<Image>("img_active");
        }

        public void SetActive(bool active) => Img_active.gameObject.SetActive(active);
    }
}