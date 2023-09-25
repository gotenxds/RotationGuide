using System;
using System.Numerics;
using ImGuiNET;
using ImGuiScene;

namespace RotationMaster.Utils;

public static class DrawUtils
{
    public static Vector2 ToWindowPositions(Vector2 position)
    {
        var winPosition = ImGui.GetWindowPos();

        return new Vector2(winPosition.X + position.X, winPosition.Y + position.Y);
    }
    
    public static Vector2 ToScreenSpace(Vector2 position)
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

    public static void RenderRotatedImage(TextureWrap imageTexture, float rotationAngleDegrees, Vector2 size, Vector2 position)
    {
        var angleRadians = MathF.PI * rotationAngleDegrees / 180.0f;
        var cos = MathF.Cos(angleRadians);
        var sin = MathF.Sin(angleRadians);
        
        var rotationMatrix = new Matrix3x2(
            cos, -sin,
            sin, cos,
            0.0f, 0.0f
        );
        
        var vertices = new Vector2[]
        {
            new(-size.X / 2, -size.Y / 2), // Top-left
            new(size.X / 2, -size.Y / 2),  // Top-right
            new(size.X / 2, size.Y / 2),   // Bottom-right
            new(-size.X / 2, size.Y / 2),  // Bottom-left
        };
        
        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i] = Vector2.Transform(vertices[i], rotationMatrix) + position;
        }
        
        ImGui.GetWindowDrawList().AddImageQuad(
            imageTexture.ImGuiHandle,
            vertices[0],
            vertices[1],
            vertices[2],
            vertices[3]
        );
    }
}
