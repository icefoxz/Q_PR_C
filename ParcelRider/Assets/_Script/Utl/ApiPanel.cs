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
        private const string CreateRiderApi = "User_CreateRider";
        private const string CreateDeliveryOrderApi = "User_CreateDeliveryOrder";
        private const string AssignRiderApi = "Rider_AssignRider";
        private const string UpdateOrderStatusApi = "Rider_UpdateOrderStatus";

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

        // CreateRider
        public static void CreateRider(Action<(bool isSuccess, string arg)> callbackAction) =>
            Call<string>(CreateRiderApi, msg => callbackAction?.Invoke((true, msg)),
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

        // AssignRider
        public static void AssignRider(DeliveryAssignmentDto assignmentDto,
            Action<(bool isSuccess, string arg)> callbackAction) => Call<string>(AssignRiderApi,
            assignmentDto, msg => callbackAction?.Invoke((true, msg)), msg => callbackAction?.Invoke((false, msg)));

        // UpdateOrderStatus
        public static void UpdateOrderStatus(DeliverySetStatusDto setStatusDto,
            Action<(bool isSuccess, string arg)> callbackAction) => Call<string>(UpdateOrderStatusApi,
            setStatusDto, msg => callbackAction?.Invoke((true, msg)), msg => callbackAction?.Invoke((false, msg)));

        public static void RegisterRider(Action<bool> callbackAction)
        {
            CallBag("User_CreateRider", bag =>
            {
                callbackAction?.Invoke(true);
            }, arg => callbackAction?.Invoke(false));
        }

        public static void GetDeliveryOrders(int limit,int page,Action<DeliveryOrderDto[]> successAction, Action<string> failedAction)
        {
            CallBag("User_GetAllDeliveryOrders", DataBag.Serialize(limit, page), bag =>
            {
                var orders = bag.Get<DeliveryOrderDto[]>(0);
                successAction?.Invoke(orders);
            }, failedAction);
        }

        public static void AssignRider(int orderId, Action<DeliveryOrderDto> successAction, Action<string> failedAction)
        {
            CallBag("Rider_AssignRider", DataBag.Serialize(orderId),
                bag => successAction?.Invoke(bag.Get<DeliveryOrderDto>(0)), failedAction);
        }

        public static void RiderLogin(string username, string password, Action<RiderDto> successAction, Action<string> failedAction)
        {
            CallBag("Anonymous_RiderLogin", DataBag.Serialize(username, password), bag =>
            {
                var rider = bag.Get<RiderDto>(0);
                if (rider == null)
                {
                    failedAction?.Invoke(bag.ToString());
                    return;
                }
                Caller.RegAccessToken(bag.Get<string>(1));
                successAction?.Invoke(rider);
            }, failedAction);
        }
    }
}