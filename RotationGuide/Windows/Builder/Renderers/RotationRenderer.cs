using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Logging;
using ImGuiNET;
using RotationGuide.Data;
using RotationGuide.Utils;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace RotationGuide.Windows;

public enum ActionType
{
    GCD,
    OGCD,
    PREPULL
}

public struct ActionClickEventArgs
{
    public int Index;
    public ActionType Type;
}

public class RotationRenderer : Renderer
{
    public Rotation Rotation { get; set; }
    public event Action<ActionClickEventArgs> OnActionClick;

    private Dictionary<uint, Action> actionsById;
    private int currentlyEditingActionTime = -1;
    private bool shouldFocusActionTimeEditor = false;

    public RotationRenderer()
    {
        actionsById = Plugin.DataManager.GetExcelSheet<Action>().ToDictionary(a => a.RowId);
    }

    public override void Render(Transition transition = Transition.None, float time = 0)
    {
        var drawList = ImGui.GetWindowDrawList();

        for (var index = 0; index < Rotation.Nodes.Length; index++)
        {
            var rotationNode = Rotation.Nodes[index];
            switch (rotationNode)
            {
                case GCDActionNode actionNode:
                {
                    RenderGCDAction(actionNode, index, drawList);
                    break;
                }
                case OGCDActionNode ogcdActionNode:
                {
                    RenderOGCDAction(ogcdActionNode, index, drawList);
                    break;
                }
                case PrePullActionNode prePullActionNode:
                    RenderPrepullAction(prePullActionNode, index, drawList);
                    break;
                case PullIndicatorNode pullIndicatorNode:
                    ImGuiExt.IndentV(Plugin.PullBarImage.Height * -0.25f);
                    ImGui.Image(Plugin.PullBarImage.ImGuiHandle,
                                new Vector2(Plugin.PullBarImage.Width, Plugin.PullBarImage.Height));
                    break;
            }

            ImGui.SameLine();
        }
    }

    private IntPtr GetIconHandle(IActionNode actionNode)
    {
        return Plugin.TextureProvider.GetIcon(actionsById[actionNode.Id].Icon).ImGuiHandle;
    }

    private void RenderOGCDAction(OGCDActionNode actionNode, int index, ImDrawListPtr drawList)
    {
        RenderAction(actionNode, index, drawList);
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

            
            ImGui.SetCursorPos(new Vector2(cursorPos.X, cursorPos.Y - (64 * 2)));
            
            ImGui.SetKeyboardFocusHere();
            var actionNodeTime = actionNode.Time;
            ImGui.SetNextItemWidth(164);
            ImGui.PushID(index);
            if (ImGui.InputInt("", ref actionNodeTime, 15))
            {
                PluginLog.Debug(actionNodeTime.ToString());
                actionNode.Time = actionNodeTime;
                Rotation.ReplaceActionNode(index, actionNode);
            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGui.IsItemHovered())
            {
                currentlyEditingActionTime = -1;
            }
            
            ImGui.PopID();
        }
        else
        {
            ImGui.SetCursorPos(new Vector2(cursorPos.X + 16, cursorPos.Y - (64 * 2)));
            ImGui.Text(actionNode.Time.ToString());

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
            OGCDActionNode => (32, ActionType.OGCD),
            PrePullActionNode => (64, ActionType.PREPULL),
            GCDActionNode => (64, ActionType.GCD),
            _ => throw new ArgumentException()
        };

        ImGui.ImageButton(GetIconHandle(actionNode), Vector2.One * size);

        if (ImGui.IsItemClicked())
        {
            OnActionClick.Invoke(new ActionClickEventArgs() { Index = index, Type = type });
        }

        drawList.AddRect(ImGui.GetItemRectMin() - (Vector2.One * 2), ImGui.GetItemRectMax() + (Vector2.One * 2),
                         ImGui.GetColorU32(ImGuiCol.NavWindowingHighlight), 10, ImDrawFlags.None, 4);
    }
}
