using System;
using System.Collections.Generic;
using AOT.Controllers;
using AOT.DataModel;
using AOT.Model;
using AOT.Test;
using AOT.Utl;
using AOT.Views;
using OrderHelperLib.DtoModels.Users;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AOT.Core
{
    public static class App
    {
        private static MonoService _monoService;
        private static bool IsRunning { get; set; }
        private static ControllerServiceContainer ServiceContainer { get; set; }

        public static T GetController<T>() where T : class, IController => ServiceContainer.Get<T>();
        public static Res Res { get; private set; }
        public static UiBuilder UiBuilder { get; private set; }
        public static UiManagerBase UiManager { get; private set; }
        public static AppModels Models { get; private set; }
        public static MessagingManager MessagingManager { get; } = new MessagingManager();

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

        public static void Run(Res res, Canvas canvas, UiManagerBase uiManager, MonoService monoService,bool startUi)
        {
            if (IsRunning)
                throw new NotImplementedException("App is running!");
            IsRunning = true;
            Res = res;
            MonoService = monoService;
            Models = new AppModels();
            ControllerReg();
            TestData();
            UiInit(canvas, uiManager, startUi);
        }

        private static void TestData()
        {
            //add testing orders
            var testList = new List<DeliveryOrder>();
            Models.SetUser(new UserDto{Id = "TestUser"});
            for (int i = 0; i < 2; i++)
            {
                var order = new DeliveryOrder
                {
                    Status = i == 0 ? 0 : Random.Range(0, 3),
                    Package = new PackageInfo(1.5f + i, 10f + i, 10f + i, 15f + i, 1f + i, 1f + i),
                    From = new IdentityInfo($"From {i}", "1234567890", "TestAddress1"),
                    To = new IdentityInfo($"To {i}", "1234567890", "TestAddress2"),
                };
                testList.Add(order);
            }
            if(AppLaunch.IsUserMode)
            {
                var userOrderController = GetController<UserOrderController>();
                userOrderController.List_Set(testList.ToArray());
            }
            else
            {
                var riderOrderController = GetController<RiderOrderController>();
                riderOrderController.List_Set(testList.ToArray());
            }
        }

        private static void UiInit(Canvas canvas, UiManagerBase uiManager,bool startUi)
        {
            UiBuilder = new UiBuilder(canvas, Res);
            UiManager = uiManager;
            UiManager.Init(startUi);
        }

        private static void ControllerReg()
        {
            ServiceContainer = new ControllerServiceContainer();
            //User
            ServiceContainer.Reg(new LoginController(), AppLaunch.TestMode);
            ServiceContainer.Reg(new AutofillAddressController(), AppLaunch.TestMode);
            ServiceContainer.Reg(new GeocodingController(), AppLaunch.TestMode);
            ServiceContainer.Reg(new UserOrderController(), AppLaunch.TestMode);

            //Rider
            ServiceContainer.Reg(new RiderLoginController(), AppLaunch.TestMode);
            ServiceContainer.Reg(new RiderOrderController(), AppLaunch.TestMode);

            //Common
            ServiceContainer.Reg(new PictureController(MonoService), AppLaunch.TestMode);
        }
    }
}
