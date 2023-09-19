using System.Numerics;
using ImGuiNET;

namespace RotationGuide.Utils;

public static class DrawUtils
{
    public static Vector2 ToWindowPositions(Vector2 position)
    {
        var winPosition = ImGui.GetWindowPos();

        return new Vector2(winPosition.X + position.X, winPosition.Y + position.Y);
    }
    
    public static void DrawLine(
        ImDrawListPtr drawListPtr, Vector2 startPosition, Vector2 endPosition, Vector4 color, float thickness = 1f)
    {
        drawListPtr.AddLine(ToWindowPositions(startPosition), ToWindowPositions(endPosition), ImGui.ColorConvertFloat4ToU32(color), thickness);
    }

    public static void DrawRect(
        ImDrawListPtr drawListPtr, Vector2 startPosition, Vector2 dimension, Vector4 color, float rounding = 0f,
        float thickness = 1f)
    {
        var (min, max) = CalcRectMinMax(startPosition, dimension);

        drawListPtr.AddRect(min, max, ImGui.ColorConvertFloat4ToU32(color), rounding, ImDrawFlags.None, thickness);
    }

    public static void DrawRectFilled(
        ImDrawListPtr drawListPtr, Vector2 startPosition, Vector2 dimension, Vector4 color, Vector4? borderColor = null,
        int borderThickness = 1)
    {
        var (min, max) = CalcRectMinMax(startPosition, dimension);

        var imDrawListPtr = ImGui.GetWindowDrawList();
        imDrawListPtr.AddRectFilled(min, max, ImGui.ColorConvertFloat4ToU32(color));

        if (borderColor.HasValue)
        {
            var borderStartPosition = startPosition with { X = startPosition.X + borderThickness };
            var borderDim = dimension with { X = dimension.X - borderThickness };
            
            DrawRect(drawListPtr, borderStartPosition, borderDim, borderColor.Value, thickness: borderThickness);
        }
    }

    private static (Vector2 min, Vector2 max) CalcRectMinMax(Vector2 startPosition, Vector2 dimension)
    {
        var min = ToWindowPositions(startPosition);
        var max = ToWindowPositions(new Vector2(startPosition.X + dimension.X, startPosition.Y + dimension.Y));

        return (min, max);
    }
}
