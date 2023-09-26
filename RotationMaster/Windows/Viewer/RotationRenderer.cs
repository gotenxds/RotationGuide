using System;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using RotationMaster.Data;
using RotationMaster.Utils;

namespace RotationMaster.Windows.Viewer;

public class RotationRenderer
{
    protected virtual int ActionBaseIconSize => 64;

    protected float UIScale
    {
        get => LastUiScale;
        set
        {
            var last = LastUiScale;
            LastUiScale = value;
            
            if (Math.Abs(last - value) > 0.001f)
            {
                RecalculateUiSizes();    
            }   
        }
    }

    protected bool HideActionNames;
    protected float ActionIconSize;
    protected float OGCDActionIconSize;
    protected float ActionMargin;
    protected float GCDActionFontSize;
    protected float OGCDActionFontSize;
    protected Vector2 PullBarSize;
    protected Vector2 OGCDBarSize;
    protected Vector2 PrepullTimeOffset;
    protected float LastUiScale;
    public RotationRenderer()
    {
        UIScale = 1;
    }

    public virtual void Render(Rotation rotation, float uiScale, bool hideActionNames = false)
    {
        UIScale = uiScale;
        HideActionNames = hideActionNames;
        
        RenderRotation(rotation);
    }

    protected virtual void RecalculateUiSizes()
    {
        ActionIconSize = ActionBaseIconSize * UIScale;
        OGCDActionIconSize = ActionIconSize * 0.8f;

        GCDActionFontSize = 32 * UIScale;
        OGCDActionFontSize = 24 * UIScale;

        ActionMargin = ActionIconSize * 0.6f;

        PullBarSize = new Vector2(Images.PullBarImage.Width, Images.PullBarImage.Height) * UIScale;
        OGCDBarSize = new Vector2(Images.OGCDBarImage.Width, Images.OGCDBarImage.Height) * UIScale;
        PrepullTimeOffset = new Vector2(ActionBaseIconSize * (0.25f * UIScale), -GCDActionFontSize);
    }

    protected virtual void RenderRotation(Rotation rotation)
    {
        var drawList = ImGui.GetWindowDrawList();
        var originalItemSpacingX = ImGui.GetStyle().ItemSpacing.X;

        // We are handling our own spacing here.
        ImGui.GetStyle().ItemSpacing.X = 0;

        ImGui.BeginGroup();

        for (var index = 0; index < rotation.Nodes.Length; index++)
        {
            var rotationNode = rotation.Nodes[index];
            switch (rotationNode)
            {
                case GCDActionNode actionNode:
                {
                    RenderGCDAction(drawList, actionNode, index);
                    ImGui.SameLine(0, ActionMargin);
                    break;
                }
                case OGCDActionsNode ogcdActionNode:
                {
                    RenderOGCDActions(drawList, ogcdActionNode, index);
                    ImGui.SameLine(0);
                    break;
                }
                case PrePullActionNode prePullActionNode:
                    RenderPrepullAction(drawList, prePullActionNode, index);
                    ImGui.SameLine(0, ActionMargin);
                    break;
                case PullIndicatorNode pullIndicatorNode:
                    RenderPullIndicator(drawList);

                    ImGui.SameLine(0, ActionMargin);
                    break;
            }
        }

        ImGui.EndGroup();

        ImGui.GetStyle().ItemSpacing.X = originalItemSpacingX;
    }

    protected virtual void RenderGCDAction(ImDrawListPtr drawList, GCDActionNode actionNode, int index)
    {
        RenderAction(drawList, actionNode, index);
    }

    protected virtual void RenderPrepullAction(ImDrawListPtr drawList, PrePullActionNode actionNode, int index)
    {
        ImGui.BeginGroup();
        RenderAction(drawList, actionNode, index);


        var cursorPos = ImGui.GetItemRectMin();

        var text = $"-{actionNode.Time}s";
        var textPosition = cursorPos + PrepullTimeOffset;

        drawList.AddText(UiBuilder.DefaultFont, GCDActionFontSize, textPosition, ImGui.GetColorU32(Vector4.One), text);

        ImGui.EndGroup();
    }

