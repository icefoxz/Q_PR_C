using Core;
using OrderHelperLib.DtoModels.Users;

namespace Model
{
    public class AppModels : ModelBase
    {
        public UserModel User { get; set; }

        public void SetUser(UserDto user)
        {
            User = new UserModel(user);
            SendEvent(EventString.User_Update);
        }
    }
}