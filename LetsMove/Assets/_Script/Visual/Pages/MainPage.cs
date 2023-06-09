using System;
using System.Collections;
using System.Linq;
using Controllers;
using Core;
using DataModel;
using UnityEngine;
using UnityEngine.UI;
using Views;

public class MainPage : PageUiBase
{
    private ListViewUi<Prefab_Order> OrderListView { get; }
    private View_packagePlayer view_packagePlayer { get; }
    private View_historySect view_historySect { get; }
    private OrderController OrderController => App.GetController<OrderController>();

    public MainPage(IView v, UiManager uiManager) : base(v, uiManager)
    {
        OrderListView = new ListViewUi<Prefab_Order>(v, "prefab_order", "scroll_orders");
        view_packagePlayer = new View_packagePlayer(v.GetObject<View>("view_packagePlayer"),
            uiManager, a => UiManager.NewPackage(a.point, a.kg, a.length, a.width, a.height));
        view_historySect = new View_historySect(v.GetObject<View>("view_historySect"), ui => uiManager.ViewOrder(ui));
        RegEvents();
        Hide();//listView代码会导致view active,所以这里要隐藏
    }

    private void RegEvents()
    {
        App.MessagingManager.RegEvent(EventString.Orders_Update, _ => RefreshOrderList());
    }

    private void RefreshOrderList()
    {
        SetOrders(OrderController.Orders.OrderBy(o => o.Status).ToArray());

        void SetOrders(DeliveryOrder[] orders)
        {
            OrderListView.ClearList(ui => ui.Destroy());
            foreach (var o in orders.Where(o => o.Status >= 0))
            {
                var ui = OrderListView.Instance(v => new Prefab_Order(v, id => UiManager.ViewOrder(id)));
                ui.Set(o);
            }

            view_historySect.UpdateHistories(orders.Where(o => o.Status < 0).ToArray());
        }
    }

    public override void Show()
    {
        RefreshOrderList();
        base.Show();
    }

    private class Prefab_Order : UiBase
    {
        private Text text_orderId { get; }
        private Prefab_DeliveryState prefab_deliveryState { get; }
        private Text text_volume { get; }
        private Text text_km { get; }
        private Text text_cost { get; }
        private Text text_to { get; }
        private Text text_contactName { get; }
        private Text text_contactPhone { get; }
        private Text text_riderName { get; }
        private Text text_riderPhone { get; }
        private Button btn_order { get; }
        private string SelectedOrderId { get; set; }

        public Prefab_Order(IView v, Action<string> onBtnClick) : base(v)
        {
            prefab_deliveryState = new Prefab_DeliveryState(v.GetObject<View>("prefab_deliveryState"));
            text_orderId = v.GetObject<Text>("text_orderId");
            text_to = v.GetObject<Text>("text_to");
            text_cost = v.GetObject<Text>("text_cost");
            text_volume = v.GetObject<Text>("text_volume");
            text_km = v.GetObject<Text>("text_km");
            text_contactName = v.GetObject<Text>("text_contactName");
            text_contactPhone = v.GetObject<Text>("text_contactPhone");
            text_riderName = v.GetObject<Text>("text_riderName");
            text_riderPhone = v.GetObject<Text>("text_riderPhone");
            btn_order = v.GetObject<Button>("btn_order");
            btn_order.OnClickAdd(() => onBtnClick?.Invoke(SelectedOrderId));
            
        }

        public void Set(DeliveryOrder o)
        {
            SelectedOrderId = o.Id;
            var state = (DeliveryOrder.States)o.Status;
            prefab_deliveryState.SetState(state);
            text_orderId.text = o.Id;
            text_to.text = ConvertText("to: ", o.To.Address, 15);
            text_cost.text = o.Package.Price.ToString("F");
            text_km.text = o.Package.Distance.ToString("F1") + "km";
            text_volume.text = o.Package.Size.ToString("F1") + "m³";
            text_contactName.text = o.To.Name;
            text_contactPhone.text = o.To.Phone;
            text_riderName.text = o.Rider?.Name;
            text_riderPhone.text = o.Rider?.Phone;
        }

        private string ConvertText(string prefix, string text, int maxChars)
        {
            var t = prefix + text;
            if(t.Length > maxChars) return t[..maxChars] + "...";
            return t;
        }
    }

    private class View_packagePlayer : UiBase
    {
        private View_sizeSwitch view_sizeSwitch { get; }
        private Element_input element_input_height { get; }
        private Element_input element_input_width { get; }
        private Element_input element_input_length { get; }
        private Element_input element_input_weight { get; }
        private View_info view_info { get; }
        private View_weightSwitch view_weightSwitch { get; }
        private View_cubePlayer view_cubePlayer { get; }
        private Button btn_setPackage { get; }
        private IUiManager UiManager { get; }

