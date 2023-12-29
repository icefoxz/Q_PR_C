using System;
using System.Collections;
using AOT.Core;
using AOT.Utl;
using OrderHelperLib;
using UnityEngine;
using UnityEngine.Networking;

namespace AOT.Test
{
    public class GeocodingController : ControllerBase
    {
        private string googleGeocodingApi = "https://maps.googleapis.com/maps/api/geocode/json?address={0}&key={1}";
        private string googlePlaceDetailApi = "https://maps.googleapis.com/maps/api/place/details/json?place_id={0}&key={1}";
        private string googleGetAddressApi = "https://maps.googleapis.com/maps/api/geocode/json?latlng={0},{1}&key={2}";
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

        public void GetGeoFromAddress(string address,
            Action<(bool isSuccess, double lat, double lng, string message)> callbackAction) =>
            App.MonoService.StartCoroutine(GetGeocodeData(address, callbackAction));

        public void GetAddressFromCoordinates(double lat, double lng, Action<bool,string> callbackAction)
        {
            App.MonoService.StartCoroutine(FetchAddressFromCoordinates(lat, lng, callbackAction));
        }

        private IEnumerator FetchAddressFromCoordinates(double lat, double lng, Action<bool, string> callbackAction)
        {
            var requestUrl = string.Format(googleGetAddressApi, lat, lng, Auth.GoogleApiKey);
            var webRequest = UnityWebRequest.Get(requestUrl);
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var response = GeocodingResponse.FromJson(webRequest.downloadHandler.text);

                if (response.Status == "OK" && response.Results.Length > 0)
                {
                    // 获取第一个结果的格式化地址
                    var address = response.Results[0].Formatted_Address;
                    callbackAction?.Invoke(true, address);
                }
                else
                {
                    callbackAction?.Invoke(false, "No address found");
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"Error fetching address: {webRequest.error}");
#endif
                callbackAction?.Invoke(false, webRequest.error);
            }
        }


        private IEnumerator GetGeocodeData(string address,
            Action<(bool isSuccess, double lat, double lng, string message)> onResultAction)
        {
            var requestUrl = string.Format(googleGeocodingApi, UnityWebRequest.EscapeURL(address), Auth.GoogleApiKey);
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

        public void GetCoordinatesFromPlaceId(string placeId, Action<(double lat, double lng)> callbackAction)
        {
            App.MonoService.StartCoroutine(FetchCoordinatesFromPlaceId(placeId, callbackAction));
        }

        private IEnumerator FetchCoordinatesFromPlaceId(string placeId, Action<(double lat, double lng)> callbackAction)
        {
            var requestUrl = string.Format(googlePlaceDetailApi, UnityWebRequest.EscapeURL(placeId),
                Auth.GoogleApiKey);
            var webRequest = UnityWebRequest.Get(requestUrl);
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var jsonResponse = GooglePlaceDetail.FromJson(webRequest.downloadHandler.text);
                if (jsonResponse is not { Result: not null }) yield break;
#if UNITY_EDITOR
                Debug.LogWarning($"{nameof(FetchCoordinatesFromPlaceId)}.Geo not found from placeId = {placeId}");
#endif
                callbackAction?.Invoke((jsonResponse.Result.Geometry.Location.Lat,
                    jsonResponse.Result.Geometry.Location.Lng));
            }
            else
            {
#if UNITY_EDITOR
                throw new NotImplementedException($"{nameof(FetchCoordinatesFromPlaceId)}.Exception: {webRequest.result}");
#endif
                callbackAction?.Invoke(default);
            }
        }

        [Serializable]
        public class GooglePlaceDetail
        {
            public PlaceDetailResult Result;

            public static GooglePlaceDetail FromJson(string jsonString)
            {
                var obj = Json.Deserialize<GooglePlaceDetail>(jsonString);
                return obj;
            }
        }

        [Serializable]
        public class PlaceDetailResult
        {
            public Geometry Geometry;
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
            public string Formatted_Address;
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
}
