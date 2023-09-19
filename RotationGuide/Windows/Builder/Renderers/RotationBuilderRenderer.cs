using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Logging;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using RotationGuide.Data;
using RotationGuide.Utils;

namespace RotationGuide.Windows;

public class RotationPageRenderer : Renderer
{
    public ClassJob Job
    {
        get => job;
        set
        {
            rotation.Reset();
            job = value;
            actionSearchPopup.Job = value;
        }
    }

    private ActionSearchPopup actionSearchPopup = new();
    private RotationRenderer rotationRenderer = new();
    private bool renderActionButtons = false;
    private ActionType nextActionToCreate;
    private int actionIndexToReplace = -1;
    private ClassJob job;
    private Rotation rotation;

    public RotationPageRenderer()
    {
        rotation = new Rotation();
        rotationRenderer.Rotation = rotation;

        rotationRenderer.OnActionClick += actionClickEventArgs =>
        {
            nextActionToCreate = actionClickEventArgs.Type;
            actionIndexToReplace = actionClickEventArgs.Index;
            actionSearchPopup.Open();
        };
        
        actionSearchPopup.OnActionSelected += a =>
        {
            IActionNode actionNode = nextActionToCreate switch
            {
                ActionType.GCD => new GCDActionNode(),
                ActionType.OGCD => new GCDActionNode(),
                ActionType.PREPULL => new PrePullActionNode(),
                _ => new OGCDActionNode()
            };
            
            actionNode.Id = a.RowId;

            if (actionIndexToReplace != -1)
            {
                rotation.ReplaceActionNode(actionIndexToReplace, actionNode);
                actionIndexToReplace = -1;
            }
            else
            {
                rotation.AddAction(actionNode);
            }
        };
    }

    public override void Render(Transition transition = Transition.None, float time = 0)
    {
        if (Job == null)
        {
            return;
        }

        ImGui.SetCursorPosY(100 + BaseCursorHeight);

        ImGui.BeginGroup();

        RenderTitle();

        ImGuiExt.IndentV(300);
        ImGui.Indent(50);

        RenderRotation();

        RenderActionButtons();

        ImGui.EndGroup();

        actionSearchPopup.Render();

        CheckForGeneralClicks();
    }

    private void RenderRotation()
    {
        rotationRenderer.Render();

        RenderAddButton();
    }

    private void CheckForGeneralClicks()
    {
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGui.IsItemHovered())
        {
            renderActionButtons = false;
        }
    }

    private void RenderAddButton()
    {
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString(), Vector2.One * 64))
        {
            renderActionButtons = true;
        }

        ImGui.PopFont();
    }

    private void RenderActionButtons()
    {
        if (!renderActionButtons)
        {
            return;
        }

        var newActionButtonSize = new Vector2(100, 100);
        var pullIndicatorButtonSize = new Vector2(200, 100);
        var newOGCDButtonSize = new Vector2(100, 100);
        var newPrepullActionButtonSize = new Vector2(100, 100);

        ImGui.SetCursorPosX(
            ImGuiExt.CalculateCenterXWith(newActionButtonSize.X + pullIndicatorButtonSize.X + newOGCDButtonSize.X));

        ImGui.BeginGroup();

        if (ImGui.Button("Action", newActionButtonSize))
        {
            nextActionToCreate = ActionType.GCD;
            renderActionButtons = false;
            actionSearchPopup.Open();
        }

        ImGui.SameLine();
        if (ImGui.Button("OGCD", newOGCDButtonSize))
        {
            nextActionToCreate = ActionType.OGCD;
            renderActionButtons = false;
            actionSearchPopup.Open();
        }

        ImGui.SameLine();
        if (!rotation.HasPullIndicator)
        {
            if (ImGui.Button("Pull Indicator", pullIndicatorButtonSize))
            {
                rotation.AddPullIndicator();
                renderActionButtons = false;
            }
            
            if (ImGui.Button("Prepull Action", pullIndicatorButtonSize))
            {
                nextActionToCreate = ActionType.PREPULL;
                renderActionButtons = false;
                actionSearchPopup.Open();
            }
        }

        ImGui.EndGroup();
    }

    private void RenderTitle()
    {
        var jobTextureWrap = Plugin.TextureProvider.GetIcon(Job.RowId + 100 + 62000u);

        ImGui.BeginGroup();

        ImGui.Image(jobTextureWrap!.ImGuiHandle, new Vector2(jobTextureWrap.Width, jobTextureWrap.Height));
        ImGui.SameLine();
        Fonts.WriteWithFont(Fonts.Jupiter23, Job.Name);

        ImGui.EndGroup();
    }
}
