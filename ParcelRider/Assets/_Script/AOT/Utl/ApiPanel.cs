using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using AOT.Core;
using AOT.Network;
using AOT.Views;
using OrderHelperLib.Dtos;
using OrderHelperLib.Dtos.DeliveryOrders;
using OrderHelperLib.Req_Models.Users;
using OrderHelperLib.Results;
using UnityEngine;
using Color = System.Drawing.Color;

namespace AOT.Utl
{
    public class ApiPanel : MonoBehaviour
    {
        protected static ApiCaller Caller { get; set; }

        [SerializeField] private Panel _panel;
        private static Panel Panel { get; set; }

        private const string User_RegisterApi = "Anonymous_User_Register";
        private const string User_LoginApi = "Anonymous_Login_User";
        private const string User_ReloginApi = "User_ReloginApi";

        private const string User_Get_Active = "User_Get_Active";
        private const string User_Get_Histories = "User_Get_Histories";
        private const string User_Get_SubStates = "User_Get_SubStates";

        private const string User_Do_Cancel = "User_Do_Cancel";
        private const string User_Do_Create = "User_Do_Create";

        private const string Rider_LoginApi = "Anonymous_Login_Rider";
        private const string Rider_Do_Assign = "Rider_Do_Assign";
        private const string Rider_Do_StateUpdate = "Rider_Do_StateUpdate";
        private const string Rider_Do_Cancel = "Rider_Do_Cancel";
        private const string Rider_Get_Unassigned = "Rider_Get_Unassigned";
        private const string Rider_Get_Assigned = "Rider_Get_Assigned";
        private const string Rider_Get_SubStates = "Rider_Get_SubStates";

        private const string Rider_Get_Histories = "Rider_Get_Histories";

        //test
        private const string TestApi = "User_TestApi";
        private const string User_CreateRiderApi = "User_CreateRider";

        public void Init(string serverUrl)
        {
            Panel = _panel;
            Caller = new ApiCaller(serverUrl);
        }

        #region Calls

        //private static void CallWithoutToken<T>(string method, object content,
        //    Action<T> callbackAction, Action<string> failedCallbackAction,
        //    params (string, string)[] queryParams)
        //    where T : class
        //{
        //    Panel.StartCall(method, false, true);
        //    Caller.Call<T>(method: method, content: content, result =>
        //        {
        //            Panel.EndCall(method);
        //            callbackAction?.Invoke(result);
        //        },
        //        result =>
        //        {
        //            Panel.EndCall(method);
        //            failedCallbackAction?.Invoke(result);
        //        },
        //        isNeedAccessToken: false, queryParams: queryParams);
        //}

        //private static void CallBag(string method, Action<DataBag> callbackAction,
        //    Action<string> failedCallbackAction) =>
        //    CallBag(method, DataBag.Serialize(), callbackAction, failedCallbackAction);

        //private static void CallBag(string method, string databag, Action<DataBag> callbackAction,
        //    Action<string> failedCallbackAction) => CallBag(method, databag, callbackAction, failedCallbackAction,
        //    Array.Empty<(string, string)>());

        //private static void CallBag(string method, string databag, Action<DataBag> callbackAction,
        //    Action<string> failedCallbackAction, params (string, string)[] queryParams)
        //{
        //    Panel.StartCall(method, false, true);
        //    Caller.CallBag(method: method, databag, result =>
        //        {
        //            Panel.EndCall(method);
        //            callbackAction?.Invoke(result);
        //        }, message =>
        //        {
        //            Panel.EndCall(method);
        //            failedCallbackAction?.Invoke(message);
        //        }, isNeedAccessToken: true,
        //        queryParams: queryParams);
        //}

        private static async Task<string> CallWithPanelAsync(string method, string databag,bool callWithToken ,params (string, string)[] queries)
        {
            Panel.StartCall(method, false, true);
            var result = await Caller.CallStringAsync(method, databag, callWithToken, queries);
            Panel.EndCall(method);
            return result;
        }

        private static void CallBagWithRetry(string method, string databag, Action<DataBag> callbackAction,
            Action<string> failedCallbackAction, params (string, string)[] queryParams) =>
            CallBagWithRetryCustomMessage(method, databag, callbackAction, failedCallbackAction, null, queryParams);

        private static void CallBagWithRetryCustomMessage(string method, string databag, Action<DataBag> callbackAction,
            Action<string> failedCallbackAction, string? customServerErrMsg,params (string, string)[] queryParams)
        {
            RetryCaller.Start(async ()=> await CallWithPanelAsync(method, databag, true, queryParams),
                result => ResolveResult(callbackAction, failedCallbackAction, result),
                onErrRetry: a =>
                {
                    // 这个是http请求失败
                    var err = customServerErrMsg ?? a.err;
#if UNITY_EDITOR
                    Debug.LogError($"Server response error! {err}".Color(Color.Red));
#endif
                    return ReqRetryWindow.Await(err);
                });
            return;

            void ResolveResult(Action<DataBag> callback, Action<string> failedCallback,string result)
            {
                var bag = DataBag.Deserialize(result);
                if (bag == null)
                    RunInMainThread(() => failedCallback?.Invoke(result));
                else
                    RunInMainThread(() => callback?.Invoke(bag));
                return;

                //主线程调用
            }
        }

