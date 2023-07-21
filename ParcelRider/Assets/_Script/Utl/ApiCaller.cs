using System;
using System.Net;
using System.Net.Http;
using OrderHelperLib;

namespace Utl
{
    public class ApiCaller
    {
        private string _accessToken;
        private string ServerUrl { get; }
        private static Type StringType { get; }= typeof(string);
        private string AccessToken
        {
            get
            {
                if (string.IsNullOrEmpty(_accessToken))
                    throw new Exception("Access token is null or empty");
                return _accessToken;
            }
        }

        public ApiCaller(string serverUrl)
        {
            ServerUrl = serverUrl;
        }

        public void RegAccessToken(string accessToken) => _accessToken = accessToken;

        public void Call<T>(string method, Action<T> callbackAction,
            Action<string> failedCallbackAction) where T : class => 
            Call(method, null, callbackAction, failedCallbackAction, true, Array.Empty<(string, string)>());

        public void Call<T>(string method, object content, Action<T> callbackAction,
            Action<string> failedCallbackAction) where T : class => 
            Call(method, content, callbackAction, failedCallbackAction, true, Array.Empty<(string, string)>());

        public void CallWithoutToken<T>(string method, object content, Action<T> callbackAction,
            Action<string> failedCallbackAction) where T : class => 
            Call(method, content, callbackAction, failedCallbackAction, false, Array.Empty<(string, string)>());

        public async void RefreshTokenCall<T>(string method, string refreshToken, object content,
            Action<T> callbackAction,
            Action<string> failedCallbackAction) where T : class
        {
            var json = content == null ? string.Empty : Json.Serialize(content);
            var result =
                await Http.SendStringContentAsync(ServerUrl + method, HttpMethod.Post, json, refreshToken);
            if (!result.isSuccess)
                failedCallbackAction?.Invoke(result.content);
            else if (typeof(T) == StringType)
                callbackAction?.Invoke(result.content as T);
            else
                callbackAction?.Invoke(Json.Deserialize<T>(result.content));
        }

        public async void Call<T>(string method, object content,
            Action<T> callbackAction,
            Action<string> failedCallbackAction,
            bool isNeedAccessToken = true,
            params (string, string)[] queryParams) where T : class
        {
            var json = content == null ? string.Empty : Json.Serialize(content);
            var result =
                await Http.SendStringContentAsync(ServerUrl + method, HttpMethod.Post, json,
                    isNeedAccessToken ? AccessToken : null, queryParams);
            if (!result.isSuccess)
                failedCallbackAction?.Invoke(result.content);
            else if (typeof(T) == StringType)
                callbackAction?.Invoke(result.content as T);
            else
                callbackAction?.Invoke(Json.Deserialize<T>(result.content));
        }

        public void CallBag(string method, string dataBag,
            Action<DataBag> callbackAction,
            Action<HttpStatusCode, string> failedCallbackAction,
            bool isNeedAccessToken = true,
            params (string, string)[] queryParams) => CallBag(method, dataBag,
            isNeedAccessToken ? AccessToken : null, callbackAction,
            failedCallbackAction, queryParams);

        public async void CallBag(string method, string dataBag,
            string token,
            Action<DataBag> callbackAction,
            Action<HttpStatusCode,string> failedCallbackAction,
            params (string, string)[] queryParams)
        {
            var (isSuccess, content, httpStatusCode) = await Http.SendStringContentAsync(ServerUrl + method, HttpMethod.Post, dataBag, token, queryParams);
            if (!isSuccess)
                failedCallbackAction?.Invoke(httpStatusCode, content);
            else
            {
                var bag = DataBag.Deserialize(content);
                if (bag == null)
                    failedCallbackAction?.Invoke(httpStatusCode, content);
                else callbackAction?.Invoke(bag);
            }
        }

        public void CallBag(string method, Action<DataBag> callbackAction,
            Action<HttpStatusCode, string> failedCallbackAction) =>
            CallBag(method, string.Empty, callbackAction, failedCallbackAction, true, Array.Empty<(string, string)>());

        public void CallBag(string method, string databag, Action<DataBag> callbackAction,
            Action<HttpStatusCode, string> failedCallbackAction) =>
            CallBag(method, databag, callbackAction, failedCallbackAction, true, Array.Empty<(string, string)>());

        public void CallBagWithoutToken(string method, string databag, Action<DataBag> callbackAction,
            Action<HttpStatusCode, string> failedCallbackAction) =>
            CallBag(method, databag, callbackAction, failedCallbackAction, false, Array.Empty<(string, string)>());
    }
}