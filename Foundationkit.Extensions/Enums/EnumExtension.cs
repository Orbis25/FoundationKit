namespace Foundationkit.Extensions.Enums;
public static class EnumExtension
{
    /// <summary>
    /// Get the name of enum in a text based in the [Display] Attribute
    /// </summary>
    /// <param name="enumValue">Enum</param>
    /// <returns>string</returns>
    public static string GetAttribute(this Enum enumValue)
        => enumValue.GetType()?.GetMember(enumValue.ToString())?.FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>(false)?.Name ?? enumValue.ToString();
}