using System;
using System.Collections;
using AOT.Core;
using AOT.Test;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;

namespace AOT.Controllers
{
    public class ImageController : ControllerBase
    {
        private const string JavaGalleryCameraHelperClass = "com.icefoxz.parcelrider.GalleryCameraHelper";
        private Action<Texture2D> _onPictureTaken;
        private MonoService MonoService { get; }

        public ImageController(MonoService monoService)
        {
            MonoService = monoService;
            MonoService.OnPictureTaken.AddListener(OnImagePathReceived);//这里接收了底层当获取到图片路径后触发
        }

        /// <summary>
        /// 尝试获取图片,并将图片转换为Sprite(支持网络或者本地加载)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callbackAction"></param>
        public void Req_Image(string url, Action<Sprite> callbackAction)
        {
            // 使用MonoService启动协程请求
            MonoService.StartCoroutine(DownloadImage(url, callbackAction));
        }

        //加载图片
        private IEnumerator DownloadImage(string url, Action<Sprite> callbackAction)
        {
            using var request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // 获取下载的Texture
                var texture = DownloadHandlerTexture.GetContent(request);

                // 将Texture2D转换为Sprite
                var sprite = SpriteFromTexture2D(texture);
                callbackAction?.Invoke(sprite);

                // 释放Texture2D资源
                if (sprite != null)
                {
                    // 确保此时不再需要texture
                    GameObject.Destroy(texture);
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"Error downloading image: {request.error}");
#endif
                callbackAction?.Invoke(null);
            }
        }

        private Sprite SpriteFromTexture2D(Texture2D texture)
        {
            // 创建一个新的Sprite
            // 注意：这里的参数'true'意味着Sprite会创建texture的副本
            // 如果设为'false'，Sprite会直接使用原始Texture2D
            return Sprite.Create(texture: texture,
                rect: new Rect(0.0f, 0.0f, texture.width, texture.height),
                pivot: new Vector2(0.5f, 0.5f),
                pixelsPerUnit: 100.0f,
                extrude: 0,
                meshType: SpriteMeshType.Tight,
                border: Vector4.zero,
                generateFallbackPhysicsShape: false);
        }

        public void OpenCamera(Action<Texture2D> onPictureTakenAction)
        {
            _onPictureTaken = onPictureTakenAction;
            if (Application.platform == RuntimePlatform.Android)
            {
                MonoService.StartCoroutine(OpenCameraAfterPermission());
            }
            else
            {
                Debug.LogError("OpenCamera is not supported on this platform.");
            }
        }

        private IEnumerator OpenCameraAfterPermission()
        {
            yield return RequestCameraPermission();
            using var galleryCameraHelper = new AndroidJavaClass(JavaGalleryCameraHelperClass);
            galleryCameraHelper.CallStatic("openCamera");
        }

        public void OpenGallery(Action<Texture2D> onPictureTakenAction)
        {
            _onPictureTaken = onPictureTakenAction;
            if (Application.platform == RuntimePlatform.Android)
            {
                MonoService.StartCoroutine(OpenGalleryAfterPermission());
            }
            else
            {
                Debug.LogError("OpenGallery is not supported on this platform.");
            }
        }

        private IEnumerator OpenGalleryAfterPermission()
        {
            yield return RequestGalleryPermission();
            using var galleryCameraHelper = new AndroidJavaClass(JavaGalleryCameraHelperClass);
            galleryCameraHelper.CallStatic("openGallery");
        }

        //底层接收到图片路径后触发
        private void OnImagePathReceived(string imagePath)
        {
            //尝试加载图片
            if (!string.IsNullOrEmpty(imagePath))
                MonoService.StartCoroutine(DownloadImage("file://" + imagePath, s =>
                {
#if UNITY_EDITOR
                    if (!s) Debug.LogError("Image path is null or empty.");
#endif
                    _onPictureTaken?.Invoke(s.texture);
                }));
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
            while (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                yield return null;
            }
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
