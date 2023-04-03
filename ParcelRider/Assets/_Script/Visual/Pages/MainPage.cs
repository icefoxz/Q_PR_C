using DataModel;
using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Views;

public class MainPage : PageUiBase
{
    private ListViewUi<Prefab_Order> OrderListView { get; }
    private View_packagePlayer view_packagePlayer { get; }

    public MainPage(IView v, UiManager uiManager) : base(v, uiManager)
    {
        OrderListView = new ListViewUi<Prefab_Order>(v, "prefab_order", "scroll_orders");
        view_packagePlayer = new View_packagePlayer(v.GetObject<View>("view_packagePlayer"),
            uiManager, a => UiManager.SetPackageConfirm(a.point, a.kg, a.volume));
    }

    public void SetOrders(DeliveryOrder[] orders)
    {
        OrderListView.ClearList(ui => ui.Destroy());
        foreach (var o in orders)
        {
            var ui = OrderListView.Instance(v => new Prefab_Order(v, id => UiManager.ViewOrder(id)));
            ui.Set(o);
        }
    }

    private class Prefab_Order : UiBase
    {
        private enum States
        {
            WaitToPick,
            Delivering,
            Drop,
            Completed,
            Err,
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
            Text_from.text = ConvertText("from: ", deliveryOrder.StartPoint, 15);
            Text_to.text = ConvertText("to: ", deliveryOrder.EndPoint, 15);
            Text_cost.text = deliveryOrder.Price.ToString("F");
            Text_km.text = deliveryOrder.Distance.ToString();
        }

        private string ConvertText(string prefix, string text, int maxChars)
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

    private class View_packagePlayer : UiBase
    {
        public const float KgToPounds = 2.2046226218f;
        public const float MeterToFeet = 3.280839895f;
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

        public View_packagePlayer(IView v,IUiManager uiManager,Action<(float point,float kg, float volume)> onPackageSetAction) : base(v)
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
            ResetValues();
        }

        private void PlayCreationPackage(Action<(float point, float kg, float volume)> callBack)
        {
            UiManager.PlayCoroutine(view_cubePlayer.PlayCubeSpin(), true,
                () => callBack((view_info.Point, view_info.Weight, view_info.Size)));
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
                View_weightSwitch.Weights.Pound => weight / KgToPounds,
                _ => throw new ArgumentOutOfRangeException()
            };
            view_info.SetKg(kg);
        }

        private void UpdateSize()
        {
            var value = CountSize(element_input_width.Value, element_input_height.Value, element_input_length.Value);
            var size = view_sizeSwitch.Current switch
            {
                View_sizeSwitch.Sizes.Meter => value * 100,
                View_sizeSwitch.Sizes.Feet => value / MeterToFeet * 100,
                View_sizeSwitch.Sizes.Centimeter => value,
                _ => throw new ArgumentOutOfRangeException()
            };
            view_info.SetCentimeter(size);
        }

        private float CountSize(float width, float height, float length) => 
            MathF.Pow(width * height * length, 1 / 3f);

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
            public float Size { get; private set; } = 1;
            public float Weight { get; private set; } = 1;
            public float Point { get; private set; } = 1;
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

            public void SetCentimeter(float cm)
            {
                Size = cm;
                text_size.text = $"{cm:F} cm³";
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
                obj_cube.transform.localScale.Set(final[0], final[1], final[2]);
            }

            public IEnumerator PlayCubeSpin()
            {
                cubeAnim.Play();
                obj_particle.gameObject.SetActive(true);
                yield return new WaitForSeconds(1);
            }
        }
    }
}