using System;
using ImGuiNET;

namespace RotationMaster.Windows;

public class RotationBuilderFooter
{
    public event Action OnAddPrePullActionClicked;
    public event Action OnAddActionClicked;
    public event Action OnAddPullBarClicked;
    public event Action OnStartRecording;
    public event Action OnStopRecording;

    private Func<bool> enablePullActions;
    private Func<bool> isRecording;

    private WindowFooter footer;

    public RotationBuilderFooter(Func<bool> enablePullActions, Func<bool> isRecording)
    {
        footer = new WindowFooter("Tools", RenderChild);
        this.enablePullActions = enablePullActions;
        this.isRecording = isRecording;
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
        var recording = isRecording();
        
        if (recording)
        {
            ImGui.BeginDisabled();
        }
        RenderPullButtons();

        ImGui.SameLine();
        if (ImGui.Button("Action"))
        {
            OnAddActionClicked.Invoke();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "An action can be a GCD or OGCD action, if followed by another action an OGCD bar will be created.");
        }

        if (recording)
        {
            ImGui.EndDisabled();
        }

        ImGui.SameLine();
        if (ImGui.Button( recording ? "Stop" : "Record"))
        {
            (recording ? OnStopRecording : OnStartRecording)?.Invoke();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "Record your actions in game and add them to the rotation.");
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

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Pre pull actions have a negative time indicator that count down to the pull.");
        }

        if (!enablePullActions() && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.EndDisabled();
            ImGui.SetTooltip("Cant add pre pull action after pull bar");
            ImGui.BeginDisabled();
        }

        ImGui.SameLine();
        if (ImGui.Button("Pull Bar"))
        {
            OnAddPullBarClicked.Invoke();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Adds a pull indicator to the rotation bar.");
        }

        if (!enablePullActions() && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.EndDisabled();
            ImGui.SetTooltip("Pull bar already placed");
            ImGui.BeginDisabled();
        }

        if (!enablePullActions())
        {
            ImGui.EndDisabled();
        }
    }
}
