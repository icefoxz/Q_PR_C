using System;
using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.Networking;
using Utls;

public class GeocodingController : IController
{
    private string geocodingUrl = "https://maps.googleapis.com/maps/api/geocode/json?address={0}&key={1}";
    private Debouncer FromDebouncer { get; }
    private Debouncer ToDebouncer { get; }
    public GeocodingController()
    {
        FromDebouncer = new Debouncer(1f, App.MonoService);
        ToDebouncer = new Debouncer(1f, App.MonoService);
    }
    public void GetGeocodeFrom(string address,
        Action<(bool isSuccess, double lat, double lng, string message)> onResultAction)
    {
        FromDebouncer.Debounce(address, debouncedInput => GetGeocodeData(debouncedInput, onResultAction));
    }
    public void GetGeocodeTo(string address,
        Action<(bool isSuccess, double lat, double lng, string message)> onResultAction)
    {
        ToDebouncer.Debounce(address, debouncedInput => GetGeocodeData(debouncedInput, onResultAction));
    }

    private IEnumerator GetGeocodeData(string address,
        Action<(bool isSuccess, double lat, double lng, string message)> onResultAction)
    {
        var requestUrl = string.Format(geocodingUrl, UnityWebRequest.EscapeURL(address), ApiKey.GoogleApiKey);
        var webRequest = UnityWebRequest.Get(requestUrl);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            var response = GeocodingResponse.FromJson(webRequest.downloadHandler.text);

            if (response.Status == "OK")
            {
                // Get the first result (most relevant one)
                var result = response.Results[0];
                var latLng = new Vector2((float)result.Geometry.Location.Lat, (float)result.Geometry.Location.Lng);
                onResultAction.Invoke((true, latLng.x, latLng.y, string.Empty));
            }
            else
                onResultAction.Invoke((false, 0, 0, response.Status));
        }
        else
        {
            onResultAction.Invoke((false, 0, 0, webRequest.error));
        }
    }

    [Serializable]
    public class GeocodingResponse
    {
        public string Status;
        public GeocodingResult[] Results;

        public static GeocodingResponse FromJson(string jsonString)
        {
            return Json.Deserialize<GeocodingResponse>(jsonString);
        }
    }

    [Serializable]
    public class GeocodingResult
    {
        public string FormattedAddress;
        public Geometry Geometry;
    }

    [Serializable]
    public class Geometry
    {
        public Location Location;
    }

    [Serializable]
    public class Location
    {
        public double Lat;
        public double Lng;
    }
}
