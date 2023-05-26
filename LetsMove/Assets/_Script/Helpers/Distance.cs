using System;

namespace Utls
{
    public class Distance
    {
        public static double CalculateDistance(double startLat, double startLon, double endLat, double endLon, DistanceUnit unit = DistanceUnit.Kilometers)
        {
            var dLat = (endLat - startLat) * Math.PI / 180.0;
            var dLon = (endLon - startLon) * Math.PI / 180.0;
            var lat1 = startLat * Math.PI / 180.0;
            var lat2 = endLat * Math.PI / 180.0;

            var a = Math.Pow(Math.Sin(dLat / 2), 2) + Math.Pow(Math.Sin(dLon / 2), 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = 6371 * c; // Radius of Earth in kilometers

            if (unit == DistanceUnit.Miles)
            {
                distance /= 1.609344;
            }

            return distance;
        }

        public enum DistanceUnit
        {
            Kilometers,
            Miles
        }
    }
}