using System;
using System.Numerics;
using ImGuiNET;

namespace RotationMaster.Windows;

public class WindowFooter
{
    private Action childrenRenderer;
    private string title;
    public Vector2 Size { get; private set; }
    
    public WindowFooter(string title, Action childrenRenderer)
    {
        this.title = title;
        this.childrenRenderer = childrenRenderer;
        Size = Vector2.One;
    }

    public void Render()
    {
        Begin();

        RenderTitle();

        childrenRenderer.Invoke();
        
        End();
    }

    private void RenderTitle()
    {
        var drawList = ImGui.GetWindowDrawList();

        ImGui.Indent(50);
        ImGui.Text(title);

        var titleRectMax = ImGui.GetItemRectMax();
        var titleRectMin = ImGui.GetItemRectMin();
        var titleSize = titleRectMax - titleRectMin;
        var titleHeightOffsetLeft = (titleSize.Y / 2) + 2;
        var titleHeightOffsetRight = -((titleSize.Y / 2) - 2);
        var fromTitleToEndOfScreen = titleRectMax with { X = titleRectMax.X + ImGui.GetWindowSize().X } +
                                     new Vector2(0, titleHeightOffsetRight);
        var fromTitleToStartOfScreen =
            new Vector2(titleRectMin.X, ImGui.GetWindowPos().Y) + new Vector2(-5, titleHeightOffsetLeft);


        drawList.AddLine(titleRectMax + new Vector2(5, titleHeightOffsetRight), fromTitleToEndOfScreen,
                         ImGui.GetColorU32(ImGuiCol.Separator));
        drawList.AddLine(ImGui.GetWindowPos() + new Vector2(0, titleHeightOffsetLeft), fromTitleToStartOfScreen,
                         ImGui.GetColorU32(ImGuiCol.Separator));
        ImGui.Unindent();
    }

    private void Begin()
    {
        ImGui.SetCursorPosX(10);
        var parentPos = ImGui.GetCursorScreenPos();
        var parentSize = ImGui.GetContentRegionAvail();

        Size = parentSize with { X = parentSize.X  -10, Y = 100 };
        var childPos = parentPos with { X = parentPos.X + ImGui.GetScrollX(), Y = parentPos.Y + parentSize.Y - Size.Y };
        
        
        ImGui.SetNextWindowPos(childPos);
        ImGui.BeginChild("BuilderTools", Size);
    }
    
    private static void End()
    {
        ImGui.EndChild();
    }
}
