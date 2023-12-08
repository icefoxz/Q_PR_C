using AOT.Core;
using System;
using System.Collections;
using AOT.Test;
using UnityEngine;
using UnityEngine.Networking;

namespace AOT.Controllers
{
    public class ImageController : ControllerBase
    {
        private MonoService MonoService => App.MonoService;

        public void Req_Image(string url, Action<Sprite> callbackAction)
        {
            // 使用MonoService启动协程
            MonoService.StartCoroutine(DownloadImage(url, callbackAction));
        }

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
                Debug.LogError($"Error downloading image: {request.error}");
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
    }
}