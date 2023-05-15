using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Core;
using DataModel;
using OrderHelperLib.Contracts;
using UnityEngine;
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
    private View_addressList view_addressList { get; }
    private Coordinate FromCo { get; set; } = new(0, 0);
    private Coordinate ToCo { get; set; } = new(0, 0);

    private DoVolume CurrentDo { get; set; }
    private AutofillAddressController AutocompleteAddressController => App.GetController<AutofillAddressController>();
    private GeocodingController GeocodingController => App.GetController<GeocodingController>();

    public string CurrentMyState { get; set; }
    private MyStates[] MyStates { get; set; }
    private bool _isUpperUi;

    public NewPackagePage(IView v, Action onSubmit, Action onCancelAction, UiManager uiManager) : base(v, uiManager)
    {
        CurrentDo = new DoVolume();
        btn_submit = v.GetObject<Button>("btn_submit");
        btn_cancel = v.GetObject<Button>("btn_cancel");
        drop_state = v.GetObject<Dropdown>("drop_state");
        InitMyStateDropdown();
        btn_cancel.OnClickAdd(onCancelAction);
        view_packageInfo = new View_packageInfo(v.GetObject<View>("view_packageInfo"));
        view_addressList = new View_addressList(v.GetObject<View>("view_addressList"), arg =>
        {
            var (placeId, address) = arg;
            if (_isUpperUi)
                element_to.SetAddress(placeId, address);
            else
                element_from.SetAddress(placeId, address);
            UpdateGeocode();
        }, arg => ProcessSuggestedAddress(arg));
        element_to = new Element_Form(v.GetObject<View>("element_to"),UpdateUis, () => ActiveAutoFillSection(true), null);
        element_from = new Element_Form(v.GetObject<View>("element_from"),UpdateUis, () => ActiveAutoFillSection(false),
            OnBottomInputSelectAction);
        btn_submit.interactable = false;
        btn_submit.OnClickAdd(onSubmit);
    }

    private void OnBottomInputSelectAction(bool isSelected)
    {
        StartCoroutine(isSelected ? ShakeMoveToUpper() : ShakeMoveToOri());

        IEnumerator ShakeMoveToUpper()
        {
            yield return null;
            if (element_from.IsSelectedInputField) element_from.MoveTo(element_to.RectTransform.anchoredPosition, null);
        }
        IEnumerator ShakeMoveToOri()
        {
            yield return null;
            if (!element_from.IsSelectedInputField) element_from.MoveOrigin(null);
        }
    }

    private void ActiveAutoFillSection(bool to)
    {
        _isUpperUi = to;
        var address = to ? element_to.Address : element_from.Address;
        if (!to) //is from
        {
            element_from.MoveTo(element_to.RectTransform.anchoredPosition, success =>
            {
                if (success)
                    view_addressList.Init(address, element_from.ResetPosition);
            });
            return;
        }
        view_addressList.Init(address, null);
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

    private void ProcessSuggestedAddress(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length < 4) return;
        if ((_isUpperUi && element_to.Address == input) ||
            (!_isUpperUi && element_from.Address == input)) return;
        AutocompleteAddressController.GetAddressSuggestions(input + ", " + CurrentMyState,
            arg => view_addressList.UpdateList(arg));
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

    private void UpdateGeocode()
    {
        if (element_from.IsAddressReady && element_to.IsAddressReady)
        {
            if (!FromCo.IsEqual(element_from))
                GeocodingController.GetGeocodeFrom(element_from.Address, r => SetGeo(element_from, FromCo, r));
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

        void UpdateDistance()
        {
            var distance = -1f;
            if (FromCo.HasCoordinate && ToCo.HasCoordinate)
                distance = (float)Distance.CalculateDistance(FromCo.Lat, FromCo.Lng, ToCo.Lat, ToCo.Lng);
            SetDistance(distance);
        }
    }

    private void SetDistance(float km)
    {
        CurrentDo.Km = km;
        view_packageInfo.UpdateKm(km);
        view_packageInfo.UpdateSize(CurrentDo.GetSize());
        UpdateUis();
    }

    private void UpdateUis()
    {
        var isKgGotValue = CurrentDo.Kg > 0;
        if (isKgGotValue) view_packageInfo.UpdateSize(CurrentDo.GetSize());
        var isFromReady = element_from.IsReady;
        var isToReady = element_to.IsReady;
        view_packageInfo.UpdateCost(CurrentDo.GetCost());
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
        private UiMover Mover { get; }
        private InputFieldUi input_contact { get; }
        private InputFieldUi input_unit { get; }
        private InputFieldUi input_phone { get; }
        private InputFieldUi input_address { get; }
        private Button btn_mapPoint { get; }

        public bool IsReady => IsAddressReady &&
                               !string.IsNullOrWhiteSpace(input_phone.InputField.text) &&
                               !string.IsNullOrEmpty(input_contact.InputField.text);

        public bool IsAddressReady => !string.IsNullOrWhiteSpace(Address) &&
                                      Address.Length > 10 ||
                                      !string.IsNullOrWhiteSpace(PlaceId);
        public string Contact => input_contact.InputField.text;
        public string PlaceId { get;private set; } //Sect_autofill_address.PlaceId;
        public string Address => input_address.InputField.text; //Sect_autofill_address.Input;
        public string Phone => input_phone.InputField.text;
        public double Lat { get; set; }
        public double Lng { get; set; }
        public bool IsSelectedInputField => input_contact.InputField.isFocused ||
                                            input_unit.InputField.isFocused ||
                                            input_phone.InputField.isFocused ||
                                            input_address.InputField.isFocused;

        public Element_Form(IView v, Action onInputChanged, Action onAddressInputSelectAction,
            Action<bool> onOtherInputFieldSelectAction) : base(v)
        {
            Mover = v.GameObject.GetComponent<UiMover>();
            input_contact = v.GetObject<InputFieldUi>("input_contact");
            input_unit = v.GetObject<InputFieldUi>("input_unit");
            input_address = v.GetObject<InputFieldUi>("input_address");
            input_phone = v.GetObject<InputFieldUi>("input_phone");
            //Btn_mapPoint = v.GetObject<Button>("btn_mapPoint");
            //Btn_mapPoint.OnClickAdd(() => Input_address.text = preserveAddress);
            input_address.OnSelectEvent.AddListener(_ => onAddressInputSelectAction?.Invoke());

            input_contact.OnSelectEvent.AddListener(_ =>
            {
                onOtherInputFieldSelectAction?.Invoke(true);
            });
            input_unit.OnSelectEvent.AddListener(_ =>
            {
                onOtherInputFieldSelectAction?.Invoke(true);
            });
            input_phone.OnSelectEvent.AddListener(_ =>
            {
                onOtherInputFieldSelectAction?.Invoke(true);
            });
            input_contact.OnDeselectEvent.AddListener(_ =>
            {
                onOtherInputFieldSelectAction?.Invoke(false);
            });
            input_unit.OnDeselectEvent.AddListener(_ =>
            {
                onOtherInputFieldSelectAction?.Invoke(false);
            });
            input_phone.OnDeselectEvent.AddListener(_ =>
            {
                onOtherInputFieldSelectAction?.Invoke(false);
            });

            input_address.InputField.onValueChanged.AddListener(arg => onInputChanged?.Invoke());
            input_unit.InputField.onValueChanged.AddListener(arg => onInputChanged?.Invoke());
            input_contact.InputField.onValueChanged.AddListener(arg => onInputChanged?.Invoke());
            input_phone.InputField.onValueChanged.AddListener(arg => onInputChanged?.Invoke());
        }

        public void SetAddress(string placeId, string address)
        {
            PlaceId = placeId;
            input_address.InputField.text = address;
        }

        public override void ResetUi()
        {
            input_contact.InputField.text = string.Empty;
            input_phone.InputField.text = string.Empty;
            input_address.InputField.text = string.Empty;
            input_unit.InputField.text = string.Empty;
            PlaceId = string.Empty;
        }

        public void MoveTo(Vector2 pos, Action<bool> callbackAction) => Mover.Move(pos, 0.2f, false, callbackAction);
        public void MoveOrigin(Action<bool> callbackAction) => Mover.MoveOrigin(0.2f, false, callbackAction);
        public void ResetPosition() => Mover.ResetPosition();
    }

    private class View_addressList : UiBase
    {
        private Sect_Autofill sect_autofill { get; }
        private Button btn_close { get; }
        private event Action onCancelCallbackAction;
        public View_addressList(IView v,
            Action<(string placeId, string address)> onAddressSelected,
            Action<string> onAddressInputAction) : base(v,false)
        {
            sect_autofill = new Sect_Autofill(v: v.GetObject<View>("sect_autofill"),
                onAddressInputAction,
                arg =>
                {
                    onAddressSelected?.Invoke(arg);
                    Reset();
                },
                Reset,
                charlimit: 40,
                contentHeight: 30,
                contentPadding: 5,
                yPosAlign: 50);
            btn_close = v.GetObject<Button>("btn_close");
            btn_close.OnClickAdd(Reset);
        }

        public void Init(string address, Action onCancelInvokeOnceAction)
        {
            onCancelCallbackAction = onCancelInvokeOnceAction;
            var input = sect_autofill;
            input.SetInputField(address);
            sect_autofill.GameObject.SetActive(true);
            Show();
        }

        public void UpdateList(ICollection<(string placeId, string address)> arg)
        {
            var input = sect_autofill;
            input.Set(arg);
        }

        private void Reset()
        {
            Hide();
            sect_autofill.ResetUi();
            onCancelCallbackAction?.Invoke();
            onCancelCallbackAction = null;
        }
    }

    private record Coordinate(double Lat, double Lng, string PlaceId = null)
    {
        public string PlaceId { get; set; } = PlaceId;
        public double Lat { get; set; } = Lat;
        public double Lng { get; set; } = Lng;
        public bool HasCoordinate => Lat != 0 && Lng != 0;

        public bool IsEqual(Element_Form form) => PlaceId == form.PlaceId && 
                                                  Math.Abs(Lat - form.Lat) < 0.00001f &&
                                                  Math.Abs(Lng - form.Lng) < 0.00001f;
    }
}