using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;

namespace RotationMaster.Utils;

public class ImGuiExt
{
    public static void IndentV(float indent)
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + indent);
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

    /**
     * This function will attempt to calculate the size of the text while taking into consideration
     * 1. Dalamud global scale
     * 2. Our own scale if provided
     * 3. The font if provided
     * 3. The custom font size (that might be different from the font's default size) if provided
     *
     * The custom font size needs to be unscaled, that is, for example, not 32 * scale, but 32.
     **/
    public static Vector2 CalcTextSize(string text, float scale = 1f, ImFontPtr? font = null, float fontSize = -1)
    {
        var fontSizeMultiplier = 1f;
        
        if (font.HasValue)
        {
            fontSizeMultiplier =  fontSize < 0 ? 1f : fontSize / font.Value.FontSize;
            
            ImGui.PushFont(font.Value);
        }

        var textSize = ImGui.CalcTextSize(text) / ImGuiHelpers.GlobalScale * scale * fontSizeMultiplier;
        
        if (font.HasValue)
        {
            ImGui.PopFont();
        }

        return textSize;
    }
}
