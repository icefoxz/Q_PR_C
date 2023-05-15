//using UnityEngine;
//using UnityEngine.UI;

//namespace Controllers
//{
//    public class GoogleSignInManager : MonoBehaviour
//    {
//        public string GoogleApiKey = "AIzaSyDLB5ohAWBpx2IQxQwiLIUNJu9Wb3V2ckk";
//        private GoogleSignInConfiguration _configuration;
//        private DependencyStatus _dependencyStatus = DependencyStatus.UnavailableOther;
//        private FirebaseAuth _auth;
//        private FirebaseUser _user;

//        public TextAsset UsernameText, UserEmailText;
//        public Image UserProfilePic;
//        public string _imageUrl;
//        public GameObject LoginScreen, ProfileScreen;

//        private void Start()
//        {
//            _auth = FirebaseAuth.DefaultInstance;
//        }

//        private void GoogleSignInClick()
//        {
            
//            GoogleSignIn.Configuration = _configuration;
//            GoogleSignIn.Configuration.UseGameSignIn = false;
//            GoogleSignIn.Configuration.RequestIdToken = true;
//            GoogleSignIn.Configuration.RequestEmail = true;
//        }
//    }
//}