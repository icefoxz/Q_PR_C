using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AOT.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace AOT.Controllers
{
    public class SpriteResManager
    {
        private class SpriteInfo
        {
            public Sprite Sprite;
            public int MemoryUsage;
            public int LastAccessTime;
        }

        private Dictionary<string, SpriteInfo> spriteCache;
        private int maxMemoryUsage;
        private int currentMemoryUsage;

        public SpriteResManager(int maxMemoryInMb)
        {
            spriteCache = new Dictionary<string, SpriteInfo>();
            maxMemoryUsage = maxMemoryInMb * 1024 * 1024;// mb
            currentMemoryUsage = 0;
        }

        public async UniTask<Sprite> GetSpriteAsync(string url)
        {
            if (spriteCache.TryGetValue(url, out SpriteInfo spriteInfo))
            {
                // Update access time
                spriteInfo.LastAccessTime = Environment.TickCount;
                return spriteInfo.Sprite;
            }

            var texture = await DownloadImageFromUrl(url);
            if (texture == null) return null;

            var newSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            int memoryUsage = CalculateTextureSize(texture);
            currentMemoryUsage += memoryUsage;

            spriteCache[url] = new SpriteInfo
            {
                Sprite = newSprite,
                MemoryUsage = memoryUsage,
                LastAccessTime = Environment.TickCount
            };

            CheckAndManageMemory();

            return newSprite;

            async Task<Texture2D> DownloadImageFromUrl(string url)
            {
                using var request = UnityWebRequestTexture.GetTexture(url);
                await request.SendWebRequest().ToUniTask();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    // 获取下载的Texture
                    var texture = DownloadHandlerTexture.GetContent(request);
                    return texture;
                }
#if UNITY_EDITOR
                Debug.LogError($"Error downloading image: {request.error}");
#endif
                return null;
            }
        }

        private void CheckAndManageMemory()
        {
            while (currentMemoryUsage > maxMemoryUsage)
            {
                var leastRecentlyUsed = spriteCache.OrderBy(pair => pair.Value.LastAccessTime).FirstOrDefault();
                if (leastRecentlyUsed.Value != null)
                {
                    currentMemoryUsage -= leastRecentlyUsed.Value.MemoryUsage;
                    spriteCache.Remove(leastRecentlyUsed.Key);
                }
            }
        }

        private int CalculateTextureSize(Texture2D texture)
        {
            int bytesPerPixel = 4; // Assuming RGBA32 format
            int mipMapFactor = texture.mipmapCount > 1 ? 4 / 3 : 1; // Adjust if using mipmaps
            return texture.width * texture.height * bytesPerPixel * mipMapFactor;
        }
    }
}