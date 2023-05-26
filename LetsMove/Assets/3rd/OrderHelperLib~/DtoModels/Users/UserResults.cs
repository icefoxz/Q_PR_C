namespace OrderHelperLib.DtoModels.Users
{
    public class LoginResult
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public UserDto User { get; set; }
    }
}
