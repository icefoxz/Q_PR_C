//using Firebase.Auth;
//using Firebase.Extensions;
//using UnityEngine;

//namespace Controllers
//{
//    public class FirebaseAuthenticateManager
//    {
//        public void SignInWithGoogle()
//        {
//            string providerId = "google.com";
//            string accessToken = "YOUR_GOOGLE_ACCESS_TOKEN"; // 从Google SDK获取
                
//            //SignInWithProvider(providerId, accessToken);
//        }

//        public void SignInWithFacebook()
//        {
//            var providerId = "facebook.com";
//            var accessToken = "YOUR_FACEBOOK_ACCESS_TOKEN"; // 从Facebook SDK获取
//            var credential = OAuthProvider.GetCredential(providerId, null, accessToken);
//            SignInWithProvider(credential);
//        }

//        private void SignInWithProvider(Credential credential)
//        {
//            FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(credential)
//                .ContinueWithOnMainThread(task =>
//                {
//                    if (task.IsCanceled)
//                    {
//                        Debug.LogError("SignIn canceled");
//                        return;
//                    }
//                    if (task.IsFaulted)
//                    {
//                        Debug.LogError("SignIn faulted: " + task.Exception);
//                        return;
//                    }

//                    FirebaseUser user = task.Result;
//                    Debug.LogFormat("SignIn success: {0} ({1})", user.DisplayName, user.UserId);

//                    // 在这里发送UID到您的服务器，以创建或登录用户
//                    SendUserIdToServer(user.UserId);
//                });
//        }

//        private void SendUserIdToServer(string userId)
//        {
//            // 这将根据您的服务器API进行调整
//            // 一般来说，您可能会使用UnityWebRequest或其他HTTP库发送请求
//        }
//    }
//}