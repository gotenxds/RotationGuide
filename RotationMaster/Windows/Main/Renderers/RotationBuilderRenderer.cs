using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Logging;
using ImGuiNET;
using Lumina.Text;
using RotationMaster.Data;
using RotationMaster.Utils;
using RotationMaster.Windows.Viewer;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace RotationMaster.Windows;

public enum ActionType
{
    GCD,
    PREPULL
}

public struct ActionClickEventArgs
{
    public int Index;
    public ActionType Type;
}

public struct OGCDClickEventArgs
{
    public int Index;
    public int InnerIndex;
}

public class RotationBuilderRenderer : RotationRenderer
{
    public RotationBuilderRenderer()
    {
        contextMenu = new RotationBuilderContextMenu();
    }

    public event Action<ActionClickEventArgs> OnActionClick;
    public event Action<OGCDClickEventArgs> OnOGCDClick;

    private int currentlyEditingActionTime = -1;
    private bool shouldFocusActionTimeEditor = false;
    private Rotation Rotation;
    private RotationBuilderContextMenu contextMenu;

    public override void Render(Rotation rotation, float uiScale, bool hideActionNames = false)
    {
        base.Render(rotation, uiScale, hideActionNames);

        contextMenu.Render(rotation);
    }

    protected override void RenderRotation(Rotation rotation)
    {
        Rotation = rotation;
        var drawList = ImGui.GetWindowDrawList();
        var columnHeight = 5 * ActionIconSize;
        var startingPosition = ImGui.GetCursorPos();

        var originalItemSpacingX = ImGui.GetStyle().ItemSpacing.X;

        // We are handling our own spacing here.
        ImGui.GetStyle().ItemSpacing.X = 0;

        ImGui.BeginGroup();
        for (var index = 0; index < rotation.Nodes.Length; index++)
        {
            PluginLog.Debug($"Rendering node {ImGui.GetContentRegionAvail().Y}");
            if (ImGuiExt.IsOverflowing(new Vector2(100, 0)) && ImGui.GetContentRegionAvail().Y > columnHeight * 1.8f)
            {
                // This logic "closes" the row and forces starting a new one, warping the the rotation 
                ImGui.EndGroup();
                ImGui.BeginGroup();
                ImGui.SetCursorPos(startingPosition with { Y = startingPosition.Y + columnHeight });
                startingPosition = ImGui.GetCursorPos();
            }

            var rotationNode = rotation.Nodes[index];
            switch (rotationNode)
            {
                case GCDActionNode actionNode:
                {
                    RenderGCDAction(actionNode, index, drawList);
                    ImGui.SameLine(0, ActionMargin);
                    break;
                }
                case OGCDActionsNode ogcdActionNode:
                {
                    RenderOGCDActions(ogcdActionNode, index, drawList);
                    break;
                }
                case PrePullActionNode prePullActionNode:
                    RenderPrepullAction(prePullActionNode, index, drawList);
                    ImGui.SameLine(0, ActionMargin);
                    break;
                case PullIndicatorNode pullIndicatorNode:
                    RenderPullIndicator(drawList, pullIndicatorNode, index);

                    ImGui.SameLine(0, ActionMargin);
                    break;
            }
        }

        ImGui.EndGroup();

        ImGui.GetStyle().ItemSpacing.X = originalItemSpacingX;
    }

