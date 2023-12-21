using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AOT.BaseUis;
using AOT.Core;
using AOT.Helpers;
using AOT.Test;
using AOT.Views;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using UnityEngine;
using UnityEngine.UI;
using Visual.Sects;

namespace Visual.Pages
{
    public class User_NewPackagePage : PageUiBase
    {
        private Button btn_submit { get; }
        private Button btn_cancel { get; }
        private Dropdown drop_state { get; }
        private View_packageInfo view_packageInfo { get; }
        private Element_Form element_to { get; }
        private Element_Form element_from { get; }
        private View_addressList view_addressList { get; }
        private Coordinate FromCo => element_from.Coordinate;
        private Coordinate ToCo => element_to.Coordinate;

        private DoVolume CurrentDo { get; set; }
        private AutofillAddressController AutocompleteAddressController => App.GetController<AutofillAddressController>();
        private GeocodingController GeocodingController => App.GetController<GeocodingController>();

        private string CurrentMyState { get; set; }
        private MyStates[] MyStates { get; set; }
        private bool _isUpperUi;

        public User_NewPackagePage(IView v, Action onSubmit, User_UiManager uiManager) : base(v)
        {
            CurrentDo = new DoVolume();
            btn_submit = v.Get<Button>("btn_submit");
            btn_cancel = v.Get<Button>("btn_cancel");
            drop_state = v.Get<Dropdown>("drop_state");
            InitMyStateDropdown();
            btn_cancel.OnClickAdd(Hide);
            view_packageInfo = new View_packageInfo(v.Get<View>("view_packageInfo"));
            view_addressList = new View_addressList(v.Get<View>("view_addressList"), OnAddressSelected, ProcessSuggestedAddress, PickFromMap);
            element_to = new Element_Form(v.Get<View>("element_to"),UpdateUis, () => ActiveAutoFillSection(true), null);
            element_from = new Element_Form(v.Get<View>("element_from"),UpdateUis, () => ActiveAutoFillSection(false),
                OnBottomInputSelectAction);
            btn_submit.interactable = false;
            btn_submit.OnClickAdd(onSubmit);
        }

        private void OnAddressSelected(string address)
        {
            if (_isUpperUi)
                element_to.SetAddress(address);
            else
                element_from.SetAddress(address);
            Action<string, Action<(bool, double, double, string)>> geoReq =
                _isUpperUi ? GeocodingController.GetGeocodeTo : GeocodingController.GetGeocodeFrom;
            geoReq.Invoke(address, arg =>
            {
                var (success, lat, lng, message) = arg;
                if (success)
                {
                    var co = _isUpperUi ? ToCo : FromCo;
                    var form = _isUpperUi ? element_to : element_from;
                    co.Lat = lat;
                    co.Lng = lng;
                    form.SetCoordinate(co);
                    return;
                }
                MessageWindow.Set("Error", message);
            });
        }

