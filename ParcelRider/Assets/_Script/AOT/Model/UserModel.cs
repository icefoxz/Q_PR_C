using AOT.Core;
using OrderHelperLib.DtoModels.DeliveryOrders;
using UnityEngine;
using UserDto = OrderHelperLib.DtoModels.Users.UserDto;

namespace AOT.Model
{
    public class RiderModel : ModelBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public Sprite Avatar { get; set; }

        public RiderModel(RiderDto dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Phone = dto.Phone;
        }

        public void SetAvatar(Sprite sprite)
        {
            Avatar = sprite;
            SendEvent(EventString.Rider_Update);
        }
    }
    
    public class UserModel : ModelBase
    {
        public UserModel(UserDto dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Username = dto.Username;
            Phone = dto.Phone;
            Email = dto.Email;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public Sprite Avatar { get; set; }

        public void Update(UserDto dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Username = dto.Username;
            Phone = dto.Phone;
            Email = dto.Email;
            SendEvent(EventString.User_Update);
        }

        public void SetAvatar(Sprite sprite)
        {
            Avatar = sprite;
            SendEvent(EventString.User_Update);
        }
    }
}
