using System.Numerics;
using ImGuiNET;

namespace RotationGuide.Utils;

public static class Colors
{
    public static readonly uint OGCDBarInner = RGBAToU32(new Vector4(47f, 47f, 55f, 255));
    public static readonly uint OGCDBarBorder = RGBAToU32(new Vector4(99f, 99f, 104f, 255));
    public static readonly uint OGCDBarSeperator = RGBAToU32(new Vector4(68f, 68f, 75f, 255));
    public static readonly uint FadedText = RGBAToU32(new Vector4(138f, 138f, 138f, 255 / 2f));


    private static uint RGBAToU32(Vector4 rgba)
    {
        return ImGui.ColorConvertFloat4ToU32(rgba * Vector4.One / 255);
    }
}
