using UnityEngine;
using UnityEngine.Events;

namespace AOT.Core
{
    /// <summary>
    /// MonoService 实现了一个单例模式，用于在Unity中启动协程,
    /// 和在Unity中接收Android端的图片路径
    /// </summary>
    public class MonoService : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            name = nameof(MonoService);
        }
        public UnityEvent<string> OnPictureTaken { get; } = new UnityEvent<string>();
        // 从底层接收Android端的图片路径
        public void OnImagePathReceived(string imagePath) => OnPictureTaken.Invoke(imagePath);
    }
}