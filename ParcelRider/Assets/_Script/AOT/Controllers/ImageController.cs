using System;
using System.Collections;
using System.Threading.Tasks;
using AOT.Core;
using AOT.Test;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;

namespace AOT.Controllers
{
    public class ImageController : ControllerBase
    {
        private const string JavaGalleryCameraHelperClass = "com.icefoxz.letsmove.GalleryCameraHelper";
        private Action<Sprite> _onPictureTaken;
        private MonoService MonoService { get; }
        private SpriteResManager SpriteResManager { get; }

        public ImageController(MonoService monoService)
        {
            MonoService = monoService;
            MonoService.OnPictureTaken.AddListener(OnImagePathReceived);//这里接收了底层当获取到图片路径后触发
            SpriteResManager = new SpriteResManager(50); //50mb
        }

        /// <summary>
        /// 尝试获取图片,并将图片转换为Sprite(支持网络或者本地加载)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callbackAction"></param>
        public void Req_Image(string url, Action<Sprite> callbackAction)
        {
            // 使用MonoService启动协程请求
            MonoService.StartCoroutine(GetImageFromResource(url, callbackAction).ToCoroutine());
        }


        // 将DownloadImage方法封装为返回UniTask<Texture2D>的异步方法
        //private Task<Texture2D> DownloadImageTask(string url)
        //{
        //    // 使用一个CompletionSource来等待协程完成
        //    var completionSource = new TaskCompletionSource<Texture2D>();

        //    // 开始执行协程，并提供一个回调来完成任务
        //    MonoService.StartCoroutine(GetImageFromResource(url, texture => completionSource.TrySetResult(texture)));

        //    // 等待并返回结果
        //    return completionSource.Task;
        //}

        //加载图片
        private async UniTask GetImageFromResource(string url, Action<Sprite> callbackAction)
        {
            var sp = await SpriteResManager.GetSpriteAsync(url);
            callbackAction?.Invoke(sp);
        }

        public void OpenCamera(Action<Sprite> onPictureTakenAction, Action<string, Exception> onErrorAction)
        {
            _onPictureTaken = onPictureTakenAction;
            if (Application.platform == RuntimePlatform.Android)
            {
                MonoService.StartCoroutine(OpenCameraAfterPermission(onErrorAction));
            }
            else
            {
                Debug.LogError("OpenCamera is not supported on this platform.");
            }
        }

        private IEnumerator OpenCameraAfterPermission(Action<string, Exception> onErrorAction)
        {
            yield return RequestCameraPermission();
            try
            {
                using var galleryCameraHelper = new AndroidJavaClass(JavaGalleryCameraHelperClass);
                galleryCameraHelper.CallStatic("openCamera");
            }
            catch (Exception e)
            {
                onErrorAction?.Invoke("Failed to open camera.", e);
            }
        }

        public void OpenGallery(Action<Sprite> onPictureTakenAction, Action<string, Exception> onErrorAction)
        {
            _onPictureTaken = onPictureTakenAction;
            if (Application.platform == RuntimePlatform.Android)
            {
                MonoService.StartCoroutine(OpenGalleryAfterPermission(onErrorAction));
            }
            else
            {
                Debug.LogError("OpenGallery is not supported on this platform.");
            }
        }

        private IEnumerator OpenGalleryAfterPermission(Action<string, Exception> onErrorAction)
        {
            yield return RequestGalleryPermission();
            try
            {
                using var galleryCameraHelper = new AndroidJavaClass(JavaGalleryCameraHelperClass);
                galleryCameraHelper.CallStatic("openGallery");
            }
            catch (Exception e)
            {
                onErrorAction?.Invoke("Failed to open gallery.", e);
            }
        }

        //底层接收到图片路径后触发
        private async void OnImagePathReceived(string imagePath)
        {
            //尝试加载图片
            if (string.IsNullOrEmpty(imagePath))
            {
                _onPictureTaken?.Invoke(null);
                return;
            }
            var path = "file://" + imagePath;
            var sp = await SpriteResManager.GetSpriteAsync(path);
            _onPictureTaken.Invoke(sp);
        }

        //private IEnumerator LoadImageFromPath(string imagePath)
        //{
        //    using var uwr = UnityWebRequestTexture.GetTexture();
        //    yield return uwr.SendWebRequest();

        //    if (uwr.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
        //    {
        //        Debug.LogError("Failed to load the image: " + uwr.error);
        //    }
        //    else
        //    {
        //        // 获取Texture
        //        var texture = DownloadHandlerTexture.GetContent(uwr);

        //        // 在这里处理获得的图像纹理
        //        _onPictureTaken?.Invoke(texture);
        //        Debug.Log("Image loaded successfully.");
        //    }
        //}

        private IEnumerator RequestCameraPermission()
        {
            if (Permission.HasUserAuthorizedPermission(Permission.Camera)) yield break;
            Permission.RequestUserPermission(Permission.Camera);
            yield return UniTask.WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.Camera));
        }

        private IEnumerator RequestGalleryPermission()
        {
            if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead)) yield break;
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
            while (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                yield return null;
            }
        }
    }
}