    protected virtual void RenderAction(ImDrawListPtr drawList, IActionNode actionNode, int index)
    {
        ImGui.Image(FFAction.GetIconHandle(actionNode.Id, true), Vector2.One * ActionIconSize);
        var rectMin = ImGui.GetItemRectMin();
        var rectMax = ImGui.GetItemRectMax();

        drawList.AddImage(Images.ActionIconBorderImage.ImGuiHandle, rectMin, rectMax);

        RenderActionName(drawList, rectMax, rectMin, actionNode.Id, GCDActionFontSize, 5);
    }

    protected virtual void RenderOGCDActions(ImDrawListPtr drawList, OGCDActionsNode ogcdaNode, int index)
    {
        ImGui.SameLine(0, -ActionMargin);
        ImGuiExt.IndentV((ActionIconSize / 2) - (OGCDBarSize.Y / 2));
        ImGui.Image(Images.OGCDBarImage.ImGuiHandle, OGCDBarSize);
        var itemRectMin = ImGui.GetItemRectMin();
        var itemRectMax = ImGui.GetItemRectMax();

        var centerPoint = itemRectMax - (OGCDBarSize * 0.5f);

        var yOffset = 10 * UIScale;

        var centerActionImageMin = new Vector2(centerPoint.X - (OGCDActionIconSize / 2), itemRectMax.Y + yOffset);
        var firstActionImageMin = new Vector2(centerPoint.X + (OGCDActionIconSize * -1.5f),
                                              itemRectMin.Y - OGCDActionIconSize - yOffset);
        var lastActionImageMin = firstActionImageMin + new Vector2(2 * OGCDActionIconSize, 0);

        for (var innerIndex = 0; innerIndex < ogcdaNode.Ids.Length; innerIndex++)
        {
            var actionId = ogcdaNode.Ids[innerIndex];

            if (FFAction.TryById(actionId, out _))
            {
                var imageMin = innerIndex == 0 ? firstActionImageMin
                               : innerIndex == 1 ? centerActionImageMin
                               : lastActionImageMin;

                var imageMax = imageMin + new Vector2(OGCDActionIconSize, OGCDActionIconSize);

                drawList.AddImage(FFAction.GetIconHandle(actionId), imageMin, imageMax);
                drawList.AddImage(Images.ActionIconBorderImage.ImGuiHandle, imageMin, imageMax);

                RenderActionName(drawList, imageMax, imageMin, actionId, OGCDActionFontSize, 15, innerIndex != 1);
            }
        }
    }

    protected virtual void RenderActionName(
        ImDrawListPtr drawList, Vector2 imageMax, Vector2 imageMin, uint actionId, float fontSize, float xOffset,
        bool reverse = false)
    {
        if (HideActionNames)
        {
            return;
        }

        var rectWidth = imageMax - imageMin;
        var rectCenter = imageMin + (rectWidth / 2);

        var split = FFAction.ById(actionId).SplitActionName();

        for (var i = 0; i < split.Length; i++)
        {
            var name = split[i];
            var textSize = ImGui.CalcTextSize(name) * UIScale;
            var textYPosition = reverse ? imageMin.Y - (fontSize * (split.Length - i)) : imageMax.Y + (fontSize * i);

            var textPosition = new Vector2(rectCenter.X - (textSize.X / 2) + (xOffset * UIScale), textYPosition);

            drawList.AddText(UiBuilder.DefaultFont, fontSize, textPosition,
                             ImGui.GetColorU32(ImGuiCol.Text),
                             name);
        }
    }

    protected virtual void RenderPullIndicator(ImDrawListPtr drawList)
    {
        ImGuiExt.IndentV(PullBarSize.Y * -0.25f);
        ImGui.Image(Images.PullBarImage.ImGuiHandle, PullBarSize);

        var pullBarRectMin = ImGui.GetItemRectMin();
        var textPosition = new Vector2(pullBarRectMin.X - (25 * UIScale), pullBarRectMin.Y - (35 * UIScale));

        drawList.AddText(UiBuilder.DefaultFont, GCDActionFontSize, textPosition, ImGui.GetColorU32(Vector4.One),
                         "PULL");
    }
}
