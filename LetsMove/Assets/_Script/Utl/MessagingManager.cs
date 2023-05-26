using System;
using System.Collections.Generic;
using OrderHelperLib;
using UnityEngine;

namespace Utl
{
    /// <summary>
    /// 事件消息系统
    /// </summary>
    public class MessagingManager
    {
        private Dictionary<string, Dictionary<string, Action<string>>> EventMap { get; set; } =
            new Dictionary<string, Dictionary<string, Action<string>>>();

        /// <summary>
        /// 发送事件, 所有物件将以<see cref="DataBag"/>序列化
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="objs"></param>
        public void SendParams(string eventName, params object[] objs) => Send(eventName, DataBag.Serialize(objs));
        public void Send(string eventName, object obj) => Send(eventName, DataBag.Serialize(obj));
        /// <summary>
        /// 发送事件, 参数为<see cref="string"/>
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="args"></param>
        public void Send(string eventName, string args)
        {
            if (EventMap.ContainsKey(eventName))
                foreach (var (_, action) in EventMap[eventName])
                    action?.Invoke(string.IsNullOrEmpty(args) ? string.Empty : args);
            else
            {
                Debug.LogWarning($"{eventName} 没有注册事件!");
            }
        }

        private string RegEvent(string eventName, Action<string> action)
        {
            if (!EventMap.ContainsKey(eventName))
            {
                EventMap.Add(eventName, new Dictionary<string, Action<string>>());
            }
            var key = Guid.NewGuid().ToString();
            EventMap[eventName].Add(key, action);
            return key;
        }
        /// <summary>
        /// 注册<see cref="DataBag"/>事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action">todo : 注意! ILRuntime lamda 不可以缩进方法, 必须把bag声明出来,例: bag=> method(bag);</param>
        /// <returns></returns>
        public string RegEvent(string eventName, Action<DataBag> action)
        {
            return RegEvent(eventName, ObjBagSerialize);
            void ObjBagSerialize(string arg) => action?.Invoke(DataBag.Deserialize(arg));
        }
        /// <summary>
        /// 删除事件方法(仅仅是删除一个事件方法, 其余的监听方法依然有效)
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="key"></param>
        public void RemoveEvent(string eventName, string key)
        {
            if (EventMap[eventName].ContainsKey(key))
                EventMap[eventName].Remove(key);
        }
    }
}