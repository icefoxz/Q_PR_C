
using System.Drawing;

namespace AOT.Utl
{
    public static class ColorExtension
    {
        public static string ColorToHex(Color color)
        {
            // 将 Color 对象的各个分量转换为十六进制字符串
            return $"{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
        }
        public static string Color(this string str, Color color) => $"<color=#{ColorToHex(color)}>{str}</color>";
    }
}