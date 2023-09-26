using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;
using RotationMaster.Data;
using RotationMaster.Utils;

namespace RotationMaster.Windows.Viewer;

public class RotationViewerWindow : Window
{
    private readonly RotationViewerConfigSideWindow configSideWindow;
    private readonly RotationRenderer horizontalRotationRenderer;
    private bool locked;
    private bool IsActive => ImGui.IsWindowHovered() || ImGui.IsWindowFocused();
    public Rotation? Rotation { get; set; }

    private bool Locked
    {
        get => locked;
        set
        {
            if (value)
            {
                Flags |= ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
            }
            else
            {
                Flags ^= ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
            }

            locked = value;
        }
    }

    private float UIScale => configSideWindow.UIScale;

    public RotationViewerWindow() : base("Rotation Viewer", ImGuiWindowFlags.NoTitleBar)
    {
        Size = new Vector2(600, 100);
        SizeCondition = ImGuiCond.FirstUseEver;

        Position = new Vector2(1500, 295);
        PositionCondition = ImGuiCond.FirstUseEver;

        configSideWindow = new RotationViewerConfigSideWindow();
        horizontalRotationRenderer = new RotationRenderer();
    }

    public override void Draw()
    {
        if (Rotation == null)
        {
            IsOpen = false;
            return;
        }

        BgAlpha = IsActive ? 1 : configSideWindow.WindowTransparencyWhenInactive;

        RenderHeader();
        configSideWindow.Render();

        ImGui.Indent(20);
        ImGuiExt.IndentV(100 * UIScale);

        horizontalRotationRenderer.Render(Rotation, UIScale, configSideWindow.HideActionNames);
    }

    private void RenderHeader()
    {
        if (configSideWindow.HideUIWhenNotFocused && !IsActive)
        {
            return;
        }

        ImGui.BeginGroup();

        ImGui.Text(Rotation!.Name);

        ImGui.SameLine(0, ImGui.GetContentRegionAvail().X - 150);

        ImGui.PushFont(UiBuilder.IconFont);

        ImGui.PushID("rotation_viewer_lock");
        if (ImGui.Button(( Locked ? FontAwesomeIcon.Lock : FontAwesomeIcon.Unlock).ToIconString(), Vector2.One * 40))
        {
            Locked = !Locked;
        }

        ImGui.PopID();

        ImGui.SameLine();

        ImGui.PushID($"rotation_viewer_config");
        if (ImGui.Button(FontAwesomeIcon.Cog.ToIconString(), Vector2.One * 40))
        {
            configSideWindow.ToggleShow();
        }

        ImGui.PopID();

        ImGui.PopFont();

        ImGui.EndGroup();
    }
}
