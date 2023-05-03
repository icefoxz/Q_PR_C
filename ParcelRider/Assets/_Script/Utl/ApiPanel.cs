using OrderHelperLib.DtoModels.Users;
using System;
using DataModel;
using OrderHelperLib;
using OrderHelperLib.DtoModels.DeliveryOrders;
using UnityEngine;

namespace Utl
{
    public class ApiPanel : MonoBehaviour
    {
        public static ApiCaller Caller { get; private set; }

        [SerializeField] private Panel _panel;
        private static Panel Panel { get; set; }

        private const string RegisterApi = "Anonymous_RegisterApi";
        private const string LoginApi = "Anonymous_LoginApi";
        private const string ReloginApi = "User_ReloginApi";
        private const string TestApi = "User_TestApi";
        private const string CreateDeliveryManApi = "User_CreateDeliveryMan";
        private const string CreateDeliveryOrderApi = "User_CreateDeliveryOrder";
        private const string AssignDeliveryManApi = "DeliveryMan_AssignDeliveryMan";
        private const string UpdateOrderStatusApi = "DeliveryMan_UpdateOrderStatus";

        public void Init(string serverUrl)
        {
            Panel = _panel;
            Caller = new ApiCaller(serverUrl);
        }

        #region Calls

        private static void Call<T>(string method, Action<T> callbackAction,
            Action<string> failedCallbackAction)
            where T : class => Call(method, null, callbackAction,failedCallbackAction);

        private static void Call<T>(string method, object content, Action<T> callbackAction,
            Action<string> failedCallbackAction) 
            where T : class => Call(method, content, callbackAction,failedCallbackAction ,Array.Empty<(string, string)>());

        private static void Call<T>(string method, object content, Action<T> callbackAction,
            Action<string> failedCallbackAction, params (string, string)[] queryParams) where T : class
        {
            Panel.Show(true, true);
            Caller.Call<T>(method: method, content: content, result =>
                {
                    Panel.Hide();
                    callbackAction?.Invoke(result);
                }, result =>
                {
                    Panel.Hide();
                    failedCallbackAction?.Invoke(result);
                }, isNeedAccessToken: true,
                queryParams: queryParams);
        }

        private static void CallWithoutToken<T>(string method, object content,
            Action<T> callbackAction, Action<string> failedCallbackAction,
            params (string, string)[] queryParams)
            where T : class
        {
            Panel.Show(true, true);
            Caller.Call<T>(method: method, content: content, result =>
                {
                    Panel.Hide();
                    callbackAction?.Invoke(result);
                },
                result =>
                {
                    Panel.Hide();
                    failedCallbackAction?.Invoke(result);
                },
                isNeedAccessToken: false, queryParams: queryParams);
        }

        private static void CallBag(string method, Action<DataBag> callbackAction,
            Action<string> failedCallbackAction) => CallBag(method, string.Empty, callbackAction, failedCallbackAction);

        private static void CallBag(string method, string databag, Action<DataBag> callbackAction,
            Action<string> failedCallbackAction) => CallBag(method, databag, callbackAction, failedCallbackAction,
            Array.Empty<(string, string)>());

        private static void CallBag(string method, string databag, Action<DataBag> callbackAction,
            Action<string> failedCallbackAction, params (string, string)[] queryParams) 
        {
            Panel.Show(true, true);
            Caller.CallBag(method: method, databag, result =>
                {
                    Panel.Hide();
                    callbackAction?.Invoke(result);
                }, result =>
                {
                    Panel.Hide();
                    failedCallbackAction?.Invoke(result);
                }, isNeedAccessToken: true,
                queryParams: queryParams);
        }

        private static void CallBagWithoutToken(string method, string databag,
            Action<DataBag> callbackAction, Action<string> failedCallbackAction,
            params (string, string)[] queryParams)
        {
            Panel.Show(true, true);
            Caller.CallBag(method: method, databag, result =>
                {
                    Panel.Hide();
                    callbackAction?.Invoke(result);
                },
                result =>
                {
                    Panel.Hide();
                    failedCallbackAction?.Invoke(result);
                },
                isNeedAccessToken: false, queryParams: queryParams);
        }

        #endregion
        // Register
        public static void Register(RegisterDto registerModel, Action<LoginResult> callbackAction,
            Action<string> failedCallbackAction) =>
            CallBagWithoutToken(RegisterApi, DataBag.SerializeWithName(nameof(RegisterDto),registerModel), bag =>
            {
                var loginResult = bag.Get<LoginResult>(0);
                Caller.RegAccessToken(loginResult.access_token);
                callbackAction?.Invoke(loginResult);
            }, failedCallbackAction);

        // Login
        public static void Login(string username, string password, Action<LoginResult> callbackAction, Action<string> failedCallbackAction)
        {
            var content = new LoginDto
            {
                Username = username,
                Password = password
            };
            CallBagWithoutToken(LoginApi, DataBag.SerializeWithName(nameof(LoginDto), content), bag =>
            {
                var obj = bag.Get<LoginResult>(0);
                Caller.RegAccessToken(obj.access_token);
                callbackAction?.Invoke(obj);
            }, msg => failedCallbackAction?.Invoke(msg));
        }


        // Relogin
        public static void Relogin(string refreshToken, string username, Action<LoginResult> callbackAction,
            Action<string> failedCallbackAction)
        {
            Caller.CallBagWithToken(ReloginApi, DataBag.Serialize(username),
                refreshToken, bag =>
                {
                    var obj = bag.Get<LoginResult>(0);
                    Caller.RegAccessToken(obj.access_token);
                    callbackAction?.Invoke(obj);
                }, failedCallbackAction);
        }

        // CreateDeliveryMan
        public static void CreateDeliveryMan(Action<(bool isSuccess, string arg)> callbackAction) =>
            Call<string>(CreateDeliveryManApi, msg => callbackAction?.Invoke((true, msg)),
                msg => callbackAction?.Invoke((false, msg)));

        // CreateDeliveryOrder
        public static void CreateDeliveryOrder(DeliveryOrderDto orderDto,
            Action<DeliveryOrderDto> successAction,
            Action<string> failedAction)
        {
            CallBag(CreateDeliveryOrderApi, DataBag.Serialize(orderDto), bag =>
            {
                var deliveryOrder = bag.Get<DeliveryOrderDto>(0);
                successAction?.Invoke(deliveryOrder);
            }, arg => failedAction?.Invoke(arg));
        }

        // AssignDeliveryMan
        public static void AssignDeliveryMan(DeliveryAssignmentDto assignmentDto,
            Action<(bool isSuccess, string arg)> callbackAction) => Call<string>(AssignDeliveryManApi,
            assignmentDto, msg => callbackAction?.Invoke((true, msg)), msg => callbackAction?.Invoke((false, msg)));

        // UpdateOrderStatus
        public static void UpdateOrderStatus(DeliverySetStatusDto setStatusDto,
            Action<(bool isSuccess, string arg)> callbackAction) => Call<string>(UpdateOrderStatusApi,
            setStatusDto, msg => callbackAction?.Invoke((true, msg)), msg => callbackAction?.Invoke((false, msg)));
    }
}