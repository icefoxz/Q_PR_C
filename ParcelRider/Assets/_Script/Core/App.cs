using System;
using Controllers;
using UnityEngine;

namespace Core
{
    public static class App
    {
        private static bool IsRunning { get; set; }
        private static ControllerServiceContainer ServiceContainer { get; set; }

        public static T GetController<T>() where T : class, IController
            => ServiceContainer.Get<T>();

        public static Res Res { get; private set; }
        public static UiBuilder UiBuilder { get; private set; }
        public static UiManager UiManager { get; private set; }

        public static void Run(Res res, Canvas canvas, UiManager uiManager)
        {
            if (IsRunning)
                throw new NotImplementedException("App is running!");
            IsRunning = true;
            Res = res;
            UiBuilder = new UiBuilder(canvas, Res);
            UiManager = uiManager;
            UiManager.Init();
            ServiceContainer = new ControllerServiceContainer();
            ServiceContainer.Reg(new LoginController());
        }

    }
}
