using System;
using AOT.Controllers;
using AOT.Model;
using AOT.Network;
using AOT.Test;
using AOT.Utl;
using AOT.Views;
using OrderHelperLib;
using UnityEngine;
using UnityEngine.Events;
using Color = System.Drawing.Color;

namespace AOT.Core
{
    public static class App
    {
        private static MonoService _monoService;
        private static SignalRClient _signalRClient;
        private static bool IsRunning { get; set; }
        private static ControllerServiceContainer ServiceContainer { get; set; }

        public static T GetController<T>() where T : class, IController => ServiceContainer.Get<T>();
        public static Res Res { get; private set; }
        public static UiBuilder UiBuilder { get; private set; }
        public static UiManagerBase UiManager { get; private set; }
        public static AppModels Models { get; private set; }
        public static MessagingManager MessagingManager { get; } = new MessagingManager();
        public static IMapCoordinates MapCoordinates { get; private set; }
        public static bool IsTestMode { get; private set; }
        public static bool IsUserMode { get; private set; }
        public static IMainThreadDispatcher MainThread { get; private set; }

        public static ISignalRClient SignalRClient => _signalRClient;

        private static ServerCallHandler ServerCallHandler { get; } = new ServerCallHandler();

        public static MonoService MonoService
        {
            get
            {
                if (_monoService == null)
                    _monoService = new GameObject("MonoService").AddComponent<MonoService>();
                return _monoService;
            }
            private set => _monoService = value;
        }

        public static bool IsLoggedIn { get; private set; }
        public static string ImageServerUrl { get; private set; }

        public static void Run(Res res,
            MonoService monoService,
            MapCoordinates mapCoordinates,
            SignalRClient signalRClient,
            bool isUserMode,
            bool isTestMode)
        {
            if (IsRunning)
                throw new NotImplementedException("App is running!");
            IsUserMode = isUserMode;
            IsTestMode = isTestMode;
            IsRunning = true;
            Res = res;
            MonoService = monoService;
            MainThread = MonoService.gameObject.AddComponent<MainThreadDispatcher>();
            MapCoordinates = mapCoordinates;
            InitSignalR(signalRClient);//必须在ControllerReg之前
            MapCoordinates.Display(false);
            Models = new AppModels();
            ControllerReg();
            RegEventsForApp();
            return;

            void ControllerReg()
            {
                var orderSyncHandler = new OrderSyncHandler(SignalRClient.Caller);
                ServiceContainer = new ControllerServiceContainer();
                //User
                ServiceContainer.Reg(new UserLoginController(), IsTestMode);
                ServiceContainer.Reg(new AutofillAddressController(), IsTestMode);
                ServiceContainer.Reg(new GeocodingController(), IsTestMode);
                ServiceContainer.Reg(new UserOrderController(orderSyncHandler), IsTestMode);

                //Rider
                ServiceContainer.Reg(new RiderLoginController(), IsTestMode);
                ServiceContainer.Reg(new RiderOrderController(orderSyncHandler), IsTestMode);

                //Common
                ServiceContainer.Reg(new ImageController(MonoService), IsTestMode);
            }

            void RegEventsForApp()
            {
                RegEvent(EventString.User_Login, _ => SetLoginStatus(true));
                RegEvent(EventString.User_Logout, _ => SetLoginStatus(false));
                RegEvent(EventString.Rider_Login, _ => SetLoginStatus(true));
                RegEvent(EventString.Rider_Logout, _ => SetLoginStatus(false));
                return;

                void SetLoginStatus(bool isSuccess) => IsLoggedIn = isSuccess;
            }

            void InitSignalR(SignalRClient client)
            {
                _signalRClient = client;
                _signalRClient.Init();
            }
        }

        public static void UiInit(Canvas canvas, UiManagerBase uiManager,bool startUi)
        {
            UiBuilder = new UiBuilder(canvas, Res);
            UiManager = uiManager;
            UiManager.Init(startUi);
        }

        public static void SendEvent(string eventName, DataBag bag) => MessagingManager.Send(eventName, bag);
        public static void SendEvent(string eventName, params object[] args)
        {
            args ??= Array.Empty<object>();
            MessagingManager.SendParams(eventName, args);
        }

        public static void RegEvent(string eventName, Action<DataBag> callbackAction) =>
            MessagingManager.RegEvent(eventName, callbackAction);

        public static void SetMap(double lat, double lng, UnityAction<(double lat, double lng)> onGeoCallback) =>
            MapCoordinates.StartMap(lat, lng, onGeoCallback);

        public static void SetServerUrls(string signalRUrl,string imageUrl)
        {
            SetImageServerUrl();
            SetSignalServerUrl();
            return;

            void SetImageServerUrl()
            {
                ImageServerUrl = imageUrl;
                Debug.Log($"SetImageServerUrl: {imageUrl.Color(Color.LightPink)}");
            }

            void SetSignalServerUrl()
            {
                _signalRClient.SetServerUrl(signalRUrl);
                Debug.Log($"SetSignalServerUrl: {signalRUrl.Color(Color.LightPink)}");
            }
        }
    }
}
