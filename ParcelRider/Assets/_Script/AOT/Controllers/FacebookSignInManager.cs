using System;
using System.Collections.Generic;
using Facebook.Unity;
using UnityEngine;

namespace AOT.Controllers
{
    public class FacebookSignInManager
    {
        public void Init()
        {
            if (!FB.IsInitialized)
            {
                FB.Init();
            }
            else
            {
                FB.ActivateApp();
            }
        }
        public void OnLoginButtonClicked(Action<(bool isSuccess, string name, string email, string pictureUrl)> callbackAction)
        {
            var permissions = new List<string>() { "public_profile", "email" };
            FB.LogInWithReadPermissions(permissions, OnLoginComplete);

            void OnLoginComplete(ILoginResult result)
            {
                if (FB.IsLoggedIn)
                {
                    // 登录成功
                    var accessToken = AccessToken.CurrentAccessToken;
                    Debug.Log("User ID: " + accessToken.UserId);
                    // 请求用户名字、头像和邮箱
                    FB.API("/me?fields=first_name,last_name,picture.type(large),email", HttpMethod.GET, OnGetInfoComplete);
                }
                else
                {
                    callbackAction?.Invoke((false, null, null, null));
                    // 登录失败
                    Debug.Log("User cancelled login");
                }
            }

            void OnGetInfoComplete(IGraphResult result)
            {
                if (result.Error != null)
                {
                    Debug.LogError("Error in getting user info: " + result.Error);
                    callbackAction?.Invoke((false, null, null, null));
                    return;
                }

                var firstName = result.ResultDictionary["first_name"] as string;
                var lastName = result.ResultDictionary["last_name"] as string;
                var email = result.ResultDictionary["email"] as string;
                Debug.Log("User's name: " + firstName + " " + lastName);
                Debug.Log("User's email: " + email);

                var pictureData = (result.ResultDictionary["picture"] as Dictionary<string, object>)?["data"] as Dictionary<string, object>;
                if (pictureData != null)
                {
                    var pictureUrl = pictureData["url"] as string;
                    Debug.Log("User's picture URL: " + pictureUrl);

                    callbackAction?.Invoke((true, firstName + " " + lastName, email, pictureUrl));
                }
                else
                {
                    callbackAction?.Invoke((true, firstName + " " + lastName, email, null));
                }
            }
        }
    }
}