using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DataModel;
using UnityEngine.UI;
using Utls;
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
    private Coordinate FromCo { get; set; } = new(0, 0);
    private Coordinate ToCo { get; set; } = new(0, 0);

    private DoVolume CurrentDo { get; set; }
    private AutofillAddressController AutocompleteAddressController => App.GetController<AutofillAddressController>();
    private GeocodingController GeocodingController => App.GetController<GeocodingController>();

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
        }, arg => OnAddressSelected());
        Element_from = new Element_Form(v.GetObject<View>("element_from"), OnInputChanged, arg =>
        {
            ProcessSuggestedAddress(arg, Element_from);
        }, arg => OnAddressSelected());
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

    private void ProcessSuggestedAddress(string input,Element_Form form) => AutocompleteAddressController.GetAddressSuggestions(input, form.SetSuggestedAddress);

    public void Set(float kg, float meter)
    {
        ResetUi();
        Input_kg.text = kg.ToString();
        CurrentDo.Size = meter;
        Show();
    }

    public DeliveryOrder GenerateOrder()
    {
        var order = EntityBase.Instance<DeliveryOrder>();
        order.Id = $"{13000 + Random.Next(500)}";
        order.UserId = 888321;
        order.DeliveryManId = string.Empty;
        order.From = new IdentityInfo(Element_from.Phone, Element_from.Contact, Element_from.Address);
        order.To = new IdentityInfo(Element_to.Phone, Element_to.Contact, Element_to.Address);
        order.Status = 0;
        order.Package = new PackageInfo(CurrentDo.Kg, CurrentDo.GetCost(), CurrentDo.Km, CurrentDo.Size);
        return order;
    }

    private void OnAddressSelected()
    {
        if (Element_from.IsAddressReady && Element_to.IsAddressReady)
        {
            if (!FromCo.IsEqual(Element_from))
                GeocodingController.GetGeocodeFrom(Element_from.Address, r => SetGeo(Element_from, FromCo,r));
            if (!ToCo.IsEqual(Element_to))
                GeocodingController.GetGeocodeTo(Element_to.Address, r => SetGeo(Element_to, ToCo, r));
        }

        void SetGeo(Element_Form form, Coordinate co,
            (bool isSuccess, double lat, double lng, string message) result)
        {
            var (isSuccess, lat, lng, message) = result;
            if (isSuccess)
            {
                co.PlaceId = form.PlaceId;
                form.Lat = co.Lat = lat;
                form.Lng = co.Lng = lng;
                UpdateDistance();
                return;
            }
            UpdateDistance();
        }
    }

    private void UpdateDistance()
    {
        var distance = -1f;
        if (FromCo.HasCoordinate && ToCo.HasCoordinate)
            distance = (float)Distance.CalculateDistance(FromCo.Lat, FromCo.Lng, ToCo.Lat, ToCo.Lng);
        SetDistance(distance);
    }

    private void SetDistance(float km)
    {
        CurrentDo.Km = km;
        Text_km.text = km < 0 ? "-1" : km.ToString("F");
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
        private Sect_Autofill Sect_autofill_address { get; }

        public bool IsReady => IsAddressReady &&
                               !string.IsNullOrWhiteSpace(Input_phone.text) &&
                               !string.IsNullOrEmpty(Input_contact.text);

        public bool IsAddressReady => !string.IsNullOrWhiteSpace(Sect_autofill_address.Input) &&
                                      Sect_autofill_address.Input.Length > 10 ||
                                      !string.IsNullOrWhiteSpace(PlaceId);
        public string PlaceId => Sect_autofill_address.PlaceId;
        public string Contact => Input_contact.text;
        public string Address => Sect_autofill_address.Input;
        public string Phone => Input_phone.text;
        public double Lat { get; set; }
        public double Lng { get; set; }

        public Element_Form(IView v, Action onInputChanged, Action<string> onAddressTriggered,
            Action<(string placeId, string address)> onAddressSelected) : base(v)
        {
            Sect_autofill_address = new Sect_Autofill(v: v.GetObject<View>("sect_autofill_address"), onAddressInputAction: arg =>
            {
                onAddressTriggered?.Invoke(arg);
                onInputChanged?.Invoke();
            },onAddressSelected, charlimit: 40, contentHeight: 30, contentPadding: 5);
            Input_contact = v.GetObject<InputField>("input_contact");
            //Btn_mapPoint = v.GetObject<Button>("btn_mapPoint");
            //Btn_mapPoint.OnClickAdd(() => Input_address.text = preserveAddress);
            Input_phone = v.GetObject<InputField>("input_phone");
            Input_contact.onValueChanged.AddListener(arg => onInputChanged?.Invoke());
            Input_phone.onValueChanged.AddListener(arg => onInputChanged?.Invoke());
        }

        public void SetSuggestedAddress(ICollection<(string placeId,string address)> arg) => Sect_autofill_address.Set(arg);

        public override void ResetUi()
        {
            Input_contact.text = string.Empty;
            Sect_autofill_address.ResetUi();
            Input_phone.text = string.Empty;
        }
    }

    private record Coordinate(double Lat, double Lng, string PlaceId = null)
    {
        public string PlaceId { get; set; }
        public double Lat { get; set; } = Lat;
        public double Lng { get; set; } = Lng;
        public bool HasCoordinate => Lat != 0 && Lng != 0;

        public bool IsEqual(Element_Form form) => PlaceId == form.PlaceId && 
                                                  Math.Abs(Lat - form.Lat) < 0.00001f &&
                                                  Math.Abs(Lng - form.Lng) < 0.00001f;
    }
}