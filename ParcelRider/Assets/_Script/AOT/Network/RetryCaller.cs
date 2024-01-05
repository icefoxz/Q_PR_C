using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AOT.Network
{
    /// <summary>
    /// 重试调用器
    /// </summary>
    public class RetryCaller
    {
        private const string ResultNull = "Result = null";

        static async Task<T> InvokeAwaitRetry<T>(Func<Task<(bool, string, T)>> task,
            Func<(string err, T arg), Task<bool>> onErrRetry)
        {
            var retry = 0;
            do
            {
                var (isSuccess, errorMessage, arg) = await task.Invoke();
                if (isSuccess) return arg;
                var isRetry = await onErrRetry((errorMessage, arg)); // 且重试机制返回true, 则继续重试
                if (!isRetry) return arg;
#if UNITY_EDITOR
                Debug.Log($"retryAwait loop = {++retry}");
#endif
            } while (true);
        }

        public static async Task<TResult> StartAsync<TResult>(Func<Task<TResult>> call,
            Func<(string err, TResult arg), Task<bool>> onErrRetry)
        {
            return await InvokeAwaitRetry(async () =>
            {
                var result = await call();
                return result.Equals(default) ? (false, ResultNull, default(TResult)) : (true, string.Empty, result);
            }, onErrRetry);
        }

        /// <summary>
        /// 调用方法,成功后调用successAction,失败后调用OnFailedAction一次, 并且可以不断重试
        /// </summary>
        /// <param name="call">主调用</param>
        /// <param name="successAction">当成功调用</param>
        /// <param name="onErrRetry">重试机制,返回是否重试</param>
        public static async void Start<TResult>(Func<Task<TResult>> call,
            Action<TResult> successAction,
            Func<(string err, TResult arg), Task<bool>> onErrRetry)
        {
            await InvokeAwaitRetry(InvokeAsync, onErrRetry);
            return;

            async Task<(bool, string, TResult)> InvokeAsync()
            {
                try
                {
                    var result = await call.Invoke();
                    if (result is not null)
                    {
                        successAction(result);
                        return (true, string.Empty, result);
                    }

                    return (false, ResultNull, default);
                }
                catch (Exception e)
                {
                    return (false, e.ToString(), default);
                }
            }
        }
    }
}