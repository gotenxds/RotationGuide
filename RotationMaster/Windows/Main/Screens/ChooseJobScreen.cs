using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.GameFonts;
using Dalamud.Logging;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using RotationMaster.Data;
using RotationMaster.Utils;
using Action = System.Action;


namespace RotationMaster.Windows;

public class ChooseJobScreen : Screen
{
    private List<ClassJob> tanks = new();
    private List<ClassJob> healers = new();
    private List<ClassJob> meleeDps = new();
    private List<ClassJob> physicalRangeDps = new();
    private List<ClassJob> magicalRangeDps = new();

    public event Action<ClassJob> OnJobSelected;

    public ChooseJobScreen()
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
                        if (classJob.ClassJobCategory.Value!.Name.ToString().Contains("War"))
                        {
                            physicalRangeDps.Add(classJob);
                        }
                        else
                        {
                            magicalRangeDps.Add(classJob);
                        }

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
        ImGui.Indent(100);

        ImGui.BeginGroup();

        Fonts.WriteWithFont(Fonts.Jupiter23, "Choose a Job");

        ImGuiExt.IndentV(50);
        RenderGroup(tanks);
        RenderGroup(healers);
        RenderGroup(meleeDps);
        RenderGroup(magicalRangeDps, true);
        RenderGroup(physicalRangeDps, true);

        ImGui.EndGroup();

        StyleTransitionEnd(transition);
    }

    private void RenderGroup(IList<ClassJob> classJobs, bool canWrap = false)
    {
        if (canWrap && ImGuiExt.IsOverflowing(new Vector2(300, 0)))
        {
            ImGui.NewLine();
        }
        
        var imDrawListPtr = ImGui.GetWindowDrawList();
        ImGui.BeginGroup();
        foreach (var classJob in classJobs)
        {
            var dalamudTextureWrap = classJob.GetIcon();

            ImGui.BeginGroup();
            var cursorPos = ImGui.GetCursorPosY();
            ImGui.Dummy(new Vector2(260, 64));
            var dummyMinPlusPadding = ImGui.GetItemRectMin() - new Vector2(9, 0);
            var dummyMaxPlusPadding = ImGui.GetItemRectMax() + new Vector2(19, 0);
            
            if (ImGui.IsItemHovered())
            {
                imDrawListPtr.AddRectFilled(dummyMinPlusPadding, dummyMaxPlusPadding, ImGui.GetColorU32(ImGuiCol.ButtonHovered));
            }

            if (ImGui.IsItemClicked())
            {
                OnJobSelected.Invoke(classJob);
            }

            ImGui.SetCursorPosY(cursorPos);
            ImGui.Image(dalamudTextureWrap.ImGuiHandle, new Vector2(64, 64));
            ImGui.SameLine();
            ImGuiExt.IndentV(14);
            ImGui.Text(classJob.Name.RawString.Capitalize());
            ImGui.EndGroup();
        }

        ImGui.EndGroup();


        var padding = (Vector2.One * 10);
        var itemRectMin = ImGui.GetItemRectMin() - padding;
        var itemRectMax = ImGui.GetItemRectMax() + padding with { X = padding.X * 2 };
        var rectWidth = itemRectMax.X - itemRectMin.X;

        imDrawListPtr.AddRect(itemRectMin, itemRectMax, Colors.FadedText, 10);

        var imgSize = Vector2.One * 64;
        var imgStartPoint = itemRectMin + new Vector2((rectWidth - imgSize.X) / 2, imgSize.Y * -0.5f);
        var imgEndPoint = imgStartPoint + imgSize;

        imDrawListPtr.AddImage(classJobs.First().GetRoleIcon()!.ImGuiHandle, imgStartPoint, imgEndPoint);

        ImGui.SameLine(0, 50);
    }
}
