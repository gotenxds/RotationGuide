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

    private Func<bool> enablePullActions;

    private WindowFooter footer;

    public RotationBuilderFooter(Func<bool> enablePullActions)
    {
        footer = new WindowFooter("Tools", RenderChild);
        this.enablePullActions = enablePullActions;
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

        ImGui.SameLine();
        if (ImGui.Button("Action"))
        {
            OnAddActionClicked.Invoke();
        }
    }
}
