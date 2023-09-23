using System;
using AOT.Views;
using OrderHelperLib;
using OrderHelperLib.Dtos.DeliveryOrders;
using OrderHelperLib.Req_Models.Users;
using OrderHelperLib.Results;
using UnityEngine;

namespace AOT.Utl
{
    public class ApiPanel : MonoBehaviour
    {
        protected static ApiCaller Caller { get; set; }

        [SerializeField] private Panel _panel;
        protected static Panel Panel { get; set; }

        private const string User_RegisterApi = "Anonymous_RegisterApi";
        private const string User_LoginApi = "Anonymous_LoginApi";
        private const string User_ReloginApi = "User_ReloginApi";
        private const string TestApi = "User_TestApi";
        private const string User_CreateRiderApi = "User_CreateRider";
        private const string User_CreateDeliveryOrderApi = "User_CreateDeliveryOrder";
        private const string User_GetAllDeliveryOrders = "User_GetAllDeliveryOrders";

        private const string Rider_LoginApi = "Anonymous_RiderLoginApi";
        private const string Rider_AssignRiderApi = "Rider_AssignRider";
        private const string Rider_UpdateOrderStatusApi = "Rider_UpdateOrderStatus";

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
            Panel.Show(false, true);
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
            Panel.Show(false, true);
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
            Panel.Show(false, true);
            Caller.CallBag(method: method, databag, result =>
                {
                    Panel.Hide();
                    callbackAction?.Invoke(result);
                }, (code,result) =>
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
            Panel.Show(false, true);
            Caller.CallBag(method: method, databag, result =>
                {
                    Panel.Hide();
                    callbackAction?.Invoke(result);
                },
                (code,result)=>
                {
                    Panel.Hide();
                    failedCallbackAction?.Invoke(result);
                },
                isNeedAccessToken: false, queryParams: queryParams);
        }

        #endregion
        // Register
        public static void User_Register(User_RegDto registerModel, Action<Login_Result> callbackAction,
            Action<string> failedCallbackAction) =>
            CallBagWithoutToken(User_RegisterApi, DataBag.SerializeWithName(nameof(User_RegDto),registerModel), bag =>
            {
                var loginResult = bag.Get<Login_Result>(0);
                Caller.RegAccessToken(loginResult.access_token);
                callbackAction?.Invoke(loginResult);
            }, failedCallbackAction);

        // Login
        public static void User_Login(string username, string password, Action<Login_Result> successCallbackAction, Action<string> failedCallbackAction)
        {
            var content = new User_LoginDto
            {
                Username = username,
                Password = password
            };
            CallBagWithoutToken(User_LoginApi, DataBag.SerializeWithName(nameof(User_LoginDto), content), bag =>
            {
                var obj = bag.Get<Login_Result>(0);
                Caller.RegAccessToken(obj.access_token);
                successCallbackAction?.Invoke(obj);
            }, msg => failedCallbackAction?.Invoke(msg));
        }
        public static void Rider_Login(string username, string password, Action<Login_Result> callbackAction, Action<string> failedCallbackAction)
        {
            var content = new User_LoginDto
            {
                Username = username,
                Password = password
            };
            CallBagWithoutToken(Rider_LoginApi, DataBag.SerializeWithName(nameof(User_LoginDto), content), bag =>
            {
                var obj = bag.Get<Login_Result>(0);
                Caller.RegAccessToken(obj.access_token);
                callbackAction?.Invoke(obj);
            }, msg => failedCallbackAction?.Invoke(msg));
        }

        // Relogin
        public static void User_Relogin(string refreshToken, string username, Action<Login_Result> callbackAction,
            Action<string> failedCallbackAction)
        {
            Caller.CallBag(User_ReloginApi, DataBag.Serialize(username),
                refreshToken, bag =>
                {
                    var obj = bag.Get<Login_Result>(0);
                    Caller.RegAccessToken(obj.access_token);
                    callbackAction?.Invoke(obj);
                }, (code, message) => failedCallbackAction(message));
        }

        // CreateRider
        public static void CreateRider(Action<(bool isSuccess, string arg)> callbackAction) =>
            Call<string>(User_CreateRiderApi, msg => callbackAction?.Invoke((true, msg)),
                msg => callbackAction?.Invoke((false, msg)));

        // CreateDeliveryOrder
        public static void CreateDeliveryOrder(DeliverOrderModel orderDto,
            Action<DeliverOrderModel> successAction,
            Action<string> failedAction)
        {
            CallBag(User_CreateDeliveryOrderApi, DataBag.Serialize(orderDto), bag =>
            {
                var deliveryOrder = bag.Get<DeliverOrderModel>(0);
                successAction?.Invoke(deliveryOrder);
            }, arg => failedAction?.Invoke(arg));
        }

        // AssignRider
        //public static void AssignRider(DeliveryAssignmentDto assignmentDto,
        //    Action<(bool isSuccess, string arg)> callbackAction) => Call<string>(AssignRiderApi,
        //    assignmentDto, msg => callbackAction?.Invoke((true, msg)), msg => callbackAction?.Invoke((false, msg)));
        public static void Rider_AssignRider(int orderId, Action<DeliverOrderModel> successAction, Action<string> failedAction)
        {
            CallBag(Rider_AssignRiderApi, DataBag.Serialize(orderId),
                bag => successAction?.Invoke(bag.Get<DeliverOrderModel>(0)), failedAction);
        }

        public static void RegisterRider(Action<bool> callbackAction)
        {
            CallBag("User_CreateRider", bag =>
            {
                callbackAction?.Invoke(true);
            }, arg => callbackAction?.Invoke(false));
        }

        public static void User_GetDeliveryOrders(int limit,int page,Action<DeliverOrderModel[]> successAction, Action<string> failedAction)
        {
            CallBag(User_GetAllDeliveryOrders, DataBag.Serialize(limit, page), bag =>
            {
                var orders = bag.Get<DeliverOrderModel[]>(0);
                successAction?.Invoke(orders);
            }, failedAction);
        }
        public static void Rider_GetDeliveryOrders(int limit,int page,Action<DeliverOrderModel[]> successAction, Action<string> failedAction)
        {
            CallBag("Rider_GetAllDeliveryOrders", DataBag.Serialize(limit, page), bag =>
            {
                var orders = bag.Get<DeliverOrderModel[]>(0);
                successAction?.Invoke(orders);
            }, failedAction);
        }

        public static void Rider_PickItem(DeliverOrderModel order, Action<DeliverOrderModel> successAction,
            Action<string> failedAction)
        {
            CallBag(Rider_UpdateOrderStatusApi, DataBag.Serialize(order),
                bag => { successAction?.Invoke(bag.Get<DeliverOrderModel>(0)); }, failedAction);
        }
    }
}