        static void RunInMainThread(Action action) => App.MainThread.Enqueue(action);

        private static void CallBagWithoutToken(string method, string databag,
            Action<DataBag> callbackAction, Action<string> failedCallbackAction,
            params (string, string)[] queryParams)
        {
            Panel.StartCall(method, false, true);
            //var result = await Caller.CallBag()
            Caller.CallBag(method: method, databag, result =>
                {
                    Panel.EndCall(method);
                    callbackAction?.Invoke(result);
                },
                message =>
                {
                    Panel.EndCall(method);
                    failedCallbackAction?.Invoke(message);
                },
                isNeedAccessToken: false, queryParams: queryParams);
        }

        #endregion

        // Register
        public static void User_Register(User_RegDto registerModel, Action<Login_Result> callbackAction,
            Action<string> failedCallbackAction) =>
            CallBagWithoutToken(User_RegisterApi, DataBag.SerializeWithName(nameof(User_RegDto), registerModel), bag =>
            {
                var loginResult = bag.Get<Login_Result>(0);
                App.SetServerUrls(loginResult.signalRUrl,loginResult.imageServerUrl);
                Caller.RegAccessToken(loginResult.access_token);
                callbackAction?.Invoke(loginResult);
            }, failedCallbackAction);

        // Login
        public static void User_Login(string username, string password, Action<Login_Result> successCallbackAction,
            Action<string> failedCallbackAction)
        {
            var content = new User_LoginDto
            {
                Username = username,
                Password = password
            };
            CallBagWithoutToken(User_LoginApi, DataBag.SerializeWithName(nameof(User_LoginDto), content), bag =>
            {
                var obj = bag.Get<Login_Result>(0);
                App.SetServerUrls(obj.signalRUrl, obj.imageServerUrl);
                Caller.RegAccessToken(obj.access_token);
                successCallbackAction?.Invoke(obj);
            }, msg => failedCallbackAction?.Invoke(msg));
        }

        public static void Rider_Login(string username, string password, Action<Login_Result> callbackAction,
            Action<string> failedCallbackAction)
        {
            var content = new User_LoginDto
            {
                Username = username,
                Password = password
            };
            var b = new DataBag
            {
                Data = new object[] { content },
                Size = 1,
            };
            var databag = DataBag.SerializeWithName(nameof(User_LoginDto), content);
            CallBagWithoutToken(Rider_LoginApi, databag , bag =>
            {
                var obj = bag.Get<Login_Result>(0);
                App.SetServerUrls(obj.signalRUrl, obj.imageServerUrl);
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
                    App.SetServerUrls(obj.signalRUrl, obj.imageServerUrl);
                    Caller.RegAccessToken(obj.access_token);
                    callbackAction?.Invoke(obj);
                }, failedCallbackAction);
        }

         // CreateDeliveryOrder
        public static void CreateDeliveryOrder(DeliverOrderModel orderDto,
            Action<DeliverOrderModel> successAction,
            Action<string> failedAction)
        {
            CallBagWithRetry(User_Do_Create, DataBag.Serialize(orderDto), bag =>
            {
                var deliveryOrder = bag.Get<DeliverOrderModel>(0);
                successAction?.Invoke(deliveryOrder);
            }, arg => failedAction?.Invoke(arg));
        }

        // AssignRider
        //public static void AssignRider(DeliveryAssignmentDto assignmentDto,
        //    Action<(bool isSuccess, string arg)> callbackAction) => Call<string>(AssignRiderApi,
        //    assignmentDto, msg => callbackAction?.Invoke((true, msg)), msg => callbackAction?.Invoke((false, msg)));
        public static void Rider_AssignRider(long orderId, Action<DeliverOrderModel> successAction,
            Action<string> failedAction)
        {
            CallBagWithRetry(Rider_Do_Assign, DataBag.Serialize(orderId),
                bag => successAction?.Invoke(bag.Get<DeliverOrderModel>(0)), failedAction);
        }


        public static void User_GetActives(int pageSize, int pageIndex,
            Action<PageList<DeliverOrderModel>> successAction, Action<string> failedAction)
        {
            CallBagWithRetry(User_Get_Active, DataBag.Serialize(pageSize, pageIndex), bag =>
            {
                var orders = bag.Get<PageList<DeliverOrderModel>>(0);
                successAction?.Invoke(orders);
            }, failedAction);
        }

