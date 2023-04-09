using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Views;

public class GoogleAutocompleteAddress : MonoBehaviour, IController
{
    public View view_autoFill;
    private AutoCompleteSect AutoComplete { get; set; }
    private string googleApiKey = "AIzaSyDtqbwGyN8z9yBJ4DrG7FAvzyE7wCWVQJQ";

    private string googleAutocompleteUrl =
        "https://maps.googleapis.com/maps/api/place/autocomplete/json?input={0}&key={1}&components=country:my";

    void Start()
    {
        AutoComplete = new AutoCompleteSect(view_autoFill, GetAddressSuggestions);
    }

    private float debounceTime = 0.5f;
    private bool isWaitingForDebounce = false;

    public void GetAddressSuggestions(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length < 3) return;
        if (isWaitingForDebounce)
        {
            CancelInvoke(nameof(DebouncedGetAddressSuggestions));
        }

        isWaitingForDebounce = true;
        Invoke(nameof(DebouncedGetAddressSuggestions), debounceTime);
    }

    private void DebouncedGetAddressSuggestions()
    {
        isWaitingForDebounce = false;
        StartCoroutine(FetchAddressSuggestions(AutoComplete.Input));
    }

    private IEnumerator FetchAddressSuggestions(string input)
    {
        var requestUrl = string.Format(googleAutocompleteUrl, UnityWebRequest.EscapeURL(input), googleApiKey);
        var webRequest = UnityWebRequest.Get(requestUrl);
        yield return webRequest.SendWebRequest();

        var resultText = string.Empty;
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            var jsonResponse = GoogleAddressAutocompleteResponse.FromJson(webRequest.downloadHandler.text);
            if (jsonResponse is not { Predictions: not null }) yield break;
            foreach (var prediction in jsonResponse.Predictions)
            {
                resultText += prediction + "\n";
            }
        }
        else
        {
            resultText = "Error fetching address suggestions: " + webRequest.error;
        }

        AutoComplete.Set(resultText.Split('\n'));
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

    public class AutoCompleteSect : UiBase
    {
        private InputField input_address { get; }
        private ListViewUi<Prefab_Address> AddressListView { get; }
        public string Input => input_address.text;
        private string SelectedAddress { get; set; }

        public AutoCompleteSect(IView v, Action<string> onAddressInputAction) : base(v)
        {
            input_address = v.GetObject<InputField>("input_address");
            AddressListView = new ListViewUi<Prefab_Address>(v, "prefab_address", "scroll_address");
            input_address.onValueChanged.AddListener(input =>
            {
                if (SelectedAddress == input) return;
                onAddressInputAction(input);
            });
        }


        public void Set(ICollection<string> addresses)
        {
            AddressListView.ScrollRect.gameObject.SetActive(true);
            ClearList();
            foreach (var address in addresses)
            {
                var ui = AddressListView.Instance(v => new Prefab_Address(v, () => OnAddressSelected(address)));
                ui.Set(address);
            }
        }

        private void ClearList() => AddressListView.ClearList(p => p.Destroy());

        private void OnAddressSelected(string address)
        {
            SelectedAddress = address;
            input_address.text = address;
            AddressListView.ScrollRect.gameObject.SetActive(false);
            ClearList();
        }

        private class Prefab_Address : UiBase
        {
            private Text text_address { get; }
            private Button btn_address { get; }

            public Prefab_Address(IView v, Action onclickAction) : base(v)
            {
                text_address = v.GetObject<Text>("text_address");
                btn_address = v.GetObject<Button>("btn_address");
                btn_address.OnClickAdd(onclickAction);
            }

            public void Set(string address) => text_address.text = address;
        }
    }
}
