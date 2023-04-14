using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Linq;
using Core;
using Utls;
using Newtonsoft.Json;

public class AutofillAddressController : IController
{
    private MonoService Mono => App.MonoService;
    private string googleAutocompleteUrl =
        "https://maps.googleapis.com/maps/api/place/autocomplete/json?input={0}&key={1}&components=country:my";

    private float debounceTime = 0.5f;
    private Debouncer Debouncer { get; } 

    public AutofillAddressController()
    {
        Debouncer = new Debouncer(debounceTime, App.MonoService);
    }

    public void GetAddressSuggestions(string input, Action<(string placeId, string address)[]> onRequestResult)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length < 3) return;
        Debouncer.Debounce(input, debouncedInput => FetchAddressSuggestions(debouncedInput, onRequestResult));
    }

    private IEnumerator FetchAddressSuggestions(string input,
        Action<(string placeId, string address)[]> onRequestResult)
    {
        var requestUrl = string.Format(googleAutocompleteUrl, UnityWebRequest.EscapeURL(input), ApiKey.GoogleApiKey);
        var webRequest = UnityWebRequest.Get(requestUrl);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success) {
            var jsonResponse = GoogleAddressAutocompleteResponse.FromJson(webRequest.downloadHandler.text);
            if (jsonResponse is not { GetPredictions: not null }) yield break;
            onRequestResult?.Invoke(jsonResponse.GetPredictions);
        }
        else
        {
            onRequestResult?.Invoke(null);
        }
    }


    [Serializable]
    public class GoogleAddressAutocompleteResponse
    {
        [SerializeField] public Prediction[] predictions;

        public (string placeId, string address)[] GetPredictions =>
            predictions?.Select(p => (p.place_id, p.description)).ToArray()?? Array.Empty<(string placeId, string address)>();

        public static GoogleAddressAutocompleteResponse FromJson(string jsonString)
        {
            var obj = Json.Deserialize<GoogleAddressAutocompleteResponse>(jsonString);
            return obj;
        }

        [Serializable]
        public class Prediction
        {
            public string description;
            public string place_id;
        }
    }
}