        public View_packagePlayer(IView v,IUiManager uiManager,Action<(float point,float kg, float length, float width, float height)> onPackageSetAction) : base(v)
        {
            UiManager = uiManager;
            view_sizeSwitch = new View_sizeSwitch(v.GetObject<View>("view_sizeSwitch"),UpdateSize);
            element_input_height = new Element_input(v.GetObject<View>("element_input_height"),UpdateSize);
            element_input_width = new Element_input(v.GetObject<View>("element_input_width"),UpdateSize);
            element_input_length = new Element_input(v.GetObject<View>("element_input_length"),UpdateSize);
            element_input_weight = new Element_input(v.GetObject<View>("element_input_weight"),
                    () => OnWeightValueChanged(element_input_weight.Value));
            view_info = new View_info(v.GetObject<View>("view_info"));
            view_weightSwitch = new View_weightSwitch(v.GetObject<View>("view_weightSwitch"), OnWeightSwitch);
            btn_setPackage = v.GetObject<Button>("btn_setPackage");
            btn_setPackage.OnClickAdd(() => PlayCreationPackage(onPackageSetAction));
            view_cubePlayer = new View_cubePlayer(v.GetObject<View>("view_cubePlayer"));
            OnWeightSwitch();
            ResetValues();
        }

        private void PlayCreationPackage(Action<(float point, float kg, float length, float width, float height)> callBack)
        {
            UiManager.PlayCoroutine(view_cubePlayer.PlayCubeSpin(), true,
                () => callBack((view_info.Point, view_info.Weight, view_info.Length, view_info.Width, view_info.Height)));
        }

        private void ResetValues()
        {
            element_input_height.Reset();
            element_input_width.Reset();
            element_input_length.Reset();
            element_input_weight.Reset();
        }

        private void OnWeightSwitch()
        {
            var label = view_weightSwitch.Current switch
            {
                View_weightSwitch.Weights.Kilogram => "kg",
                View_weightSwitch.Weights.Gram => "g",
                View_weightSwitch.Weights.Pound => "lb",
                _ => throw new ArgumentOutOfRangeException()
            };
            element_input_weight.SetLabel(label);
            OnWeightValueChanged(element_input_weight.Value);
        }

        private void OnWeightValueChanged(float weight)
        {
            var kg = view_weightSwitch.Current switch
            {
                View_weightSwitch.Weights.Kilogram => weight,
                View_weightSwitch.Weights.Gram => weight / 1000f,
                View_weightSwitch.Weights.Pound => weight / OrderController.KgToPounds,
                _ => throw new ArgumentOutOfRangeException()
            };
            view_info.SetKg(kg);
        }

