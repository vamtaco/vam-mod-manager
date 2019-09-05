
namespace VaM.ModManager.App.Extensions
{
    public static class EnumExtensions
    {
        public static string ToName<TEnum>(this TEnum EnumValue) where TEnum : struct
        {
            return EnumValue.GetType().GetEnumName(EnumValue);
        }
    }
}
