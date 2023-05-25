using System.Collections;
using Core;
using OrderHelperLib.DtoModels.Users;
using UnityEngine.Networking;
using UnityEngine;

namespace Model
{
    public class AppModels : ModelBase
    {
        public UserModel User { get; set; }

        public void SetUser(UserDto user)
        {
            User = new UserModel(user);
            SendEvent(EventString.User_Update);
            if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
                App.MonoService.StartCoroutine(LoadImage(user.AvatarUrl));
        }

        private IEnumerator LoadImage(string imageUrl)
        {
            if (imageUrl == null) yield break;

            var www = UnityWebRequestTexture.GetTexture(imageUrl);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                Debug.Log(www.error);
            else
            {
                var texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                var avatar = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                User.SetAvatar(avatar);
            }
        }
    }
}