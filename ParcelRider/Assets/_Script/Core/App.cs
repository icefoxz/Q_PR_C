using System;
using System.Collections.Generic;
using Controllers;
using DataModel;
using Model;
using UnityEngine;
using Utl;
using Random = UnityEngine.Random;

namespace Core
{
    public static class App
    {
        private static MonoService _monoService;
        private static bool IsRunning { get; set; }
        private static ControllerServiceContainer ServiceContainer { get; set; }

        public static T GetController<T>() where T : class, IController
            => ServiceContainer.Get<T>();

        public static Res Res { get; private set; }
        public static UiBuilder UiBuilder { get; private set; }
        public static UiManager UiManager { get; private set; }
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

        public static void Run(Res res, Canvas canvas, UiManager uiManager, MonoService monoService)
        {
            if (IsRunning)
                throw new NotImplementedException("App is running!");
            IsRunning = true;
            Res = res;
            MonoService = monoService;
            Models = new AppModels();
            ControllerReg();
            TestData();
            UiInit(canvas, uiManager);
        }

        private static void TestData()
        {
            var packageController = GetController<PackageController>();
            //add testing orders
            var testList = new List<DeliveryOrder>();
            for (int i = 0; i < 3; i++)
            {
                var order = new DeliveryOrder
                {
                    Id = i.ToString(),
                    Status = i == 0 ? 0 : Random.Range(0, 5),
                    Package = new PackageInfo(1.5f + i, 10f + i, 10f + i, 15f + i, 1f + i, 1f + i),
                    From = new IdentityInfo($"From {i}", "1234567890", "TestAddress1"),
                    To = new IdentityInfo($"To {i}", "1234567890", "TestAddress2"),
                };
                testList.Add(order);
            }
            packageController.AddOrder(testList.ToArray());
        }

        private static void UiInit(Canvas canvas, UiManager uiManager)
        {
            UiBuilder = new UiBuilder(canvas, Res);
            UiManager = uiManager;
            UiManager.Init();
        }

        private static void ControllerReg()
        {
            ServiceContainer = new ControllerServiceContainer();
            ServiceContainer.Reg(new LoginController());
            ServiceContainer.Reg(new PackageController());
            ServiceContainer.Reg(new AutofillAddressController());
            ServiceContainer.Reg(new GeocodingController());
            ServiceContainer.Reg(new RiderController());
            ServiceContainer.Reg(new PictureController(MonoService));
        }
    }
}
