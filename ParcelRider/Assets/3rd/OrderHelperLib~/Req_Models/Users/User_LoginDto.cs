namespace OrderHelperLib.Req_Models.Users;

public record User_LoginDto
{
    public string Username { get; set; }
    public string Password { get; set; }

    public User_LoginDto()
    {
        
    }
}