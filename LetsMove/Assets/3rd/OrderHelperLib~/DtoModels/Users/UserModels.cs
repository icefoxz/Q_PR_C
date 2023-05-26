namespace OrderHelperLib.DtoModels.Users
{
    public class UserDto 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AvatarUrl { get; set; }
    }
    public class RegisterDto
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set;}
    }

    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RefreshTokenDto
    {
        public string Username { get; set; }
    }
}