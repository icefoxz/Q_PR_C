using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DataModel;
using OrderHelperLib.Contracts;
using UnityEngine.UI;
using Utls;
using Views;
using Visual.Sects;

public class NewPackagePage : PageUiBase
{
    private Button btn_submit { get; }
    private Button btn_cancel { get; }
    private Dropdown drop_state { get; }
    private View_packageInfo view_packageInfo { get; }
    private Element_Form element_to { get; }
    private Element_Form element_from { get; }
    private Coordinate FromCo { get; set; } = new(0, 0);
    private Coordinate ToCo { get; set; } = new(0, 0);

    private DoVolume CurrentDo { get; set; }
    private AutofillAddressController AutocompleteAddressController => App.GetController<AutofillAddressController>();
    private GeocodingController GeocodingController => App.GetController<GeocodingController>();

    public string CurrentMyState { get; set; }
    private MyStates[] MyStates { get; set; }

    public NewPackagePage(IView v, Action onSubmit,Action onCancelAction,UiManager uiManager) : base(v,uiManager)
    {
        CurrentDo = new DoVolume();
        btn_submit = v.GetObject<Button>("btn_submit");
        btn_cancel = v.GetObject<Button>("btn_cancel");
        drop_state = v.GetObject<Dropdown>("drop_state");
        InitMyStateDropdown();
        btn_cancel.OnClickAdd(onCancelAction);
        view_packageInfo = new View_packageInfo(v.GetObject<View>("view_packageInfo"));
        element_to = new Element_Form(v.GetObject<View>("element_to"), OnInputChanged, arg =>
        {
            ProcessSuggestedAddress(arg,element_to);
        }, arg => OnAddressSelected());
        element_from = new Element_Form(v.GetObject<View>("element_from"), OnInputChanged, arg =>
        {
            ProcessSuggestedAddress(arg, element_from);
        }, arg => OnAddressSelected());
        btn_submit.interactable = false;
        btn_submit.OnClickAdd(onSubmit);
    }

    private void InitMyStateDropdown()
    {
        MyStates = Enum.GetValues(typeof(MyStates)).Cast<MyStates>().Select(x => x).ToArray();
        drop_state.ClearOptions();
        drop_state.onValueChanged.AddListener(SetState);
        drop_state.AddOptions(MyStates.Select(s => s.Text()).ToList());

        var defaultStateIndex = Array.IndexOf(MyStates, OrderHelperLib.Contracts.MyStates.Sarawak);
        drop_state.value = defaultStateIndex;
        SetState(defaultStateIndex);
    }

    private void SetState(int option)
    {
        var state = MyStates[option];
        CurrentMyState = state.Text();
    }

    private void ProcessSuggestedAddress(string input, Element_Form form)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length < 5) return;
        AutocompleteAddressController.GetAddressSuggestions(input + ", " + CurrentMyState, form.SetSuggestedAddress);
    }

    public void Set(float kg, float length, float width, float height)
    {
        ResetUi();
        CurrentDo.Kg = kg;
        CurrentDo.Height = height;
        CurrentDo.Width = width;
        CurrentDo.Length = length;
        view_packageInfo.UpdateKg(kg);
        view_packageInfo.UpdateCost(CurrentDo.GetCost());
        view_packageInfo.UpdateSize(CurrentDo.GetSize());
        Show();
    }

    public DeliveryOrder GenerateOrder()
    {
        var order = new DeliveryOrder();
        order.UserId = App.Models.User.Id;
        order.From = new IdentityInfo(element_from.Phone, element_from.Contact, element_from.Address);
        order.To = new IdentityInfo(element_to.Phone, element_to.Contact, element_to.Address);
        order.Status = 0;
        order.Package = new PackageInfo(CurrentDo.Kg, CurrentDo.GetCost(), CurrentDo.Km, CurrentDo.Length,
            CurrentDo.Width, CurrentDo.Height);
        return order;
    }

    private void OnAddressSelected()
    {
        if (element_from.IsAddressReady && element_to.IsAddressReady)
        {
            if (!FromCo.IsEqual(element_from))
                GeocodingController.GetGeocodeFrom(element_from.Address, r => SetGeo(element_from, FromCo,r));
            if (!ToCo.IsEqual(element_to))
                GeocodingController.GetGeocodeTo(element_to.Address, r => SetGeo(element_to, ToCo, r));
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
        view_packageInfo.UpdateKm(km);
        view_packageInfo.UpdateSize(CurrentDo.GetSize());
    }

    private void OnInputChanged()
    {
        var isKgGotValue = CurrentDo.Kg > 0;
        if (isKgGotValue) view_packageInfo.UpdateSize(CurrentDo.GetSize());
        var isFromReady = element_from.IsReady;
        var isToReady = element_to.IsReady;
        btn_submit.interactable = isFromReady && isToReady && isKgGotValue;
    }

    public override void ResetUi()
    {
        btn_submit.interactable = false;
        view_packageInfo.Set(0, 0, 0, 0);
        element_to.ResetUi();
        element_from.ResetUi();

    }

    private class Element_Form : UiBase
    {
        private InputField input_contact { get; }
        private InputField input_unit { get; }
        private Button btn_mapPoint { get; }
        private InputField input_phone { get; }
        private Sect_Autofill Sect_autofill_address { get; }

        public bool IsReady => IsAddressReady &&
                               !string.IsNullOrWhiteSpace(input_phone.text) &&
                               !string.IsNullOrEmpty(input_contact.text);

        public bool IsAddressReady => !string.IsNullOrWhiteSpace(Sect_autofill_address.Input) &&
                                      Sect_autofill_address.Input.Length > 10 ||
                                      !string.IsNullOrWhiteSpace(PlaceId);
        public string PlaceId => Sect_autofill_address.PlaceId;
        public string Contact => input_contact.text;
        public string Address => Sect_autofill_address.Input;
        public string Phone => input_phone.text;
        public double Lat { get; set; }
        public double Lng { get; set; }

        public Element_Form(IView v, Action onInputChanged, Action<string> onAddressTriggered,
            Action<(string placeId, string address)> onAddressSelected) : base(v)
        {
            Sect_autofill_address = new Sect_Autofill(v: v.GetObject<View>("sect_autofill_address"),
                onAddressInputAction: arg =>
                {
                    onAddressTriggered?.Invoke(arg);
                    onInputChanged?.Invoke();
                }, onAddressSelected,
                charlimit: 40,
                contentHeight: 30,
                contentPadding: 5,
                yPosAlign: 50);
            input_contact = v.GetObject<InputField>("input_contact");
            //Btn_mapPoint = v.GetObject<Button>("btn_mapPoint");
            //Btn_mapPoint.OnClickAdd(() => Input_address.text = preserveAddress);
            input_phone = v.GetObject<InputField>("input_phone");
            input_contact.onValueChanged.AddListener(arg => onInputChanged?.Invoke());
            input_phone.onValueChanged.AddListener(arg => onInputChanged?.Invoke());
        }

        public void SetSuggestedAddress(ICollection<(string placeId,string address)> arg) => Sect_autofill_address.Set(arg);

        public override void ResetUi()
        {
            input_contact.text = string.Empty;
            Sect_autofill_address.ResetUi();
            input_phone.text = string.Empty;
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