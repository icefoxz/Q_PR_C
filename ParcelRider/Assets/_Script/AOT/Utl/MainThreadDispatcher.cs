using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace AOT.Utl
{
    public interface IMainThreadDispatcher
    {
        void Enqueue(Action action);
    }
    public class MainThreadDispatcher : MonoBehaviour,IMainThreadDispatcher
    {
        private static readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();
        private static MainThreadDispatcher _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Enqueue(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
#if !UNITY_EDITOR
            action();
            return;
#else
            _executionQueue.Enqueue(action);
#endif
        }

        private void Update()
        {
            while (_executionQueue.TryDequeue(out var action))
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    // 日志记录或其他异常处理
                    //Debug.LogError($"Exception occurred during MainThreadDispatcher action: {ex}");
                    Debug.LogException(ex);
                }
            }
        }
    }
}