namespace AOT.DataModel
{
    public class User : EntityBase<string>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
}