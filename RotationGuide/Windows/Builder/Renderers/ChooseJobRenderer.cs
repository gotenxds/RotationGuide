using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.GameFonts;
using Dalamud.Logging;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using RotationGuide.Utils;
using Action = System.Action;


namespace RotationGuide.Windows;

public class ChooseJobRenderer : Renderer
{
    private static HashSet<string> ViableJobs = new()
    {
        "PLD", "WAR", "DRK", "GNB",
        "WHM", "SCH", "AST", "SGE",
        "MNK", "DRG", "NIN", "SAM", "RPR",
        "BRD", "MCH", "DNC",
        "BLM", "SMN", "RDM"
    };
    
    private List<ClassJob> tanks = new();
    private List<ClassJob> healers = new();
    private List<ClassJob> RDps = new();
    private List<ClassJob> mDps = new();

    public event Action<ClassJob> OnJobSelected;

    public ChooseJobRenderer()
    {
        foreach (var classJob in Plugin.DataManager.GetExcelSheet<ClassJob>())
        {
            if (ViableJobs.Contains(classJob.Abbreviation))
            {
                switch (classJob.Role)
                {
                    case 1:
                        tanks.Add(classJob);
                        break;
                    case 2:
                        RDps.Add(classJob);
                        break;
                    case 3:
                        mDps.Add(classJob);
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
        ImGui.SetCursorPosX((windowSize.X / 2) - 150);
        ImGui.SetCursorPosY((windowSize.Y / 2) - 200 + BaseCursorHeight);

        ImGui.BeginGroup();
        
        Fonts.WriteWithFont(Fonts.Jupiter23, "Choose a Job");

        RenderGroup(tanks);
        RenderGroup(healers);
        RenderGroup(RDps);
        RenderGroup(mDps);
        
        ImGui.EndGroup();

        StyleTransitionEnd(transition);
    }

    private void RenderGroup(IEnumerable<ClassJob> classJobs)
    {
        ImGui.BeginGroup();
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, 500);
        foreach (var classJob in classJobs)
        {
            var dalamudTextureWrap = Plugin.TextureProvider.GetIcon(classJob.RowId + 100 + 62000u);
            if (ImGui.ImageButton(dalamudTextureWrap.ImGuiHandle,
                                  new Vector2(dalamudTextureWrap.Width, dalamudTextureWrap.Height)))
            {
                OnJobSelected.Invoke(classJob);
            }
        }

        ImGui.EndGroup();
        ImGui.SameLine();
    }
}
