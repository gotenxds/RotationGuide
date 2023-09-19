using System.Numerics;
using ImGuiNET;

namespace RotationGuide.Utils;

public class ImGuiExt
{
    public static void IndentV(float indent)
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + indent);
    }

    public static float CalculateCenterXWith(float itemWidth)
    {
        var windowSize = ImGui.GetWindowSize();

        return (windowSize.X / 2) - (itemWidth / 2);
    }
}
