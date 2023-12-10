using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Controllers;
using AOT.DataModel;
using AOT.Model;
using AOT.Test;
using AOT.Utl;
using AOT.Views;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using OrderHelperLib.Dtos.Riders;
using OrderHelperLib.Dtos.Users;
using UnityEngine;
using UnityEngine.Events;

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
        public static IMapCoordinates MapCoordinates { get; private set; }
        public static bool IsTestMode { get; private set; }
        public static bool IsUserMode { get; private set; }

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

        public static void Run(Res res, MonoService monoService, MapCoordinates mapCoordinates, bool isUserMode,
            bool isTestMode)
        {
            if (IsRunning)
                throw new NotImplementedException("App is running!");
            IsUserMode = isUserMode;
            IsTestMode = isTestMode;
            IsRunning = true;
            Res = res;
            MonoService = monoService;
            MapCoordinates = mapCoordinates;
            MapCoordinates.Display(false);
            Models = new AppModels();
            ControllerReg();
            //TestData();
        }

        private static void TestData()
        {
            //add testing orders
            var testList = GenerateRandomOrders(5);
                Models.SetUser(new UserModel() { Id = "TestUser" });
            var pageIndex = 0;
            if (IsUserMode)
            {
                var userOrderController = GetController<UserOrderController>();
                userOrderController.List_ActiveOrder_Set(testList, pageIndex);
            }
            else
            {
                var riderOrderController = GetController<RiderOrderController>();
                riderOrderController.List_ActiveOrder_Set(testList, pageIndex);
            }

            #region TestRandomGenerateOrder
            List<DeliverOrderModel> GenerateRandomOrders(int count)
            {
                var random = new System.Random();
                var orders = new List<DeliverOrderModel>();

                for (int i = 0; i < count; i++)
                {
                    var order = new DeliverOrderModel
                    {
                        Id = random.Next(1000, 9999), // Random order id
                        UserId = $"User{random.Next(1000, 9999)}", // Random user id
                        User = new UserModel
                        {
                            /* Assign properties as required */
                        },
                        MyState = $"State{random.Next(1, 10)}", // Random state
                        Status = random.Next(0, 2), // Random status between 0 and 1
                        ItemInfo = new ItemInfoDto
                        {
                            Weight = random.Next(1, 10), // Random weight between 1 and 10
                            Length = random.Next(1, 10), // Similarly for Length, Width and Height
                            Width = random.Next(1, 10),
                            Height = random.Next(1, 10)
                        },
                        PaymentInfo = new PaymentInfoDto
                        {
                            Charge = random.Next(1, 100), // Random Charge
                            Fee = random.Next(1, 100), // Random Fee
                            Method = PaymentMethods.UserCredit.ToString() // Random payment method
                        },
                        DeliveryInfo = new DeliveryInfoDto
                        {
                            Distance = random.Next(1, 100), // Random distance
                            StartLocation = new LocationDto
                            {
                                Address = $"Address{random.Next(1, 10)}",
                                Latitude = random.NextDouble() * (90.0 - -90.0) + -90.0, // Random latitude
                                Longitude = random.NextDouble() * (180.0 - -180.0) + -180.0 // Random longitude
                            },
                            EndLocation = new LocationDto
                            {
                                Address = $"Address{random.Next(1, 10)}",
                                Latitude = random.NextDouble() * (90.0 - -90.0) + -90.0, // Random latitude
                                Longitude = random.NextDouble() * (180.0 - -180.0) + -180.0 // Random longitude
                            }
                        },
                        SenderInfo = new SenderInfoDto
                        {
                            User = new UserModel
                            {
                                /* Assign properties as required */
                            },
                            UserId = $"User{random.Next(1000, 9999)}" // Random sender user id
                        },
                        ReceiverInfo = new ReceiverInfoDto
                        {
                            PhoneNumber = $"PhoneNumber{random.Next(1000, 9999)}", // Random phone number
                            Name = $"Name{random.Next(1000, 9999)}" // Random name
                        },
                        RiderId = random.Next(1, 100).ToString(), // Random RiderId
                        Rider = new RiderModel
                        {
                            /* Assign properties as required */
                        }
                    };

                    orders.Add(order);
                }

                return orders;
            }
            #endregion
        }


        public static void UiInit(Canvas canvas, UiManagerBase uiManager,bool startUi)
        {
            UiBuilder = new UiBuilder(canvas, Res);
            UiManager = uiManager;
            UiManager.Init(startUi);
        }

        private static void ControllerReg()
        {
            ServiceContainer = new ControllerServiceContainer();
            //User
            ServiceContainer.Reg(new LoginController(), IsTestMode);
            ServiceContainer.Reg(new AutofillAddressController(), IsTestMode);
            ServiceContainer.Reg(new GeocodingController(), IsTestMode);
            ServiceContainer.Reg(new UserOrderController(), IsTestMode);

            //Rider
            ServiceContainer.Reg(new RiderLoginController(), IsTestMode);
            ServiceContainer.Reg(new RiderOrderController(), IsTestMode);

            //Common
            ServiceContainer.Reg(new PictureController(MonoService), IsTestMode);
            ServiceContainer.Reg(new ImageController(), IsTestMode);
        }

        public static void SendEvent(string eventName, params object[] args)
        {
            args ??= Array.Empty<object>();
            MessagingManager.SendParams(eventName, args);
        }

        public static void RegEvent(string eventName, Action<DataBag> callbackAction) =>
            MessagingManager.RegEvent(eventName, callbackAction);

        public static void SetMap(double lat, double lng, UnityAction<(double lat, double lng)> onGeoCallback) =>
            MapCoordinates.StartMap(lat, lng, onGeoCallback);
    }
}
