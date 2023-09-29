using System.Numerics;
using ImGuiNET;

namespace RotationMaster.Utils;

public static class Colors
{
    public static readonly uint FadedText = RGBAToU32(new Vector4(138f, 138f, 138f, 255 / 2f));
    public static readonly uint Recording = RGBAToU32(new Vector4(254, 35, 36, 255));


    private static uint RGBAToU32(Vector4 rgba)
    {
        return ImGui.ColorConvertFloat4ToU32(rgba * Vector4.One / 255);
    }
}