        private void PickFromMap(string address)
        {
            if (address.Length < 4)
                address += CurrentMyState;
            GeocodingController.GetGeoFromAddress(address, arg =>
            {
                var (success, lat, lng, message) = arg;
                App.SetMap(lat, lng, OnGeoCallback);
            });
            return;

            void OnGeoCallback((double lat,double lng) arg)
            {
                var (lat, lng) = arg;
                var form = _isUpperUi ? element_to : element_from;
                var co = new Coordinate(lat, lng);
                SetGeo(form, co);
                GeocodingController.GetAddressFromCoordinates(co.Lat, co.Lng, UpdateAddress);
            }

            void UpdateAddress(bool success, string coAddress)
            {
                if (!success)
                {
                    MessageWindow.Set("Error", "Unable to update address.");
                    return;
                }
                var form = _isUpperUi ? element_to : element_from;
                form.SetAddress(coAddress, true);
            }
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
                        view_addressList.Init(address, OnCancelAddressList);
                });
                return;
            }
            view_addressList.Init(address, null);
        }

        private void OnCancelAddressList(string address)
        {
            element_from.ResetPosition();
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

        public DeliverOrderModel GenerateDoModel()
        {
            var order = new DeliverOrderModel
            {
                UserId = App.Models.User.Id,
                User = App.Models.User,
                MyState = CurrentMyState,
                Status = 0,
                ItemInfo = GetItemInfo(),
                PaymentInfo = GetPaymentInfo(),
                DeliveryInfo = GetDeliveryInfo(),
                SenderInfo = new SenderInfoDto
                {
                    User = App.Models.User,
                    UserId = App.Models.User.Id,
                    PhoneNumber = element_from.Phone,
                    Name = element_from.Contact,
                },
                ReceiverInfo = new ReceiverInfoDto
                {
                    PhoneNumber = element_to.Phone,
                    Name = element_to.Contact,
                },
            };
            return order;

            DeliveryInfoDto GetDeliveryInfo()
            {
                return new DeliveryInfoDto
                {
                    Distance = CurrentDo.Km,
                    StartLocation = GetLocation(element_from.Address, FromCo),
                    EndLocation = GetLocation(element_to.Address, ToCo)
                };
            }

            LocationDto GetLocation(string address, Coordinate co)
            {
                return new LocationDto
                {
                    Address = address,
                    Latitude = co.Lat,
                    Longitude = co.Lng
                };
            }

            PaymentInfoDto GetPaymentInfo()
            {
                return new PaymentInfoDto
                {
                    Charge = CurrentDo.GetCost(),
                    Fee = CurrentDo.GetCost(),
                    Method = 0.ToString(),
                };
            }

            ItemInfoDto GetItemInfo()
            {
                return new ItemInfoDto
                {
                    Weight = CurrentDo.Kg, Length = CurrentDo.Length,
                    Width = CurrentDo.Width, Height = CurrentDo.Height
                };
            }
        }

        private void SetGeo(Element_Form form, Coordinate co)
        {
            form.SetCoordinate(co);
            UpdateDistance();

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
            private InputFieldUi input_address { get; }
            private InputFieldUi input_phone { get; }
            private InputFieldUi input_geoLocation { get; }

            public bool IsReady => IsAddressReady &&
                                   !string.IsNullOrWhiteSpace(input_phone.InputField.text) &&
                                   !string.IsNullOrEmpty(input_contact.InputField.text);

            public bool IsAddressReady => !string.IsNullOrWhiteSpace(Address) &&
                                          Address.Length > 10 || IsAllAddressGeoLocated;
            //确保地址被标识(并且有coordinate)
            public bool IsAllAddressGeoLocated { get; private set; }
            public string Contact => input_contact.InputField.text;
            //public string PlaceId { get;private set; } //Sect_autofill_address.PlaceId;
            public string Address => input_geoLocation.InputField.text; //Sect_autofill_address.Input;
            public string Phone => input_phone.InputField.text;
            public Coordinate Coordinate { get; private set; } = new(0, 0);
            public bool IsSelectedInputField => input_contact.InputField.isFocused ||
                                                input_address.InputField.isFocused ||
                                                input_phone.InputField.isFocused ||
                                                input_geoLocation.InputField.isFocused;

            public Element_Form(IView v, Action onInputChanged, Action onGeoInputSelectAction,
                Action<bool> onOtherInputFieldSelectAction) : base(v)
            {
                Mover = v.GameObject.GetComponent<UiMover>();
                input_contact = v.Get<InputFieldUi>("input_contact");
                input_address = v.Get<InputFieldUi>("input_address");
                input_geoLocation = v.Get<InputFieldUi>("input_geoLocation");
                input_phone = v.Get<InputFieldUi>("input_phone");
                //Btn_mapPoint = v.GetObject<Button>("btn_mapPoint");
                //Btn_mapPoint.OnClickAdd(() => Input_address.text = preserveAddress);
                input_geoLocation.OnSelectEvent.AddListener(_ => onGeoInputSelectAction?.Invoke());

                input_contact.OnSelectEvent.AddListener(_ =>
                {
                    onOtherInputFieldSelectAction?.Invoke(true);
                });
                input_address.OnSelectEvent.AddListener(_ =>
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
                input_address.OnDeselectEvent.AddListener(_ =>
                {
                    onOtherInputFieldSelectAction?.Invoke(false);
                });
                input_phone.OnDeselectEvent.AddListener(_ =>
                {
                    onOtherInputFieldSelectAction?.Invoke(false);
                });

                input_geoLocation.InputField.onValueChanged.AddListener(arg => onInputChanged?.Invoke());
                input_address.InputField.onValueChanged.AddListener(arg => onInputChanged?.Invoke());
                input_contact.InputField.onValueChanged.AddListener(arg => onInputChanged?.Invoke());
                input_phone.InputField.onValueChanged.AddListener(arg => onInputChanged?.Invoke());
            }

            public void SetAddress(string address,bool isGeoLocated = false)
            {
                input_geoLocation.InputField.text = address;
                IsAllAddressGeoLocated = isGeoLocated;
            }

            public override void ResetUi()
            {
                input_contact.InputField.text = string.Empty;
                input_phone.InputField.text = string.Empty;
                input_geoLocation.InputField.text = string.Empty;
                input_address.InputField.text = string.Empty;
            }

            public void MoveTo(Vector2 pos, Action<bool> callbackAction) => Mover.Move(pos, 0.2f, false, callbackAction);
            public void MoveOrigin(Action<bool> callbackAction) => Mover.MoveOrigin(0.2f, false, callbackAction);
            public void ResetPosition() => Mover.ResetPosition();

            public void SetCoordinate(Coordinate co)
            {
                Coordinate = co;
                IsAllAddressGeoLocated = true;
            }
        }

        private class View_addressList : UiBase
        {
            private Sect_Autofill sect_autofill { get; }
            private Button btn_close { get; }
            private Button btn_pickFromMap { get; }
            private event Action<string> onCancelCallbackAction;
            public View_addressList(IView v,
                Action<string> onAddressSelected,
                Action<string> onAddressInputAction,
                Action<string> onPickFromMap) : base(v,false)
            {
                sect_autofill = new Sect_Autofill(v: v.Get<View>("sect_autofill"),
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
                btn_close = v.Get<Button>("btn_close");
                btn_close.OnClickAdd(Reset);
                btn_pickFromMap = v.Get<Button>("btn_pickFromMap");
                btn_pickFromMap.OnClickAdd(() =>
                {
                    onPickFromMap(sect_autofill.Input);
                    Reset();
                });
            }

            public void Init(string address, Action<string> onCancelInvokeOnceAction)
            {
                onCancelCallbackAction = onCancelInvokeOnceAction;
                var input = sect_autofill;
                input.SetInputField(address);
                sect_autofill.GameObject.SetActive(true);
                Show();
            }

            public void UpdateList(ICollection<string> addresses)
            {
                var input = sect_autofill;
                input.Set(addresses);
            }

            private void Reset()
            {
                onCancelCallbackAction?.Invoke(sect_autofill.Input);
                Hide();
                sect_autofill.ResetUi();
                onCancelCallbackAction = null;
            }
        }

        private record Coordinate(double Lat, double Lng)
        {
            public double Lat { get; set; } = Lat;
            public double Lng { get; set; } = Lng;
            public bool HasCoordinate => Lat != 0 && Lng != 0;

            public bool IsEqual(Element_Form form) => Math.Abs(Lat - form.Coordinate.Lat) < 0.00001f &&
                                                      Math.Abs(Lng - form.Coordinate.Lng) < 0.00001f;
        }
    }
}