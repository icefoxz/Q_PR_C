using AOT.Core;
using OrderHelperLib.Dtos.Users;
using UnityEngine;

namespace AOT.Model
{
    public record User : UserModel
    {
        public Sprite Avatar { get; set; }
        public User(UserModel dto) : base(dto)
        {
        }

        public void Update(UserModel dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Username = dto.Username;
            Phone = dto.Phone;
            Email = dto.Email;
            App.SendEvent(EventString.User_Update);
        }

        public void SetAvatar(Sprite sprite)
        {
            Avatar = sprite;
            App.SendEvent(EventString.User_Update);
        }
    }
}