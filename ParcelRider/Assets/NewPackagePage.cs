using System;
using DataModel;
using UnityEngine.Events;
using UnityEngine.UI;
using Views;

public class NewPackagePage : UiBase
{
    private static Random Random { get; } = new Random();
    private Button Btn_submit { get; }
    private Text Text_cost { get; }
    private Text Text_km { get; }
    private InputField Input_kg { get; }
    private Element_Form Element_to { get; }
    private Element_Form Element_from { get; }
    private event Func<float> GetCost;
    private event UnityAction<int> OnKmChanged;

    public NewPackagePage(IView v, 
        UnityAction<float> onKgChanged,
        UnityAction<int> onKmChanged,
        Func<float> getCost) : base(v)
    {
        GetCost = getCost;
        OnKmChanged = onKmChanged;
        Btn_submit = v.GetObject<Button>("btn_submit");
        Text_cost = v.GetObject<Text>("text_cost");
        Text_km = v.GetObject<Text>("text_km");
        Input_kg = v.GetObject<InputField>("input_kg");
        Element_to = new Element_Form(v.GetObject<View>("element_to"),
            "Luak Esplanade, 98000 Miri, Sarawak",
            OnValueChanged, OnAddressTriggered);
        Element_from = new Element_Form(v.GetObject<View>("element_from"),
            "Riam Institute of Technology Lot 1305, Mile 3.5, Riam Road, 98000 Miri, Sarawak",
            OnValueChanged, OnAddressTriggered);
        Input_kg.onValueChanged.AddListener(arg =>
        {
            if (!float.TryParse(arg, out var value))
            {
                value = 0;
                Input_kg.text = value.ToString();
            }
            onKgChanged?.Invoke(value);
            OnValueChanged();
        });
        Btn_submit.interactable = false;
        //Btn_submit.OnClickAdd(onSubmit);
    }

    public DeliveryOrder GenerateOrder()
    {
        var order = EntityBase.Instance<DeliveryOrder>();
        order.Id = 13000 + Random.Next(500);
        order.UserId = 888321;
        order.DeliveryManId = -1;
        order.StartPoint = Element_from.Address;
        order.EndPoint = Element_to.Address;
        order.Status = 0;
        return order;
    }

    private void OnAddressTriggered()
    {
        var km = 0;
        if (Element_from.IsAddressReady && Element_to.IsAddressReady)
        {
            km = Random.Next(5, 80);
        }
        OnKmChanged?.Invoke(km);
        Text_km.text = km.ToString();
    }

    private void OnValueChanged()
    {
        var kg = 0f;
        float.TryParse(Input_kg.text, out kg);
        var isKgGotValue = kg > 0;
        if (isKgGotValue) Text_cost.text = $"{GetCost?.Invoke()}";
        var isFromReady = Element_from.IsReady;
        var isToReady = Element_to.IsReady;
        Btn_submit.interactable = isFromReady && isToReady && isKgGotValue;
    }

    public override void ResetUi()
    {
        Btn_submit.interactable = false;
        Text_cost.text = "0";
        Text_km.text = "0";
        Input_kg.text = string.Empty;
        Element_to.ResetUi();
        Element_from.ResetUi();

    }

    private class Element_Form : UiBase
    {
        private InputField Input_contact { get; }
        private InputField Input_address { get; }
        private Button Btn_mapPoint { get; }
        private InputField Input_phone { get; }

        public bool IsReady => IsAddressReady &&
                               !string.IsNullOrWhiteSpace(Input_phone.text) &&
                               !string.IsNullOrEmpty(Input_contact.text);
        public bool IsAddressReady => !string.IsNullOrWhiteSpace(Input_address.text);
        public string Contact => Input_contact.text;
        public string Address => Input_address.text;
        public string Phone => Input_phone.text;

        public Element_Form(IView v,string preserveAddress,Action onInputChanged,Action onAddressTriggered) : base(v)
        {
            Input_contact = v.GetObject<InputField>("input_contact");
            Input_address = v.GetObject<InputField>("input_address");
            Btn_mapPoint = v.GetObject<Button>("btn_mapPoint");
            Btn_mapPoint.OnClickAdd(() => Input_address.text = preserveAddress);
            Input_phone = v.GetObject<InputField>("input_phone");
            Input_address.onValueChanged.AddListener(arg =>
            {
                onInputChanged?.Invoke();
                onAddressTriggered?.Invoke();
            });
            Input_contact.onValueChanged.AddListener(arg=>onInputChanged?.Invoke());
            Input_phone.onValueChanged.AddListener(arg=>onInputChanged?.Invoke());
        }

        public override void ResetUi()
        {
            Input_contact.text = string.Empty;
            Input_address.text = string.Empty;
            Input_phone.text = string.Empty;
        }
    }

}