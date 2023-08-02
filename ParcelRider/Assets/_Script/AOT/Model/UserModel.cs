using AOT.Core;
using AOT.DataModel;
using OrderHelperLib.DtoModels.DeliveryOrders;
using OrderHelperLib.DtoModels.Users;
using UnityEngine;

namespace AOT.Model
{
    public class RiderModel : ModelBase
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public Sprite Avatar { get; set; }

        public RiderModel(RiderDto dto)
        {
            Id = dto.Id.ToString();
            Name = dto.Name;
            Phone = dto.Phone;
        }

        public RiderModel()
        {
        }

        public void SetAvatar(Sprite sprite)
        {
            Avatar = sprite;
            SendEvent(EventString.Rider_Update);
        }

        public Rider ToEntity() =>
            new()
            {
                Id = Id,
                Name = Name,
                Phone = Phone
            };
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