    private void RenderOGCDActions(OGCDActionsNode actionsNode, int index, ImDrawListPtr drawList)
    {
        var cursorScreenPositionToReturnTo = ImGui.GetCursorScreenPos();

        var renderingOnANewLine = cursorScreenPositionToReturnTo.X < ImGui.GetItemRectMax().X;

        if (!renderingOnANewLine)
        {
            ImGui.SameLine(0, 3);
        }

        ImGuiExt.IndentV((ActionIconSize / 2) - (OGCDBarSize.Y / 2));
        ImGui.Image(Images.OGCDBarImage.ImGuiHandle, OGCDBarSize);
        var itemRectMin = ImGui.GetItemRectMin();
        var itemRectMax = ImGui.GetItemRectMax();

        var centerPoint = itemRectMax - (OGCDBarSize * 0.5f);

        var topButtonsYOffset = 16 * UIScale;
        var centerButtonYOffset = 10 * UIScale;

        var centerActionImageMin =
            new Vector2(centerPoint.X - (OGCDActionIconSize / 2), itemRectMax.Y + centerButtonYOffset);
        var firstActionImageMin = new Vector2(centerPoint.X + (OGCDActionIconSize * -1.5f),
                                              itemRectMin.Y - OGCDActionIconSize - topButtonsYOffset);
        var lastActionImageMin = firstActionImageMin + new Vector2(2 * OGCDActionIconSize, 0);

        for (var innerIndex = 0; innerIndex < actionsNode.Ids.Length; innerIndex++)
        {
            var actionId = actionsNode.Ids[innerIndex];
            var imageMin = innerIndex == 0 ? firstActionImageMin
                           : innerIndex == 1 ? centerActionImageMin
                           : lastActionImageMin;

            ImGui.PushID($"ogcdButton-{index}-{innerIndex}");
            ImGui.SetCursorScreenPos(imageMin);

            if (FFAction.TryById(actionId, out _))
            {
                ImGui.ImageButton(FFAction.GetIconHandle(actionId), OGCDActionIconSize * Vector2.One);

                var borderColor = ImGui.GetColorU32(ImGuiCol.NavWindowingHighlight);

                if (ImGui.IsItemHovered())
                {
                    borderColor = ImGui.GetColorU32(ImGuiCol.ButtonHovered);
                }

                var (borderRectMin, borderRectMax) =
                    DrawBorderAroundAction(drawList, ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), borderColor);

                RenderActionName(drawList, borderRectMax, borderRectMin, actionId, OGCDActionFontSize, 0,
                                 innerIndex != 1);
            }
            else
            {
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.Button(FontAwesomeIcon.Plus.ToIconString(), OGCDActionIconSize * Vector2.One);
                ImGui.PopFont();
            }

            ImGui.PopID();

            if (ImGui.IsItemClicked())
            {
                OnOGCDClick.Invoke(new OGCDClickEventArgs() { Index = index, InnerIndex = innerIndex });
            }
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                PluginLog.Debug("click");
                contextMenu.Open(actionsNode, index, innerIndex);
            }
        }

        ImGui.SetCursorScreenPos(new Vector2(itemRectMax.X + 2, cursorScreenPositionToReturnTo.Y));
    }

    private static (Vector2 min, Vector2 max) DrawBorderAroundAction(
        ImDrawListPtr drawList, Vector2 imageMin, Vector2 imageMax, uint color)
    {
        var borderRectMin = imageMin - (Vector2.One * 2);
        var borderRectMax = imageMax + (Vector2.One * 2);
        drawList.AddRect(borderRectMin, borderRectMax, color, 10,
                         ImDrawFlags.None, 4);

        return (borderRectMin, borderRectMax);
    }

    private void RenderGCDAction(GCDActionNode actionNode, int index, ImDrawListPtr drawList)
    {
        RenderAction(drawList, actionNode, index);
    }

    private void RenderPrepullAction(PrePullActionNode actionNode, int index, ImDrawListPtr drawList)
    {
        ImGui.BeginGroup();
        RenderAction(drawList, actionNode, index);

        var cursorPos = ImGui.GetCursorPos();

        if (currentlyEditingActionTime == index)
        {
            ImGui.SetCursorPosY(cursorPos.Y - (ActionIconSize * 2));

            var actionNodeTime = actionNode.Time;
            ImGui.SetNextItemWidth(164);
            if (ImGui.InputInt($"##{index}", ref actionNodeTime, 1, 5))
            {
                actionNode.Time = actionNodeTime;
                Rotation.ReplaceActionNode(index, actionNode);
            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGui.IsItemHovered())
            {
                currentlyEditingActionTime = -1;
            }
        }
        else
        {
            ImGui.SetCursorPos(new Vector2(cursorPos.X + 25, cursorPos.Y - (ActionIconSize * 2)));
            ImGui.Text($"{actionNode.Time}s");

            if (ImGui.IsItemClicked())
            {
                currentlyEditingActionTime = index;
                shouldFocusActionTimeEditor = true;
            }
        }

        ImGui.SetCursorPos(cursorPos);
        ImGui.EndGroup();
    }

    protected override void RenderAction(ImDrawListPtr drawList, IActionNode actionNode, int index)
    {
        var type = actionNode switch
        {
            PrePullActionNode => ActionType.PREPULL,
            GCDActionNode => ActionType.GCD,
            _ => throw new ArgumentException()
        };

        ImGui.ImageButton(FFAction.GetIconHandle(actionNode.Id), Vector2.One * ActionIconSize);

        if (ImGui.IsItemClicked())
        {
            OnActionClick.Invoke(new ActionClickEventArgs() { Index = index, Type = type });
        }

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            contextMenu.Open(actionNode, index);
        }

        var borderColor = ImGui.GetColorU32(ImGuiCol.NavWindowingHighlight);

        if (ImGui.IsItemHovered())
        {
            borderColor = ImGui.GetColorU32(ImGuiCol.ButtonHovered);
        }

        var (borderRectMin, borderRectMax) =
            DrawBorderAroundAction(drawList, ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), borderColor);

        RenderActionName(drawList, borderRectMax, borderRectMin, actionNode.Id, GCDActionFontSize, 0);
    }

    protected override void RenderPullIndicator(ImDrawListPtr drawList, PullIndicatorNode node, int index)
    {
        ImGuiExt.IndentV(Images.PullBarImage.Height * -0.25f);
        var imagePosition = new Vector2(Images.PullBarImage.Width, Images.PullBarImage.Height);
        ImGui.Image(Images.PullBarImage.ImGuiHandle, imagePosition, Vector2.Zero, Vector2.One);

        var pullBarRectMin = ImGui.GetItemRectMin();
        drawList.AddText(new Vector2(pullBarRectMin.X - 25, pullBarRectMin.Y - 35), ImGui.GetColorU32(Vector4.One),
                         "PULL");
        
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            contextMenu.Open(node, index);
        }
    }
}
