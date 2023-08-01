using System;
using System.Collections;
using System.Threading.Tasks;
using AOT.Core;
using Firebase.Auth;
using Firebase.Extensions;
using Google;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace AOT.Controllers
{
    public class GoogleSignInManager 
    {
        public string GoogleWebClientId = "467986330636-o4mmt3gvf95rq4noala9iba93lfpgl82.apps.googleusercontent.com";
        private GoogleSignInConfiguration _configuration;
        private FirebaseAuth _auth;
        private FirebaseUser _user;

        public Image UserProfilePic;
        public string _imageUrl;
        public bool IsInit { get; private set; }

        public void Init()
        {
            if (IsInit) return;
            IsInit = true;
            _configuration = new GoogleSignInConfiguration
            {
                WebClientId = GoogleWebClientId,
                RequestIdToken = true,
                UseGameSignIn = false,
                RequestEmail = true,
            };
            _auth = FirebaseAuth.DefaultInstance;
        }

        public void GoogleSignInClick(Action<FirebaseUser> callbackAction)
        {
            GoogleSignIn.Configuration = _configuration;
            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthenticatedFinished);

            void OnGoogleAuthenticatedFinished(Task<GoogleSignInUser> task)
            {
                if (task.IsFaulted)
                {
                    Debug.Log("Google sign in failed");
                }
                else if (task.IsCanceled)
                {
                    Debug.Log("Google sign in canceled");
                }
                else
                {
                    var credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
                    _auth.SignInWithCredentialAsync(credential)
                        .ContinueWithOnMainThread(t =>
                        {
                            if (t.IsCompleted || t.IsCompletedSuccessfully)
                            {
                                _user = t.Result;
                                Debug.LogFormat("User signed in successfully: {0} ({1})",
                                    _user.DisplayName, _user.UserId);
                                _imageUrl = _user.PhotoUrl.ToString();
                                App.MonoService.StartCoroutine(LoadImage(_imageUrl));
                                callbackAction?.Invoke(_user);
                            }

                            if (t.IsCanceled)
                            {
                                Debug.Log("Sign in canceled");
                                callbackAction?.Invoke(null);
                                return;
                            }

                            Debug.Log("Sign in failed");
                            callbackAction?.Invoke(null);
                        });
                }
            }
        }

        //private IEnumerator LoadImage(string imageUrl)
        //{
        //    var www = new WWW(imageUrl);
        //    yield return www;
        //    UserProfilePic.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height),
        //        new Vector2());
        //}
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
                UserProfilePic.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
    }

}