using System;
using OrderHelperLib.Dtos.DeliveryOrders;

namespace AOT.Extensions
{
    public static class DtoExtension
    {
        public static double Size(this ItemInfoDto info) => MathF.Pow(info.Length * info.Width * info.Height, 1f / 3f);

        public static double GetVolume(this ItemInfoDto info) => info.Height * info.Width * info.Length / 6000;
    }
}
