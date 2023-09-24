using ImGuiNET;

namespace RotationGuide.Windows;

public static class CreateRotationDialog
{
    private const string DialogName = "CREATE_ROTATION";
    public static void Render()
    {
        var isOpenRef = true;

        if (ImGui.BeginPopupModal(DialogName))
        {
            ImGui.BeginCombo("Job", "");
            
            ImGui.EndCombo();
         
            ImGui.EndPopup();
        }
    }
}
