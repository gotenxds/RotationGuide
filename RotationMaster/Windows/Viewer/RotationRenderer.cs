using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using RotationMaster.Data;
using RotationMaster.Utils;

namespace RotationMaster.Windows.Viewer;

public class RotationRenderer
{
    protected virtual int ActionBaseIconSize => 64;

    protected float actionIconSize;
    protected float ogcdActionIconSize;
    protected float actionMargin;
    protected float gcdActionFontSize;
    protected float ogcdActionFontSize;
    protected Vector2 pullBarSize;
    protected Vector2 ogcdBarSize;
    protected Vector2 prepullTimeOffset;

    protected readonly RotationViewerConfigSideWindow config;
    protected float UIScale => config.UIScale;

    public RotationRenderer(RotationViewerConfigSideWindow config)
    {
        this.config = config;
        config.OnUIScaleChanged += _ => RecalculateUiSizes();

        RecalculateUiSizes();
    }

    public virtual void Render(Rotation rotation)
    {
        RenderRotation(rotation);
    }

    protected virtual void RecalculateUiSizes()
    {
        actionIconSize = ActionBaseIconSize * UIScale;
        ogcdActionIconSize = actionIconSize * 0.8f;

        gcdActionFontSize = 32 * UIScale;
        ogcdActionFontSize = 24 * UIScale;

        actionMargin = actionIconSize * 0.6f;

        pullBarSize = new Vector2(Images.PullBarImage.Width, Images.PullBarImage.Height) * UIScale;
        ogcdBarSize = new Vector2(Images.OGCDBarImage.Width, Images.OGCDBarImage.Height) * UIScale;
        prepullTimeOffset = new Vector2(ActionBaseIconSize * (0.25f * UIScale), -gcdActionFontSize);
    }

    protected virtual void RenderRotation(Rotation rotation)
    {
        var drawList = ImGui.GetWindowDrawList();
        var originalItemSpacingX = ImGui.GetStyle().ItemSpacing.X;

        // We are handling our own spacing here.
        ImGui.GetStyle().ItemSpacing.X = 0;

        ImGui.BeginGroup();

        foreach (var rotationNode in rotation.Nodes)
        {
            switch (rotationNode)
            {
                case GCDActionNode actionNode:
                {
                    RenderGCDAction(actionNode, drawList);
                    ImGui.SameLine(0, actionMargin);
                    break;
                }
                case OGCDActionsNode ogcdActionNode:
                {
                    RenderOGCDActions(ogcdActionNode, drawList);
                    ImGui.SameLine(0);
                    break;
                }
                case PrePullActionNode prePullActionNode:
                    RenderPrepullAction(prePullActionNode, drawList);
                    ImGui.SameLine(0, actionMargin);
                    break;
                case PullIndicatorNode pullIndicatorNode:
                    RenderPullIndicator(drawList);

                    ImGui.SameLine(0, actionMargin);
                    break;
            }
        }

        ImGui.EndGroup();

        ImGui.GetStyle().ItemSpacing.X = originalItemSpacingX;
    }

    protected virtual void RenderGCDAction(Data.GCDActionNode actionNode, ImDrawListPtr drawList)
    {
        RenderAction(actionNode, drawList);
    }

    protected virtual void RenderPrepullAction(PrePullActionNode actionNode, ImDrawListPtr drawList)
    {
        ImGui.BeginGroup();
        RenderAction(actionNode, drawList);


        var cursorPos = ImGui.GetItemRectMin();

        var text = $"-{actionNode.Time}s";
        var textPosition = cursorPos + prepullTimeOffset;

        drawList.AddText(UiBuilder.DefaultFont, gcdActionFontSize, textPosition, ImGui.GetColorU32(Vector4.One), text);

        ImGui.EndGroup();
    }

    protected virtual void RenderAction(IActionNode actionNode, ImDrawListPtr drawList)
    {
        ImGui.Image(FFAction.GetIconHandle(actionNode.Id, true), Vector2.One * actionIconSize);
        var rectMin = ImGui.GetItemRectMin();
        var rectMax = ImGui.GetItemRectMax();

        drawList.AddImage(Images.ActionIconBorderImage.ImGuiHandle, rectMin, rectMax);

        RenderActionName(drawList, rectMax, rectMin, actionNode.Id, gcdActionFontSize, 5);
    }

    protected virtual void RenderOGCDActions(OGCDActionsNode ogcdaNode, ImDrawListPtr drawList)
    {
        ImGui.SameLine(0, -actionMargin);
        ImGuiExt.IndentV((actionIconSize / 2) - (ogcdBarSize.Y / 2));
        ImGui.Image(Images.OGCDBarImage.ImGuiHandle, ogcdBarSize);
        var itemRectMin = ImGui.GetItemRectMin();
        var itemRectMax = ImGui.GetItemRectMax();

        var centerPoint = itemRectMax - (ogcdBarSize * 0.5f);

        var yOffset = 10 * UIScale;

        var centerActionImageMin = new Vector2(centerPoint.X - (ogcdActionIconSize / 2), itemRectMax.Y + yOffset);
        var firstActionImageMin = new Vector2(centerPoint.X + (ogcdActionIconSize * -1.5f),
                                              itemRectMin.Y - ogcdActionIconSize - yOffset);
        var lastActionImageMin = firstActionImageMin + new Vector2(2 * ogcdActionIconSize, 0);

        for (var index = 0; index < ogcdaNode.Ids.Length; index++)
        {
            var actionId = ogcdaNode.Ids[index];

            if (FFAction.TryById(actionId, out _))
            {
                var imageMin = index == 0 ? firstActionImageMin
                               : index == 1 ? centerActionImageMin
                               : lastActionImageMin;

                var imageMax = imageMin + new Vector2(ogcdActionIconSize, ogcdActionIconSize);

                drawList.AddImage(FFAction.GetIconHandle(actionId), imageMin, imageMax);
                drawList.AddImage(Images.ActionIconBorderImage.ImGuiHandle, imageMin, imageMax);

                RenderActionName(drawList, imageMax, imageMin, actionId, ogcdActionFontSize, 15, index != 1);
            }
        }
    }

    protected virtual void RenderActionName(
        ImDrawListPtr drawList, Vector2 imageMax, Vector2 imageMin, uint actionId, float fontSize, float xOffset,
        bool reverse = false)
    {
        if (config.HideActionNames)
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
        ImGuiExt.IndentV(pullBarSize.Y * -0.25f);
        ImGui.Image(Images.PullBarImage.ImGuiHandle, pullBarSize);

        var pullBarRectMin = ImGui.GetItemRectMin();
        var textPosition = new Vector2(pullBarRectMin.X - (25 * UIScale), pullBarRectMin.Y - (35 * UIScale));

        drawList.AddText(UiBuilder.DefaultFont, gcdActionFontSize, textPosition, ImGui.GetColorU32(Vector4.One),
                         "PULL");
    }
}
