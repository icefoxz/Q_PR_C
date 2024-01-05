using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AOT.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace AOT.Controllers
{
    public class SpriteResHandler
    {
        private const int MaxPixels = 1024;

        private class SpriteInfo
        {
            public Sprite Sprite;
            public int MemoryUsage;
            public int LastAccessTime;
            public Texture2D Texture;
        }

        private Dictionary<string, SpriteInfo> spriteCache;
        private int maxMemoryUsage;
        private int currentMemoryUsage;
        private string ImageServerUrl => App.ImageServerUrl;

        public SpriteResHandler(int maxMemoryInMb)
        {
            spriteCache = new Dictionary<string, SpriteInfo>();
            maxMemoryUsage = maxMemoryInMb * 1024 * 1024;// mb
            currentMemoryUsage = 0;
        }

        // 处理图片参数
        private string ProcessImageParam(string imageParam)
        {
            if (ImageServerUrl == null) throw new Exception("Image server url is not set!");
            // 检查是否为GUID
            if (Guid.TryParse(imageParam, out _))
            {
                // 转换GUID为HTTP URL
                return $"{ImageServerUrl}/{imageParam}"; // 假设图片格式为jpg
            }
            // 检查是否为HTTP URL

            if (imageParam.StartsWith("http://") || imageParam.StartsWith("https://"))
                return imageParam; // 使用HTTP URL

            // 处理本地文件路径
            if(imageParam.StartsWith("file://")) return imageParam; // 使用本地路径

#if UNITY_EDITOR
            Debug.LogError($"Image param not recognized = {imageParam}");
#endif
            return null;
        }

        public async UniTask<Sprite> GetSpriteAsync(string arg)
        {
            var url = ProcessImageParam(arg);
            if (spriteCache.TryGetValue(url, out SpriteInfo spriteInfo))
            {
                // Update access time
                spriteInfo.LastAccessTime = Environment.TickCount;
                return spriteInfo.Sprite;
            }

            var texture = await DownloadImageFromUrl(url);
            if (texture == null) return null;

            var newSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            var memoryUsage = CalculateTextureSize(texture);
            currentMemoryUsage += memoryUsage;

            spriteCache[url] = new SpriteInfo
            {
                Sprite = newSprite,
                MemoryUsage = memoryUsage,
                LastAccessTime = Environment.TickCount,
                Texture = texture
            };

            CheckAndManageMemory();

            return newSprite;

            async Task<Texture2D> DownloadImageFromUrl(string imgUrl)
            {
                using var request = UnityWebRequestTexture.GetTexture(imgUrl);
                await request.SendWebRequest().ToUniTask();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    // 获取下载的Texture
                    var tex = DownloadHandlerTexture.GetContent(request);
                    //return texture;
                    var resizedTexture = CopyAndResizeTexture(tex);
                    return resizedTexture;
                }
#if UNITY_EDITOR
                Debug.LogError($"Error downloading image: {request.error}");
#endif
                return null;
            }

            Texture2D CopyAndResizeTexture(Texture2D originalTexture)
            {
                var originalWidth = originalTexture.width;
                var originalHeight = originalTexture.height;

                // 如果原始纹理尺寸已经在限制范围内，不需要复制，直接返回原纹理
                if (originalWidth <= MaxPixels && originalHeight <= MaxPixels)
                    return originalTexture;

                // 确定新纹理的尺寸
                var aspectRatio = (float)originalWidth / originalHeight;
                int newWidth, newHeight;

                if (originalWidth > originalHeight)
                {
                    newWidth = MaxPixels;
                    newHeight = (int)(newWidth / aspectRatio);
                }
                else
                {
                    newHeight = MaxPixels;
                    newWidth = (int)(newHeight * aspectRatio);
                }

                // 创建新的 Texture2D 对象
                var newTexture = new Texture2D(newWidth, newHeight, originalTexture.format, originalTexture.mipmapCount > 1);
                newTexture.SetPixels(originalTexture.GetPixels());
                newTexture.Apply();

                // 销毁原始纹理
                Object.Destroy(originalTexture);

                return newTexture;
            }
            void CheckAndManageMemory()
            {
                while (currentMemoryUsage > maxMemoryUsage)
                {
                    var leastRecentlyUsed = spriteCache.OrderBy(pair => pair.Value.LastAccessTime).FirstOrDefault();
                    if (leastRecentlyUsed.Value != null) Remove(leastRecentlyUsed.Key);
                }
            }
        }

        public void Remove(string url)
        {
            if (!spriteCache.Remove(url, out SpriteInfo spriteInfo)) return;
            currentMemoryUsage -= spriteInfo.MemoryUsage;
            Object.Destroy(spriteInfo.Texture);
        }

        private int CalculateTextureSize(Texture2D texture)
        {
            int bytesPerPixel = 4; // Assuming RGBA32 format
            int mipMapFactor = texture.mipmapCount > 1 ? 4 / 3 : 1; // Adjust if using mipmaps
            return texture.width * texture.height * bytesPerPixel * mipMapFactor;
        }

        public (string url, Texture2D)[] GetTextures(string[] urls) =>
            spriteCache.Join(urls, m => m.Key, u => u, (s, u) => (u, s.Value.Texture)).ToArray();

    }
}