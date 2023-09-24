using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using Lumina.Text;
using RotationMaster.Data;
using RotationMaster.Utils;
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

public class RotationRenderer : Renderer
{
    private const int ActionIconSize = 64;
    private const int ActionMargin = ActionIconSize / 2;
    public Rotation Rotation { get; set; }
    public event Action<ActionClickEventArgs> OnActionClick;
    public event Action<OGCDClickEventArgs> OnOGCDClick;

    private Dictionary<uint, Action> actionsById;
    private int currentlyEditingActionTime = -1;
    private bool shouldFocusActionTimeEditor = false;

    public bool IsEditing => currentlyEditingActionTime != -1;

    public RotationRenderer()
    {
        actionsById = Plugin.DataManager.GetExcelSheet<Action>().ToDictionary(a => a.RowId);
    }

    public override void Render(Transition transition = Transition.None, float time = 0)
    {
        var drawList = ImGui.GetWindowDrawList();
        var columnHeight = 3 * ActionIconSize;
        var startingPosition = ImGui.GetCursorPos();
                 
        ImGui.BeginGroup();
        for (var index = 0; index < Rotation.Nodes.Length; index++)
        {
            if (ImGuiExt.IsOverflowing(new Vector2(200, 0)) && ImGui.GetContentRegionAvail().Y > 450)
            {
                // This logic "closes" the row and forces starting a new one, warping the the rotation 
                ImGui.EndGroup();
                ImGui.BeginGroup();
                ImGui.SetCursorPos(startingPosition with { Y = startingPosition.Y + columnHeight });
                startingPosition = ImGui.GetCursorPos();
            }
            
            var rotationNode = Rotation.Nodes[index];
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
                    ImGui.SameLine(0, 34);
                    break;
                }
                case PrePullActionNode prePullActionNode:
                    RenderPrepullAction(prePullActionNode, index, drawList);
                    ImGui.SameLine(0, ActionMargin);
                    break;
                case PullIndicatorNode pullIndicatorNode:
                    RenderPullIndicator(drawList);

                    ImGui.SameLine(0, ActionMargin);
                    break;
            }
        }
        ImGui.EndGroup();
    }

    private IntPtr GetIconHandle(uint actionId)
    {
        return Plugin.TextureProvider.GetIcon(actionsById[actionId].Icon).ImGuiHandle;
    }

    private void RenderOGCDActions(OGCDActionsNode actionsNode, int index, ImDrawListPtr drawList)
    {
        const float ogcdIconSize = ActionIconSize * .8f;
        const float halfOgcdSize = ogcdIconSize * 0.5f;
        const int rectBorderSize = 8;
        const int offsetY = 16;
        var barSize = new Vector2(ogcdIconSize * 3.5f, ActionIconSize);
        
        var barRectMin = ImGui.GetCursorScreenPos() - new Vector2(12, 0);
        var barRectMax = barRectMin + barSize;

        var outerRectMin = new Vector2(barRectMin.X - (ActionMargin / 2f), barRectMin.Y + (ActionIconSize * 0.25f));
        var outerRectMax = new Vector2(barRectMax.X + (ActionMargin * 0.5f), barRectMax.Y - (ActionIconSize * 0.15f));

        var innerRectMin = outerRectMin + (Vector2.One * rectBorderSize);
        var innerRectMax = outerRectMax - (Vector2.One * rectBorderSize);

        var outerRectSize = outerRectMax - outerRectMin;
        var innerRectSize = innerRectMax - innerRectMin;
        var separatorPointA = innerRectMin with { X = innerRectMin.X + (innerRectSize.X * 0.5f) };
        var separatorPointB = separatorPointA with { Y = innerRectMax.Y - 1 };

        var middleButtonPos = separatorPointA + new Vector2(-halfOgcdSize + 1.5f, halfOgcdSize + offsetY);
        var leftButtonPos = new Vector2(middleButtonPos.X - (innerRectSize.X * 0.25f),
                                        outerRectMin.Y - outerRectSize.Y - halfOgcdSize);
        var rightButtonPos = leftButtonPos + new Vector2(innerRectSize.X * 0.5f, 0);
        var buttonPositions = new[] { leftButtonPos, middleButtonPos, rightButtonPos };


        ImGui.BeginGroup();

        for (var innerIndex = 0; innerIndex < actionsNode.Ids.Length; innerIndex++)
        {
            var actionsNodeId = actionsNode.Ids[innerIndex];

            ImGui.PushID($"ogcdButton-{index}-{innerIndex}");
            ImGui.SetCursorScreenPos(buttonPositions[innerIndex]);
            if (actionsById.TryGetValue(actionsNodeId, out var action))
            {
                ImGui.ImageButton(GetIconHandle(actionsNodeId), Vector2.One * ogcdIconSize);
                var split = SplitActionName(action.Name);

                var buttonMax = ImGui.GetItemRectMax();
                var buttonCenter = buttonMax.X - (ogcdIconSize / 2);

                for (var i = 0; i < split.Length; i++)
                {
                    var name = split[i];
                    var textSize = ImGui.CalcTextSize(name);

                    var textYPos = innerIndex == 1
                                       ? buttonMax.Y + (20 * i)
                                       : buttonMax.Y - ogcdIconSize - (15 * (split.Length - i)) - 15;

                    drawList.AddText(UiBuilder.DefaultFont, 24,
                                     new Vector2(buttonCenter - (textSize.X / 2) + 8, textYPos),
                                     ImGui.GetColorU32(ImGuiCol.Text), name);
                }
            }
            else
            {
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.Button(FontAwesomeIcon.Plus.ToIconString(), Vector2.One * ogcdIconSize);
                ImGui.PopFont();
            }

            ImGui.PopID();

            if (ImGui.IsItemClicked())
            {
                OnOGCDClick.Invoke(new OGCDClickEventArgs() { Index = index, InnerIndex = innerIndex });
            }
        }


        ImGui.EndGroup();

        drawList.AddRectFilled(outerRectMin, outerRectMax, Colors.OGCDBarBorder, 3);
        drawList.AddRectFilled(innerRectMin, innerRectMax, Colors.OGCDBarInner);
        drawList.AddLine(separatorPointA, separatorPointB, Colors.OGCDBarSeperator, rectBorderSize);
    }

    private void RenderGCDAction(GCDActionNode actionNode, int index, ImDrawListPtr drawList)
    {
        RenderAction(actionNode, index, drawList);
    }

    private void RenderPrepullAction(PrePullActionNode actionNode, int index, ImDrawListPtr drawList)
    {
        ImGui.BeginGroup();
        RenderAction(actionNode, index, drawList);

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

    private void RenderAction(IActionNode actionNode, int index, ImDrawListPtr drawList)
    {
        var (size, type) = actionNode switch
        {
            PrePullActionNode => (actionIconSize: ActionIconSize, ActionType.PREPULL),
            GCDActionNode => (actionIconSize: ActionIconSize, ActionType.GCD),
            _ => throw new ArgumentException()
        };

        ImGui.ImageButton(GetIconHandle(actionNode.Id), Vector2.One * size);

        if (ImGui.IsItemClicked())
        {
            OnActionClick.Invoke(new ActionClickEventArgs() { Index = index, Type = type });
        }

        var borderRectMin = ImGui.GetItemRectMin() - (Vector2.One * 2);
        var borderRectMax = ImGui.GetItemRectMax() + (Vector2.One * 2);
        drawList.AddRect(borderRectMin, borderRectMax, ImGui.GetColorU32(ImGuiCol.NavWindowingHighlight), 10,
                         ImDrawFlags.None, 4);

        var borderRectWidth = borderRectMax.X - borderRectMin.X;
        var borderRectCenter = borderRectMin.X + borderRectWidth / 2;

        var actionName = actionsById[actionNode.Id].Name;

        var split = SplitActionName(actionName);

        for (var i = 0; i < split.Length; i++)
        {
            var name = split[i];
            var textSize = ImGui.CalcTextSize(name);
            drawList.AddText(new Vector2(borderRectCenter - (textSize.X / 2), borderRectMax.Y + (25 * i)),
                             ImGui.GetColorU32(ImGuiCol.Text), name);
        }
    }

    private static string[] SplitActionName(SeString actionName)
    {
        var split = actionName.RawString.Split(" ").SelectMany(part =>
        {
            const int max = 8;
            const int sliceAmount = max - 1;
            if (part.Length <= max)
            {
                return new List<string>() { part };
            }

            return new List<string>() { $"{part[..(sliceAmount)]}-", part[(sliceAmount)..] };
        }).ToArray();

        return split;
    }

    private void RenderPullIndicator(ImDrawListPtr drawList)
    {
        ImGuiExt.IndentV(Plugin.PullBarImage.Height * -0.25f);
        ImGui.Image(Plugin.PullBarImage.ImGuiHandle,
                    new Vector2(Plugin.PullBarImage.Width, Plugin.PullBarImage.Height));

        var pullBarRectMin = ImGui.GetItemRectMin();
        drawList.AddText(new Vector2(pullBarRectMin.X - 25, pullBarRectMin.Y - 35), ImGui.GetColorU32(Vector4.One),
                         "PULL");
    }
}
