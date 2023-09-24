using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Animation;
using Dalamud.Logging;
using ImGuiNET;
using RotationMaster.Services;
using RotationMaster.Utils;


namespace RotationMaster.Windows;

public class MenuRenderer : Renderer
{ 
    public event Action OnGoToCreateChooseJob;
    private RotationListRenderer rotationListRenderer = new();

    public override void Render(Transition transition = Transition.None, float time = 0)
    {
        StyleTransitionBegin(transition, time);

        if (RotationDataService.GetAll().Any())
        {
            rotationListRenderer.Render();
        }
        else
        {
            var windowSize = ImGui.GetWindowSize();

            var windowPos = ImGui.GetWindowPos();
            var windowCenter = windowPos + (windowSize / 2) - new Vector2(350, 75);
            var createButtonPos = windowCenter + new Vector2(236, 50);
            var orUseAnPos = createButtonPos + new Vector2(100, 0);
            var importButtonPos = createButtonPos + new Vector2(140, 0);

            var imDrawListPtr = ImGui.GetWindowDrawList();
            
            imDrawListPtr.AddText(Fonts.Axis32.ImFont, 44, windowCenter, Colors.FadedText, "You dont seem to have any rotations yet.");
            ImGui.SetCursorScreenPos(createButtonPos);
            if (ImGui.Button("Create"))
            {
                OnGoToCreateChooseJob.Invoke();
            }
            imDrawListPtr.AddText(Fonts.Axis32.ImFont, 44, orUseAnPos, Colors.FadedText, "/");
            ImGui.SetCursorScreenPos(importButtonPos);
            ImGui.Button("Import");
        }

        

        StyleTransitionEnd(transition);
    }
}
