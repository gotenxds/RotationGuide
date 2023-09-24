namespace RotationMaster.Utils;

public static class StringUtils
{
    public static string Capitalize(this string str)
    {
        return char.ToUpper(str[0]) + str[1..];
    } 
}
