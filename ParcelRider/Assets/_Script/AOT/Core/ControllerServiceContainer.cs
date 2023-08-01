using System;
using System.Collections.Generic;

namespace AOT.Core
{
    /// <summary>
    /// 逻辑控制器, 主要控制与模型和数据的交互
    /// </summary>
    public interface IController
    {
        void SetTestMode(bool isTestMode);
    }
    /// <summary>
    /// 基于<see cref="IController"/>的DI容器
    /// </summary>
    public class ControllerServiceContainer
    {
        private Dictionary<object, IController> Container { get; set; } = new Dictionary<object, IController>();

        public T Get<T>() where T : class, IController
        {
            var type = typeof(T);
            if (!Container.TryGetValue(type.Name, out var obj))
                throw new NotImplementedException($"{type.Name} hasn't register!");
            return obj as T;
        }

        public void Reg<T>(T controller, bool testMode) where T : class, IController
        {
            controller.SetTestMode(testMode);
            Container.Add(typeof(T).Name, controller);
        }
    }
}