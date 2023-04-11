using System;
using System.Collections.Generic;
using Core;
using DataModel;
using UnityEngine.UI;
using Views;
using Visual.Sects;

public class NewPackagePage : PageUiBase
{
    private static Random Random { get; } = new Random();
    private Button Btn_submit { get; }
    private Text Text_cost { get; }
    private Text Text_km { get; }
    private InputField Input_kg { get; }
    private Element_Form Element_to { get; }
    private Element_Form Element_from { get; }

    private DoVolume CurrentDo { get; set; }
    private GoogleAutocompleteAddress AutocompleteAddress => App.GetController<GoogleAutocompleteAddress>();

    public NewPackagePage(IView v, Action onSubmit,UiManager uiManager) : base(v,uiManager)
    {
        CurrentDo = new DoVolume();
        Btn_submit = v.GetObject<Button>("btn_submit");
        Text_cost = v.GetObject<Text>("text_cost");
        Text_km = v.GetObject<Text>("text_km");
        Input_kg = v.GetObject<InputField>("input_kg");
        Input_kg.interactable = false;
        Element_to = new Element_Form(v.GetObject<View>("element_to"), OnInputChanged, arg =>
        {
            ProcessSuggestedAddress(arg,Element_to);
            OnAddressTriggered(arg);
        });
        Element_from = new Element_Form(v.GetObject<View>("element_from"), OnInputChanged, arg=>
        {
            ProcessSuggestedAddress(arg, Element_from);
            OnAddressTriggered(arg);
        });
        Input_kg.onValueChanged.AddListener(arg =>
        {
            if (!float.TryParse(arg, out var value))
            {
                value = 0;
                Input_kg.text = value.ToString();
            }

            CurrentDo.Kg = value;
            OnInputChanged();
        });
        Btn_submit.interactable = false;
        Btn_submit.OnClickAdd(onSubmit);
    }

    private void ProcessSuggestedAddress(string input,Element_Form form) => AutocompleteAddress.GetAddressSuggestions(input, form.SetSuggestedAddress);

    public void Set(float kg, float meter)
    {
        ResetUi();
        Input_kg.text = kg.ToString();
        Show();
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

    private void OnAddressTriggered(string address)
    {
        var km = 0;
        if (Element_from.IsAddressReady && Element_to.IsAddressReady)
        {
            km = Random.Next(5, 80);
        }
        CurrentDo.Km = km;
        Text_km.text = km.ToString();
    }

    private void OnInputChanged()
    {
        var isKgGotValue = CurrentDo.Kg > 0;
        if (isKgGotValue) Text_cost.text = $"{CurrentDo.GetCost()}";
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
        private Button Btn_mapPoint { get; }
        private InputField Input_phone { get; }
        private Sect_Autofil Sect_autofill_address { get; }

        public bool IsReady => IsAddressReady &&
                               !string.IsNullOrWhiteSpace(Input_phone.text) &&
                               !string.IsNullOrEmpty(Input_contact.text);
        public bool IsAddressReady => !string.IsNullOrWhiteSpace(Sect_autofill_address.Input);
        public string Contact => Input_contact.text;
        public string Address => Sect_autofill_address.Input;
        public string Phone => Input_phone.text;

        public Element_Form(IView v, Action onInputChanged, Action<string> onAddressTriggered) : base(v)
        {
            Sect_autofill_address = new Sect_Autofil(v.GetObject<View>("sect_autofill_address"), arg =>
            {
                onAddressTriggered?.Invoke(arg);
                onInputChanged?.Invoke();
            }, 40, 30, 10);
            Input_contact = v.GetObject<InputField>("input_contact");
            //Btn_mapPoint = v.GetObject<Button>("btn_mapPoint");
            //Btn_mapPoint.OnClickAdd(() => Input_address.text = preserveAddress);
            Input_phone = v.GetObject<InputField>("input_phone");
            Input_contact.onValueChanged.AddListener(arg => onInputChanged?.Invoke());
            Input_phone.onValueChanged.AddListener(arg => onInputChanged?.Invoke());
        }

        public void SetSuggestedAddress(ICollection<string> address) => Sect_autofill_address.Set(address);

        public override void ResetUi()
        {
            Input_contact.text = string.Empty;
            Sect_autofill_address.ResetUi();
            Input_phone.text = string.Empty;
        }
    }
}