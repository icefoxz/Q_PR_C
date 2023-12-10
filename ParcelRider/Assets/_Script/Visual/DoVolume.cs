using System;

namespace Visual
{
    public record DoVolume
    {
        public float Kg { get; set; }
        public float Km { get; set; }
        public float Length { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float GetCost() => GetCost(Kg, Km);
        public static float GetCost(float kg, float km)
        {
            if (kg <= 0 || km <= 0) return 0;

            var min = 5;
            var kgFare = kg switch
            {
                > 15 => 8 + 1.5f * kg,
                >= 5 => 8 + 1 * kg,
                _ => 8
            };
            var kmFare = km switch
            {
                > 10 => 5 + 0.8f * km,
                >= 5 => 5 + 1 * km,
                _ => 5
            };
            var fare = Math.Max(kgFare, kmFare);
            return Math.Max(fare, min);
        }

        public float GetSize() => Length * Width * Height;
    }
}