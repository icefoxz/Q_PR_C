using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AOT.Network
{
    /// <summary>
    /// 重试调用器
    /// </summary>
    public class RetryCaller
    {
        /// <summary>
        /// 调用方法,成功后调用successAction,失败后调用OnFailedAction一次, 并且可以不断重试
        /// </summary>
        /// <param name="call">主调用</param>
        /// <param name="successAction">当成功调用</param>
        /// <param name="onErrRetry">重试机制</param>
        public async Task RetryAwait(Func<Task<string>> call,
            Action<string> successAction,
            Func<string, Task<bool>> onErrRetry)
        {
            var retry = 0;
            do
            {
                var (isSuccess, errorMessage) = await InvokeAsync(call, successAction);
                if (isSuccess) return;
                var isRetry = await onErrRetry(errorMessage); // 且重试机制返回true, 则继续重试
                if (!isRetry) return;
#if UNITY_EDITOR
                Debug.Log($"retryAwait loop = {++retry}");
#endif
            } while (true);
        }

        private static async Task<(bool,string)> InvokeAsync(Func<Task<string>> call, Action<string> successAction)
        {
            try
            {
                var result = await call.Invoke();
                if (result is not null)
                {
                    successAction(result);
                    return (true, string.Empty);
                }

                return (false,"Result = null");
            }
            catch (Exception e)
            {
                return (false, e.ToString());
            }
        }
    }
}