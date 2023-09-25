using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using RotationMaster.Data;
using RotationMaster.Utils;

namespace RotationMaster.Windows.Viewer;

public class VerticalRotationRenderer : RotationRenderer
{
    protected override int ActionBaseIconSize => 40;

    public VerticalRotationRenderer(RotationViewerConfigSideWindow config) : base(config) { }

    protected override void RecalculateUiSizes()
    {
        base.RecalculateUiSizes();

        actionMargin = actionIconSize * 0.2f;
    }

    /*
     * Currently not used
     * Not fully implemented
     */
    protected override void RenderRotation(Rotation rotation)
    {
        var drawList = ImGui.GetWindowDrawList();

        foreach (var rotationNode in rotation.Nodes)
        {
            switch (rotationNode)
            {
                case GCDActionNode actionNode:
                {
                    RenderGCDAction(actionNode, drawList);
                    ImGuiExt.IndentV(actionMargin);
                    break;
                }
                case OGCDActionsNode ogcdActionNode:
                {
                    RenderOGCDActions(ogcdActionNode, drawList);
                    break;
                }
                case PrePullActionNode prePullActionNode:
                    RenderPrepullAction(prePullActionNode, drawList);
                    break;
                case PullIndicatorNode pullIndicatorNode:
                    RenderPullIndicator(drawList);
                    break;
            }
        }

        ImGui.EndGroup();
    }

    protected override void RenderPrepullAction(PrePullActionNode actionNode, ImDrawListPtr drawList)
    {
        RenderAction(actionNode, drawList);

        drawList.AddText(UiBuilder.DefaultFont, gcdActionFontSize, ImGui.GetItemRectMax(),
                         ImGui.GetColorU32(ImGuiCol.Text),
                         $"(-{actionNode.Time})");
    }

    protected override void RenderOGCDActions(OGCDActionsNode ogcdaNode, ImDrawListPtr drawList) { }

    protected override void RenderActionName(
        ImDrawListPtr drawList, Vector2 imageMax, Vector2 imageMin, uint actionId, float fontSize, float xOffset,
        bool reverse = false)
    {
        var text = FFAction.ById(actionId).Name;

        var textPosition = new Vector2(imageMax.X + xOffset, imageMin.Y + (2 * UIScale));

        drawList.AddText(UiBuilder.DefaultFont, fontSize, textPosition,
                         ImGui.GetColorU32(ImGuiCol.Text),
                         text);
    }

    protected override void RenderPullIndicator(ImDrawListPtr drawList)
    {
        var lastActionStartPosition = ImGui.GetItemRectMin();

        ImGui.Dummy(pullBarSize.Transpose() * 2);
        var dummySize = ImGui.GetItemRectMax() - ImGui.GetItemRectMin();
        var dummyCenter = ImGui.GetItemRectMax() - (dummySize / 2);

        var position = new Vector2(lastActionStartPosition.X + (pullBarSize.Y / 2),
                                   dummyCenter.Y - (pullBarSize.X / 2));

        DrawUtils.RenderRotatedImage(Images.PullBarImage, 90f, pullBarSize, position);
    }
}
