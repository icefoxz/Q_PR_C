using OrderHelperLib.Dtos.Users;

namespace OrderHelperLib.Results
{
    public record Login_Result
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string signalRUrl { get; set; }
        public string imageServerUrl { get; set; }
        public UserModel User { get; set; }

        public Login_Result()
        {
            
        }
    }
}