        public static void User_GetHistories(int pageSize, int pageIndex,
            Action<PageList<DeliverOrderModel>> successAction, Action<string> failedAction)
        {
            CallBagWithRetry(User_Get_Histories, DataBag.Serialize(pageSize, pageIndex), b =>
            {
                var orders = b.Get<PageList<DeliverOrderModel>>(0);
                successAction?.Invoke(orders);
            }, failedAction);
        }

        public static void Rider_GetAssigned(int limit, int pageIndex, Action<PageList<DeliverOrderModel>> successAction,
            Action<string> failedAction)
        {
            CallBagWithRetry(Rider_Get_Assigned, DataBag.Serialize(limit, pageIndex), bag =>
            {
                var orders = bag.Get<PageList<DeliverOrderModel>>(0);
                successAction?.Invoke(orders);
            }, failedAction);
        }

        public static void Rider_GetUnassigned(int limit, int pageIndex, Action<PageList<DeliverOrderModel>> successAction,
            Action<string> failedAction)
        {
            CallBagWithRetry(Rider_Get_Unassigned, DataBag.Serialize(limit, pageIndex), bag =>
            {
                var orders = bag.Get<PageList<DeliverOrderModel>>(0);
                successAction?.Invoke(orders);
            }, failedAction);
        }

        public static void Rider_GetHistories(int limit, int pageIndex, Action<PageList<DeliverOrderModel>> successAction,
            Action<string> failedAction)
        {
            CallBagWithRetry(Rider_Get_Histories, DataBag.Serialize(limit, pageIndex), bag =>
            {
                var orders = bag.Get<PageList<DeliverOrderModel>>(0);
                successAction?.Invoke(orders);
            }, failedAction);
        }

        public static void Rider_PickItem(DeliverOrderModel order, Action<DeliverOrderModel> successAction,
            Action<string> failedAction)
        {
            CallBagWithRetry(Rider_Do_StateUpdate, DataBag.Serialize(order),
                bag => { successAction?.Invoke(bag.Get<DeliverOrderModel>(0)); }, failedAction);
        }

        public static void CancelDeliveryOrder(long orderId, string subState,
            Action<bool, DataBag, string> callbackAction)
        {
            CallBagWithRetry(User_Do_Cancel, DataBag.Serialize(orderId, subState),
                b => callbackAction(true, b, null), m => callbackAction(false, null, m));
        }

        public static void User_GetSubStates(Action<DataBag> successAction, Action<string> failedAction) =>
            CallBagWithRetry(User_Get_SubStates, DataBag.Serialize(), successAction, failedAction);

        public static void Rider_GetSubStates(Action<DataBag> successAction, Action<string> failedAction) =>
            CallBagWithRetry(Rider_Get_SubStates, DataBag.Serialize(), successAction, failedAction);

        public static void Rider_UpdateState(long orderId, string subState, Action<DataBag> successAction,
            Action<string> failedAction) => CallBagWithRetry(EventString.Rider_Do_StateUpdate,
            DataBag.Serialize(orderId, subState), successAction, failedAction);

        public static void User_DoPay_Rider(long orderId, Action<DataBag> successAction, Action<string> failedAction) =>
            CallBagWithRetry(EventString.User_DoPay_Rider, DataBag.SerializeWithName(EventString.User_DoPay_Rider, orderId),
                successAction, failedAction);

        public static void User_DoPay_Credit(long orderId, Action<DataBag> success, Action<string> failed) =>
            CallBagWithRetry(EventString.User_DoPay_Credit, DataBag.SerializeWithName(EventString.User_DoPay_Credit, orderId), success,
                failed);

        public static async void Rider_DoTask_UploadImages(long orderId, (string url,Texture2D tex)[] args ,Action<(string url, DeliverOrderModel order)> successAction,Action<string> failedAction)
        {
            var methodName = nameof(Rider_DoTask_UploadImages);
            Panel.StartCall(methodName, false, true);
            var tokenResult = await Caller.CallStringAsync(EventString.Rider_Do_GetUpdateImagesToken,
                DataBag.SerializeWithName(EventString.Rider_Do_GetUpdateImagesToken, orderId));
            var tokenBag = DataBag.Deserialize(tokenResult);
            if (tokenBag == null)
            {
                failedAction("Connection failed!");
                Panel.EndCall(methodName);
                return;
            }
            var token = tokenBag.Get<string>(0);
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var texture = arg.tex;
                var bytes = texture.EncodeToJPG(75);
                await using var stream = new MemoryStream(bytes);
                var result = await Caller.CallStreamAsync(EventString.Rider_Image_Do, stream, true, ("token", token));
                var bag = DataBag.Deserialize(result);
                if (bag == null)
                {
                    failedAction("Unload failed!");
                    Panel.EndCall(methodName);
                    return;
                }
                var order = bag.Get<DeliverOrderModel>(0);
                RunInMainThread(() => successAction((arg.url, order)));
            }
            Panel.EndCall(methodName);
        }
    }
}