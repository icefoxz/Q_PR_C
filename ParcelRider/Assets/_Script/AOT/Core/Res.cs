using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AOT.Core
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public class Res : MonoBehaviour
    {
        public async Task<T> LoadAsync<T>(string key)
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            return await handle.Task;
        }
        public async Task<GameObject> InstantiateAsync(string key, Transform parent)
        {
            var handle = Addressables.InstantiateAsync(key);
            var obj = await handle.Task;
            if (!obj) throw new NullReferenceException($"找不到资源：Key = {key}");
            if (parent) obj.transform.SetParent(parent);
            ResetObj(obj);
            return obj;
        }

        public void Initialize(UnityAction onCompleteAction, UnityAction<float> onProgressAction)
        {
            var handle = Addressables.InitializeAsync();
            handle.Completed += _ => onCompleteAction?.Invoke();
            StartCoroutine(AutoReleaseHandleCoroutine());

            IEnumerator AutoReleaseHandleCoroutine()
            {
                onProgressAction?.Invoke(handle.PercentComplete);
                yield return handle;
            }
        }

        public void Instantiate(string key, Transform parent, UnityAction<GameObject> callBackAction)
        {
            var handle = Addressables.InstantiateAsync(key);
            StartCoroutine(CoLoadObj(handle, ObjectLoaded));

            void ObjectLoaded(GameObject o)
            {
                if (parent) o.transform.SetParent(parent);
                ResetObj(o);
                callBackAction?.Invoke(o);
            }
        }
        //协程加载
        private IEnumerator CoLoadObj<T>(AsyncOperationHandle<T> handle, UnityAction<T> callbackAction)
        {
            if (!handle.IsDone)
                yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                callbackAction.Invoke(handle.Result);
                yield break;
            }

            throw new NullReferenceException($"资源加载.状态[{handle.Status}]:{handle.DebugName}");
        }

        private static void ResetObj(GameObject obj)
        {
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.name = obj.name.Replace("(Clone)", string.Empty);
        }

        public AsyncOperationHandle<T> LoadAssetAsyncHandler<T>(string key) =>
            Addressables.LoadAssetAsync<T>(key);

        public AsyncOperationHandle<GameObject> InstantiateAsyncHandler(string key) =>
            Addressables.InstantiateAsync(key);
    }
}