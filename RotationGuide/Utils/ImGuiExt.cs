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

    public static bool IsOverflowing(Vector2 cursorPosOffset)
    {
        var cursorPos = ImGui.GetCursorPos() + cursorPosOffset;
        var windowSize = ImGui.GetWindowSize();
        
        return cursorPos.X < 0 || cursorPos.Y < 0 || cursorPos.X > windowSize.X || cursorPos.Y > windowSize.Y;
    }
    
    // This will include the header and resizeButton 
    public static bool IsMouseHoverWindow()
    {
        var windowStart = ImGui.GetWindowPos();
        var headerSize = ImGui.GetWindowSize() with { Y = ImGui.GetWindowContentRegionMin().Y };
        
        return IsBoundedBy(ImGui.GetMousePos(), windowStart - headerSize, windowStart + headerSize);
    }

  public static bool IsBoundedBy(Vector2 cursor, Vector2 minBounds, Vector2 maxBounds)
    {
        if (cursor.X >= minBounds.X && cursor.Y >= minBounds.Y)
        {
            if (cursor.X <= maxBounds.X && cursor.Y <= maxBounds.Y)
            {
                return true;
            }
        }

        return false;
    }
    
}