        private void UpdateSize()
        {
            var width = element_input_width.Value;
            var height = element_input_height.Value;
            var length = element_input_length.Value;
            view_info.SetMeter(GetMeter(length),GetMeter(width), GetMeter(height));
            view_cubePlayer.SetObjectSize(width, height, length);

            float GetMeter(float value)
            {
                return view_sizeSwitch.Current switch
                {
                    View_sizeSwitch.Sizes.Meter => value,
                    View_sizeSwitch.Sizes.Feet => value / OrderController.MeterToFeet,
                    View_sizeSwitch.Sizes.Centimeter => value / 100,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }


        private class View_weightSwitch : UiBase
        {
            public enum Weights
            {
                Kilogram,
                Gram,
                Pound
            }

            private Button btn_kg { get; }
            private Button btn_gram { get; }
            private Button btn_pound { get; }
            private int _weightSwitch;
            public Weights Current => (Weights)_weightSwitch;
            public View_weightSwitch(IView v, Action onSwitch,bool display = true) : base(v, display)
            {
                btn_kg = v.GetObject<Button>("btn_kg");
                btn_gram = v.GetObject<Button>("btn_gram");
                btn_pound = v.GetObject<Button>("btn_pound");
                btn_kg.OnClickAdd(() =>
                {
                    WeightSwitch();
                    onSwitch.Invoke();
                });
                btn_gram.OnClickAdd(() =>
                {
                    WeightSwitch();
                    onSwitch.Invoke();
                });
                btn_pound.OnClickAdd(() =>
                {
                    WeightSwitch();
                    onSwitch.Invoke();
                });
                UpdateButtons();
            }

            private void UpdateButtons()
            {
                btn_kg.gameObject.SetActive(Current == Weights.Kilogram);
                btn_gram.gameObject.SetActive(Current == Weights.Gram);
                btn_pound.gameObject.SetActive(Current == Weights.Pound);
            }

            private void WeightSwitch()
            {
                _weightSwitch++;
                if (_weightSwitch > 2)
                    _weightSwitch = 0;
                UpdateButtons();
            }
        }
        private class Element_input:UiBase
        {
            private InputField input_value { get; }
            private Text text_label { get; }
            private Button btn_add { get; }
            private Button btn_sub { get; }
            public float Value => float.TryParse(input_value.text, out var f) ? f : 0;
            public Element_input(IView v,Action onValueChanged, bool display = true) : base(v, display)
            {
                input_value = v.GetObject<InputField>("input_value");
                text_label = v.GetObject<Text>("text_label");
                btn_add = v.GetObject<Button>("btn_add");
                btn_sub = v.GetObject<Button>("btn_sub");
                btn_add.OnClickAdd(() =>
                {
                    AddValue(1);
                    onValueChanged.Invoke();
                });
                btn_sub.OnClickAdd(() =>
                {
                    AddValue(-1);
                    onValueChanged.Invoke();
                });
                input_value.onValueChanged.AddListener(a => onValueChanged());
            }

            private void AddValue(int add)
            {
                if (!float.TryParse(input_value.text, out var value) || value + add <= 0)
                {
                    input_value.text = 1.ToString();
                    return;
                }
                input_value.text = (value + add).ToString();
            }

            public void SetLabel(string label) => text_label.text = label;

            public void Reset() => input_value.text = 1.ToString();
        }
        private class View_info : UiBase
        {
            private Text text_point { get; }
            private Text text_weight { get; }
            private Text text_size { get; }
            public float Size => MathF.Pow(Length * Width * Height, 1f / 3f);
            public float Weight { get; private set; } = 1;
            public float Point { get; private set; } = 1;
            public float Length { get; private set; }
            public float Width { get; private set; }
            public float Height { get; private set; }

            public View_info(IView v, bool display = true) : base(v, display)
            {
                text_point = v.GetObject<Text>("text_point");
                text_weight = v.GetObject<Text>("text_weight");
                text_size = v.GetObject<Text>("text_size");
            }
            public void SetKg(float kg)
            {
                Weight = kg;
                text_weight.text = $"{kg:F} kg";
            }

            public void SetMeter(float length, float width, float height)
            {
                Length = length;
                Width = width;
                Height = height;
                text_size.text = $"{Size:F} m³";
            }

            public void SetPoint(float point)
            {
                Point = point;
                text_point.text = point.ToString("##.##");
            }
        }
        private class View_sizeSwitch : UiBase
        {
            public enum Sizes
            {
                Meter,
                Feet,
                Centimeter
            }
            private int _current;
            private Button btn_meter { get; }
            private Button btn_feet { get; }
            private Button btn_centimeter { get; }
            public Sizes Current => (Sizes)_current;
            public View_sizeSwitch(IView v,Action onSwitch ,bool display = true) : base(v, display)
            {
                btn_meter = v.GetObject<Button>("btn_meter");
                btn_feet = v.GetObject<Button>("btn_feet");
                btn_centimeter = v.GetObject<Button>("btn_centimeter");
                btn_meter.OnClickAdd(() =>
                {
                    SwitchSize();
                    onSwitch.Invoke();
                });
                btn_feet.OnClickAdd(() =>
                {
                    SwitchSize();
                    onSwitch.Invoke();
                });
                btn_centimeter.OnClickAdd(() =>
                {
                    SwitchSize();
                    onSwitch. Invoke();
                });
                UpdateButtons();
            }

            private void SwitchSize()
            {
                _current++;
                if (_current > 2)
                    _current = 0;
                UpdateButtons();
            }

            private void UpdateButtons()
            {
                btn_meter.gameObject.SetActive(Current == Sizes.Meter);
                btn_feet.gameObject.SetActive(Current == Sizes.Feet);
                btn_centimeter.gameObject.SetActive(Current == Sizes.Centimeter);
            }
        }
        private class View_cubePlayer : UiBase
        {
            private GameObject obj_cube { get; }
            private Animation cubeAnim { get; }
            private GameObject obj_particle { get; }
            public View_cubePlayer(IView v) : base(v)
            {
                obj_cube = v.GetObject("obj_cube");
                obj_particle = v.GetObject("obj_particle");
                cubeAnim = obj_cube.GetComponent<Animation>();
            }

            public void SetObjectSize(float width, float height, float length)
            {
                var array = new[] { width, height, length };
                var max = array.Max();
                var final = new float[3];
                for (int i = 0; i < array.Length; i++)
                {
                    var value = array[i];
                    final[i] = value / max * 100;
                }
                obj_cube.transform.localScale = new Vector3(final[0], final[1], final[2]);
            }

            public IEnumerator PlayCubeSpin()
            {
                cubeAnim.Play();
                obj_particle.gameObject.SetActive(true);
                yield return new WaitForSeconds(1);
            }
        }
    }

    private class View_historySect : UiBase
    {
        private ListViewUi<Prefab_history> HistoryView { get; }
        private event Action<string> OnSelectedHistoryAction;
        
        public View_historySect(IView v,Action<string> onSelectedHistoryAction, bool display = true) : base(v, display)
        {
            OnSelectedHistoryAction = onSelectedHistoryAction;
            HistoryView = new ListViewUi<Prefab_history>(v, "prefab_history", "scroll_history");
        }

        public void UpdateHistories(DeliveryOrder[] dos)
        {
            HistoryView.ClearList(ui=>ui.Destroy());
            for (var i = 0; i < dos.Length; i++)
            {
                var o = dos[i];
                var size = MathF.Pow(o.Package.Height * o.Package.Width * o.Package.Length, 1 / 3f);
                var ui = HistoryView.Instance(v => new Prefab_history(v, () => OnSelectedHistoryAction?.Invoke(o.Id)));
                ui.SetInfo(o.Id, state: (DeliveryOrder.States)o.Status, address: o.To.Address,
                    contactName: o.To.Name,
                    contactPhone: o.To.Phone, weight: o.Package.Weight, size: size,
                    point: o.Package.Price, o.Package.Distance);
            }
        }

        private class Prefab_history : UiBase
        {
            private Text text_orderId { get; }
            private Text text_toAddress { get; }
            private View_state view_state { get; }
            private View_contact view_contact { get; }
            private View_parcelInfo view_parcelInfo { get; }
            private Button btn_history { get; }
            public Prefab_history(IView v, Action onClickAction ,bool display = true) : base(v, display)
            {
                text_orderId = v.GetObject<Text>("text_orderId");
                text_toAddress = v.GetObject<Text>("text_toAddress");
                view_state = new View_state(v.GetObject<View>("view_state"));
                view_contact = new View_contact(v.GetObject<View>("view_contact"));
                view_parcelInfo = new View_parcelInfo(v.GetObject<View>("view_parcelInfo"));
                btn_history = v.GetObject<Button>("btn_history");
                btn_history.OnClickAdd(onClickAction);
            }

            public void SetInfo(string orderId,DeliveryOrder.States state, string address, string contactName, string contactPhone, float weight, float size, float point, float distance)
            {
                text_orderId.text = orderId;
                text_toAddress.text = address;
                view_state.SetState(state);
                view_contact.Set(contactName, contactPhone);
                view_parcelInfo.Set(point, weight, size, distance);
            }

            private class View_state : UiBase
            {
                private Image img_waitState { get; }
                private Image img_errState { get; }
                private Image img_completeState { get; }
                private Image img_closeState { get; }
                public View_state(IView v, bool display = true) : base(v, display)
                {
                    img_waitState = v.GetObject<Image>("img_waitState");
                    img_errState = v.GetObject<Image>("img_errState");
                    img_completeState = v.GetObject<Image>("img_completeState");
                    img_closeState = v.GetObject<Image>("img_closeState");
                }

                public void SetState(DeliveryOrder.States state)
                {
                    img_waitState.gameObject.SetActive(state == DeliveryOrder.States.Wait);
                    img_errState.gameObject.SetActive(state == DeliveryOrder.States.Exception);
                    img_completeState.gameObject.SetActive(state == DeliveryOrder.States.Complete);
                    img_closeState.gameObject.SetActive(false);
                }
            }

            private class View_parcelInfo : UiBase
            {
                private Text text_point { get; }
                private Text text_weight { get; }
                private Text text_size { get; }
                private Text text_distance { get; }
                public View_parcelInfo(IView v, bool display = true) : base(v, display)
                {
                    text_point = v.GetObject<Text>("text_point");
                    text_weight = v.GetObject<Text>("text_weight");
                    text_size = v.GetObject<Text>("text_size");
                    text_distance = v.GetObject<Text>("text_distance");
                }

                public void Set(float point, float weight, float size, float distance)
                {
                    text_point.text = point.ToString("##.##");
                    text_weight.text = $"{weight:F}";
                    text_size.text = $"{size:F}";
                    text_distance.text = $"{distance:F}";
                }
            }

            private class View_contact : UiBase
            {
                private Text text_name { get; }
                private Text text_phone { get; }
                public View_contact(IView v, bool display = true) : base(v, display)
                {
                    text_name = v.GetObject<Text>("text_name");
                    text_phone = v.GetObject<Text>("text_phone");
                }
                public void Set(string name, string phone)
                {
                    text_name.text = name;
                    text_phone.text = phone;
                }
            }
        }
    }

}