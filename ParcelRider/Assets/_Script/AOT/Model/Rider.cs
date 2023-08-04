using AOT.Core;
using OrderHelperLib.Dtos.Riders;
using UnityEngine;

namespace AOT.Model
{
    public record Rider : RiderModel
    {
        public Sprite Avatar { get; set; }

        public Rider(Rider dto):base(dto)
        {
            Avatar = dto.Avatar;
        }

        public Rider()
        {
        }

        public void SetAvatar(Sprite sprite)
        {
            Avatar = sprite;
            App.SendEvent(EventString.Rider_Update);
        }
    }
}
