using System;
using System.Numerics;
using ImGuiNET;

namespace RotationMaster.Windows.Viewer;

public class RotationViewerConfigSideWindow
{
    public Action<float> OnUIScaleChanged;
    public float UIScale
    {
        get => uiScale;
        set
        {
            uiScale = Math.Clamp(value, 0.5f, 2);
            OnUIScaleChanged?.Invoke(uiScale);
        }
    }

    public RotationViewerConfigSideWindow()
    {
        UIScale = 1;
    }

    public float WindowTransparencyWhenInactive => windowTransparencyWhenInactive;
    public bool HideActionNames => hideActionNames;
    public bool HideUIWhenNotFocused => hideUIWhenNotFocused;

    private bool showConfig = false;
    private float uiScale;
    private bool hideUIWhenNotFocused;
    private bool hideActionNames;
    private float windowTransparencyWhenInactive = 0.8f;
    
    public void Render()
    {
        if (!showConfig)
        {
            return;
        }

        var primaryWindowPos = ImGui.GetWindowPos();
        var primaryWindowSize = ImGui.GetWindowSize();
        var configWindowPos = primaryWindowPos with { X = primaryWindowPos.X + primaryWindowSize.X };

        ImGui.SetNextWindowPos(configWindowPos);
        ImGui.SetNextWindowSizeConstraints(new Vector2(350, 350), new Vector2(700, 700));
        ImGui.Begin("Viewer Config");

        var uiScaleRef = UIScale;
        ImGui.SetNextItemWidth(50f);
        if (ImGui.SliderFloat("UI Scale", ref uiScaleRef, 0.5f, 2f, "", ImGuiSliderFlags.None))
        {
            UIScale = uiScaleRef;
        }

        ImGui.SetNextItemWidth(50f);
        ImGui.SliderFloat("Unfocused transparency", ref windowTransparencyWhenInactive, 0f, 1f, "",
                          ImGuiSliderFlags.None);

        ImGui.Checkbox("Hide UI when not focused", ref hideUIWhenNotFocused);
        ImGui.Checkbox("Hide action names", ref hideActionNames);

        ImGui.End();
    }

    public void ToggleShow()
    {
        showConfig = !showConfig;
    }
}
