using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.GameFonts;
using Dalamud.Logging;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using RotationGuide.Data;
using RotationGuide.Utils;
using Action = System.Action;


namespace RotationGuide.Windows;

public class ChooseJobRenderer : Renderer
{

    
    private List<ClassJob> tanks = new();
    private List<ClassJob> healers = new();
    private List<ClassJob> meleeDps = new();
    private List<ClassJob> rangeDps = new();

    public event Action<ClassJob> OnJobSelected;

    public ChooseJobRenderer()
    {
        foreach (var classJob in Plugin.DataManager.GetExcelSheet<ClassJob>())
        {
            if (Job.ViableJobAbbreviation.Contains(classJob.Abbreviation))
            {
                switch (classJob.Role)
                {
                    case 1:
                        tanks.Add(classJob);
                        break;
                    case 2:
                        meleeDps.Add(classJob);
                        break;
                    case 3:
                        rangeDps.Add(classJob);
                        break;
                    case 4:
                        healers.Add(classJob);
                        break;
                }
            }
        }
    }

    public override void Render(Transition transition = Transition.None, float time = 0)
    {
        StyleTransitionBegin(transition, time);

        var windowSize = ImGui.GetWindowSize();
        ImGui.SetCursorPosX((windowSize.X / 2) - 319);
        
        ImGui.BeginGroup();
        
        Fonts.WriteWithFont(Fonts.Jupiter23, "Choose a Job");

        RenderGroup(tanks);
        RenderGroup(healers);
        RenderGroup(meleeDps);
        RenderGroup(rangeDps);
        
        ImGui.EndGroup();

        StyleTransitionEnd(transition);
    }

    private void RenderGroup(IList<ClassJob> classJobs)
    {
        ImGui.BeginGroup();
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, 500);
        for (var index = 0; index < classJobs.Count; index++)
        {
            var classJob = classJobs.ToList()[index];
            var dalamudTextureWrap = Plugin.TextureProvider.GetIcon(classJob.RowId + 100 + 62000u);
            
            if (index == classJobs.Count - 1 && classJobs.Count % 2 != 0)
            {
                ImGui.Indent((ImGui.GetItemRectSize().X / 2) + 4);
            }
            
            if (ImGui.ImageButton(dalamudTextureWrap.ImGuiHandle,
                                  new Vector2(dalamudTextureWrap.Width, dalamudTextureWrap.Height)))
            {
                OnJobSelected.Invoke(classJob);
            }

            if ((index + 1) % 2 != 0)
            {
                ImGui.SameLine(); 
            }
        }
        ImGui.EndGroup();
        
        // ImGui.GetWindowDrawList().AddRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), Colors.FadedText);
        
        ImGui.SetCursorScreenPos(ImGui.GetItemRectMax() - new Vector2(-10, ImGui.GetItemRectSize().Y));
    }
}
