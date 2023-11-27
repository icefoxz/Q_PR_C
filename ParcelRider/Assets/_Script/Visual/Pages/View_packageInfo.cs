using AOT.BaseUis;
using AOT.Views;
using UnityEngine.UI;

namespace Visual.Pages
{
    public class View_packageInfo : UiBase
    {
        private Text text_cost { get; }
        private Text text_km { get; }
        private Text text_kg { get; }
        private Text text_size { get; }

        public View_packageInfo(IView v) : base(v)
        {
            text_cost = v.Get<Text>("text_cost");
            text_km = v.Get<Text>("text_km");
            text_kg = v.Get<Text>("text_kg");
            text_size = v.Get<Text>("text_size");
        }

        public void Set(float cost, float km, float kg, double size)
        {
            UpdateCost(cost);
            UpdateKm(km);
            UpdateKg(kg);
            UpdateSize(size);
        }

        public void UpdateKg(float kg) => text_kg.text = kg.ToString("F1") + "kg";
        public void UpdateSize(double size) => text_size.text = size.ToString("F2") + "m³";
        public void UpdateCost(float cost) => text_cost.text = cost.ToString("F1");
        public void UpdateKm(float km) => text_km.text = km.ToString("F1") + "km";
    }
}