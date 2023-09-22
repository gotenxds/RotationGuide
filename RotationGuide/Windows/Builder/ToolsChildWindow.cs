using System;
using System.Numerics;
using ImGuiNET;
using RotationGuide.Utils;

namespace RotationGuide.Windows;

public class ToolsChildWindow
{
    public event Action OnAddPrePullActionClicked;
    public event Action OnAddActionClicked;
    public event Action OnAddPullBarClicked;

    private Func<bool> enablePullActions;

    public ToolsChildWindow(Func<bool> enablePullActions)
    {
        this.enablePullActions = enablePullActions;
    }

    public void Render()
    {
        Begin();

        RenderTitle();

        RenderButtons();
        
        ActionSearchPopup.Instance.Render();
        End();
    }

    private static void RenderTitle()
    {
        var drawList = ImGui.GetWindowDrawList();

        ImGui.Indent(50);
        ImGui.Text("Tools");

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

    private void RenderButtons()
    {
        if (!enablePullActions())
        {
            ImGui.BeginDisabled();
        }
        if (ImGui.Button("PrePull Action"))
        {
            OnAddPrePullActionClicked.Invoke();
        }
        ImGui.SameLine();
        if (ImGui.Button("Pull Bar"))
        {
            OnAddPullBarClicked.Invoke();
        }
        if (!enablePullActions())
        {
            ImGui.EndDisabled();
        }
        ImGui.SameLine();
        if (ImGui.Button("Action"))
        {
            OnAddActionClicked.Invoke();
        }
    }

    private static void Begin()
    {
        ImGui.SetCursorPosX(10);
        var parentPos = ImGui.GetCursorScreenPos();
        var parentSize = ImGui.GetContentRegionAvail();

        var childSize = parentSize with { X = parentSize.X  -10, Y = 100 };
        var childPos = parentPos with { X = parentPos.X + ImGui.GetScrollX(), Y = parentPos.Y + parentSize.Y - childSize.Y };

        ImGui.SetNextWindowPos(childPos);

        ImGui.BeginChild("BuilderTools", childSize);
    }
    
    private static void End()
    {
        ImGui.EndChild();
    }
}
