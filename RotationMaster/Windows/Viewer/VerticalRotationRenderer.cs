using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using RotationMaster.Data;
using RotationMaster.Utils;

namespace RotationMaster.Windows.Viewer;

public class VerticalRotationRenderer : RotationRenderer
{
    protected override int ActionBaseIconSize => 40;

    protected override void RecalculateUiSizes()
    {
        base.RecalculateUiSizes();

        ActionMargin = ActionIconSize * 0.2f;
    }

    /*
     * Currently not used
     * Not fully implemented
     */
    protected override void RenderRotation(Rotation rotation)
    {
        var drawList = ImGui.GetWindowDrawList();

        for (var index = 0; index < rotation.Nodes.Length; index++)
        {
            var rotationNode = rotation.Nodes[index];
            switch (rotationNode)
            {
                case GCDActionNode actionNode:
                {
                    RenderGCDAction(drawList, actionNode, index);
                    ImGuiExt.IndentV(ActionMargin);
                    break;
                }
                case OGCDActionsNode ogcdActionNode:
                {
                    RenderOGCDActions(drawList, ogcdActionNode, index);
                    break;
                }
                case PrePullActionNode prePullActionNode:
                    RenderPrepullAction(drawList, prePullActionNode, index);
                    break;
                case PullIndicatorNode pullIndicatorNode:
                    RenderPullIndicator(drawList);
                    break;
            }
        }

        ImGui.EndGroup();
    }

    protected override void RenderPrepullAction(ImDrawListPtr drawList, PrePullActionNode actionNode, int index)
    {
        RenderAction(drawList, actionNode, index);

        drawList.AddText(UiBuilder.DefaultFont, GCDActionFontSize, ImGui.GetItemRectMax(),
                         ImGui.GetColorU32(ImGuiCol.Text),
                         $"(-{actionNode.Time})");
    }

    protected override void RenderOGCDActions(ImDrawListPtr drawList, OGCDActionsNode ogcdaNode, int index) { }

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

        ImGui.Dummy(PullBarSize.Transpose() * 2);
        var dummySize = ImGui.GetItemRectMax() - ImGui.GetItemRectMin();
        var dummyCenter = ImGui.GetItemRectMax() - (dummySize / 2);

        var position = new Vector2(lastActionStartPosition.X + (PullBarSize.Y / 2),
                                   dummyCenter.Y - (PullBarSize.X / 2));

        DrawUtils.RenderRotatedImage(Images.PullBarImage, 90f, PullBarSize, position);
    }
}
