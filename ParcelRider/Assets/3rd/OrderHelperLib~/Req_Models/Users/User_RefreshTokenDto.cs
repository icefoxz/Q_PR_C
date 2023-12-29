namespace OrderHelperLib.Req_Models.Users;

public record User_RefreshTokenDto
{
    public string Username { get; set; }

    public User_RefreshTokenDto()
    {
    }
}