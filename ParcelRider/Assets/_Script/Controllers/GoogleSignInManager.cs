using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using Google;
using UnityEngine.Networking;

namespace Controllers
{
    //public class FacebookSignInManager
    //{
    //    public void OnLoginButtonClicked()
    //    {
    //        var permissions = new List<string>() { "public_profile", "email" };
    //        FB.LogInWithReadPermissions(permissions, OnLoginComplete);
    //    }
    //}

    public class GoogleSignInManager : MonoBehaviour
    {
        public string GoogleWebClientId = "467986330636-o4mmt3gvf95rq4noala9iba93lfpgl82.apps.googleusercontent.com";
        private GoogleSignInConfiguration _configuration;
        private DependencyStatus _dependencyStatus = DependencyStatus.UnavailableOther;
        private FirebaseAuth _auth;
        private FirebaseUser _user;

        public string UsernameText, UserEmailText;
        public Image UserProfilePic;
        public string _imageUrl;
        public GameObject LoginScreen, ProfileScreen;

        private void Start()
        {
            _configuration = new GoogleSignInConfiguration
            {
                WebClientId = GoogleWebClientId,
                RequestIdToken = true,
                UseGameSignIn = false,
                RequestEmail = true,
            };
            _auth = FirebaseAuth.DefaultInstance;
        }

        private void GoogleSignInClick()
        {
            GoogleSignIn.Configuration = _configuration;
            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthenticatedFinished);
        }

        private void OnGoogleAuthenticatedFinished(Task<GoogleSignInUser> task)
        {
            if (task.IsFaulted)
            {
                Debug.Log("Google sign in failed");
            }else if (task.IsCanceled)
            {
                Debug.Log("Google sign in canceled");
            }
            else
            {
                Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
                _auth.SignInWithCredentialAsync(credential)
                    .ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCanceled)
                        {
                            Debug.Log("Sign in canceled");
                            return;
                        }

                        if (task.IsFaulted)
                        {
                            Debug.Log("Sign in failed");
                            return;
                        }

                        if (task.IsCompleted)
                        {
                            _user = task.Result;
                            Debug.LogFormat("User signed in successfully: {0} ({1})",
                                                               _user.DisplayName, _user.UserId);
                            UsernameText= _user.DisplayName;
                            UserEmailText= _user.Email;
                            _imageUrl = _user.PhotoUrl.ToString();
                            StartCoroutine(LoadImage(_imageUrl));
                            LoginScreen.SetActive(false);
                            ProfileScreen.SetActive(true);
                        }

                    });
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