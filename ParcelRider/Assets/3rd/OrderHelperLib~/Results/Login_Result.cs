using OrderHelperLib.Dtos.Users;

namespace OrderHelperLib.Results
{
    public class Login_Result
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string signalRUrl { get; set; }
        public UserModel User { get; set; }
    }
}
