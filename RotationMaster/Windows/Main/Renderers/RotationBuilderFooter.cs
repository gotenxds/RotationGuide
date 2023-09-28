using System;
using System.Numerics;
using Dalamud.Logging;
using ImGuiNET;

namespace RotationMaster.Windows;

public class RotationBuilderFooter
{
    public event Action OnAddPrePullActionClicked;
    public event Action OnAddActionClicked;
    public event Action OnAddPullBarClicked;
    public event Action OnDeleteLastItem;

    private Func<bool> enablePullActions;
    private Func<bool> isEmpty;

    private WindowFooter footer;

    public RotationBuilderFooter(Func<bool> enablePullActions, Func<bool> isEmpty)
    {
        footer = new WindowFooter("Tools", RenderChild);
        this.enablePullActions = enablePullActions;
        this.isEmpty = isEmpty;
    }

    public void Render()
    {
        footer.Render();
    }

    private void RenderChild()
    {
        RenderButtons();
        
        ActionSearchDialog.Instance.Render();
    }

    private void RenderButtons()
    {
        RenderPullButtons();
        
        ImGui.SameLine();
        if (ImGui.Button("Action"))
        {
            OnAddActionClicked.Invoke();
        }
    }
    
    private void RenderPullButtons()
    {
        if (!enablePullActions())
        {
            ImGui.BeginDisabled();
        }

        
        if (ImGui.Button("PrePull Action"))
        {
            OnAddPrePullActionClicked.Invoke();
        }

        if (!enablePullActions() && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.BeginTooltip();
            ImGui.Text("Cant add pre pull action after pull bar");
            ImGui.EndTooltip();
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Pull Bar"))
        {
            OnAddPullBarClicked.Invoke();
        }
        
        if (!enablePullActions() && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.BeginTooltip();
            ImGui.Text("Pull bar already placed");
            ImGui.EndTooltip();
        }
        
        if (!enablePullActions())
        {
            ImGui.EndDisabled();
        }
    }
}
