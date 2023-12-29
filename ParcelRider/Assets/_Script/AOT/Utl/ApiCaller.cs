using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AOT.Core;
using AOT.Network;
using AOT.Views;
using OrderHelperLib;
using UnityEngine;

namespace AOT.Utl
{
    public class ApiCaller
    {
        private static string _accessToken;
        private string ServerUrl { get; }
        public static string AccessToken
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

        public void RefreshTokenCall(string method, string refreshToken, string stringContent,
            Action<DataBag> callbackAction,
            Action<string> failedCallbackAction)
        {
            var requestUri = Http.GetUri(ServerUrl, method);
            CallWAwaitRetry(() => Http.SendStringContentAsync(requestUri, HttpMethod.Post, stringContent, refreshToken), 
                callbackAction, err => failedCallbackAction($"{requestUri} --- {err}"), "Login error.");
        }

        private void CallWAwaitRetry(Func<Task<(bool isSuccess, string content, HttpStatusCode code)>> func,
            Action<DataBag> callbackAction,
            Action<string> failedCallbackAction, string customServerErrMsg = null)
        {
            RetryCaller.Start(func,
                result => ResolveResult(callbackAction, failedCallbackAction, result),
                onErrRetry: a =>
                {
                    // 这个是http请求失败
                    var err = customServerErrMsg ?? a.err;
                    var (isSuccess,content,code)  = a.arg;
#if UNITY_EDITOR
                    Debug.LogError(
                        $"<color=red>Server response error! {code} code = {(int)code}\n{content}</color>");
#endif
                    return ReqRetryWindow.Await(err);
                });
            return;

            void ResolveResult(Action<DataBag> callback, Action<string> failedCallback,
                (bool isSuccess, string content, HttpStatusCode code) result)
            {
                if (!result.isSuccess)
                {
                    RunInMainThread(() => failedCallback?.Invoke(result.content));
                }
                else
                    RunInMainThread(() =>
                    {
                        var bag = DataBag.Deserialize(result.content);
                        // 这个是请求成功，但是返回的是string类型(失败)
                        if (bag == null)
                        {
                            failedCallback?.Invoke(result.content);
                            return;
                        }

                        callback?.Invoke(bag);
                    });

                //主线程调用
                void RunInMainThread(Action action) => App.MainThread.Enqueue(action);
            }
        }

        //public async void Call<T>(string method, object content,
        //    Action<T> callbackAction,
        //    Action<string> failedCallbackAction,
        //    bool isNeedAccessToken = true,
        //    params (string, string)[] queryParams) where T : class
        //{
        //    var json = content == null ? string.Empty : Json.Serialize(content);
        //    var result =
        //        await Http.SendStringContentAsync(ServerUrl + method, HttpMethod.Post, json,
        //            isNeedAccessToken ? AccessToken : null, queryParams);
        //    ResolveResult(callbackAction, failedCallbackAction, result);
        //}


        public void CallBag(string method, string dataBag,
            Action<DataBag> callbackAction,
            Action<string> failedCallbackAction,
            bool isNeedAccessToken = true,
            params (string, string)[] queryParams) => CallBag(method, dataBag,
            isNeedAccessToken ? AccessToken : null, callbackAction,
            failedCallbackAction, queryParams);

        public void CallBag(string method, string dataBag,
            string token,
            Action<DataBag> callbackAction,
            Action<string> failedCallbackAction,
            params (string, string)[] queryParams)
        {
            CallWAwaitRetry(
                () => Http.SendStringContentAsync(Http.GetUri(ServerUrl, method, queryParams), HttpMethod.Post, dataBag, token),
                callbackAction, failedCallbackAction, "Request error.");
        }

        public void CallBag(string method, Action<DataBag> callbackAction,
            Action<string> failedCallbackAction) =>
            CallBag(method, string.Empty, callbackAction, failedCallbackAction, true, Array.Empty<(string, string)>());

        public void CallBag(string method, string databag, Action<DataBag> callbackAction,
            Action<string> failedCallbackAction) =>
            CallBag(method, databag, callbackAction, failedCallbackAction, true, Array.Empty<(string, string)>());

        public void CallBagWithoutToken(string method, string databag, Action<DataBag> callbackAction,
            Action<string> failedCallbackAction) =>
            CallBag(method, databag, callbackAction, failedCallbackAction, false, Array.Empty<(string, string)>());
    }
}