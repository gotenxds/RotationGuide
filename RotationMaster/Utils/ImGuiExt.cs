using System.Numerics;
using ImGuiNET;

namespace RotationMaster.Utils;

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

    public static bool IsOverflowing(Vector2 cursorPosOffset)
    {
        var cursorPos = ImGui.GetCursorPos() + cursorPosOffset;
        var windowSize = ImGui.GetWindowSize();

        return cursorPos.X < 0 || cursorPos.Y < 0 || cursorPos.X > windowSize.X || cursorPos.Y > windowSize.Y;
    }

    public static bool IsMenuHovered()
    {
        return ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
    }
}
