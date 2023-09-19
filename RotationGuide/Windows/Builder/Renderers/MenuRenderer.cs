using System;
using System.Numerics;
using Dalamud.Interface.Animation;
using Dalamud.Logging;
using ImGuiNET;
using RotationGuide.Utils;


namespace RotationGuide.Windows;

public class MenuRenderer : Renderer
{
    public readonly Vector2 buttonSize = new(500, 100);
    public event Action OnGoToCreateChooseJob;


    public override void Render(Transition transition = Transition.None, float time = 0)
    {
        StyleTransitionBegin(transition, time);

        var windowSize = ImGui.GetWindowSize();

        ImGui.SetCursorPosX((windowSize.X / 2) - (buttonSize.X / 2));
        ImGui.SetCursorPosY((windowSize.Y / 2) - (buttonSize.Y / 2) + BaseCursorHeight);

        if (ImGui.Button("CREATE", buttonSize) && transition == Transition.None)
        {
            OnGoToCreateChooseJob.Invoke();
        }

        StyleTransitionEnd(transition);
    }
}
