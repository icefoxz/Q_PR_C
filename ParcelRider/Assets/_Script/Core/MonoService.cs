using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    public class MonoService : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            name = nameof(MonoService);
        }
        public UnityEvent<string> OnPictureTaken { get; } = new UnityEvent<string>();
        public void OnImagePathReceived(string imagePath) => OnPictureTaken.Invoke(imagePath);
    }
}