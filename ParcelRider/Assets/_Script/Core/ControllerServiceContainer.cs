using System;
using System.Collections.Generic;

namespace Core
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
        private Dictionary<Type, IController> Container { get; set; } = new Dictionary<Type, IController>();

        public T Get<T>() where T : class, IController
        {
            var type = typeof(T);
            if (!Container.TryGetValue(type, out var obj))
                throw new NotImplementedException($"{type.Name} hasn't register!");
            return obj as T;
        }

        public void Reg<T>(T controller, bool testMode) where T : class, IController
        {
            controller.SetTestMode(testMode);
            Container.Add(typeof(T), controller);
        }
    }
}