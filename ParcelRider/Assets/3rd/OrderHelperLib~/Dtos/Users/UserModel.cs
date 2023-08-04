using OrderHelperLib.Dtos.Lingaus;

namespace OrderHelperLib.Dtos.Users
{
    public record UserModel : Dto<string>
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AvatarUrl { get; set; }
        public LingauModel Lingau { get; set; }
    }
}