using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Linq;
using Core;
using Views;
using Visual.Sects;

public class GoogleAutocompleteAddress : IController
{
    private string googleApiKey = "";
    private MonoService Mono => App.MonoService;
    private string googleAutocompleteUrl =
        "https://maps.googleapis.com/maps/api/place/autocomplete/json?input={0}&key={1}&components=country:my";

    private float debounceTime = 0.5f;
    private bool isWaitingForDebounce = false;

    public void GetAddressSuggestions(string input, Action<string[]> onRequestResult)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length < 3) return;
        if (isWaitingForDebounce)
        {
            Mono.CancelInvoke(nameof(DebouncedGetAddressSuggestions));
        }

        isWaitingForDebounce = true;
        Mono.Invoke(nameof(DebouncedGetAddressSuggestions), debounceTime);

        void DebouncedGetAddressSuggestions()
        {
            isWaitingForDebounce = false;
            Mono.StartCoroutine(FetchAddressSuggestions());
        }

        IEnumerator FetchAddressSuggestions()
        {
            var requestUrl = string.Format(googleAutocompleteUrl, UnityWebRequest.EscapeURL(input), googleApiKey);
            var webRequest = UnityWebRequest.Get(requestUrl);
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var jsonResponse = GoogleAddressAutocompleteResponse.FromJson(webRequest.downloadHandler.text);
                if (jsonResponse is not { Predictions: not null }) yield break;
                onRequestResult?.Invoke(jsonResponse.Predictions);
            }
            else
            {
                onRequestResult?.Invoke(null);
            }
        }
    }

    [Serializable]
    public class GoogleAddressAutocompleteResponse
    {
        [SerializeField] private Prediction[] predictions;
        public string[] Predictions => predictions.Select(p => p.description).ToArray();

        public static GoogleAddressAutocompleteResponse FromJson(string jsonString) =>
            JsonUtility.FromJson<GoogleAddressAutocompleteResponse>(jsonString);

        [Serializable]
        private class Prediction
        {
            public string description;
        }

    }

}