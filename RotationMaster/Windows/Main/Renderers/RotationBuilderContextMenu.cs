using System;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Logging;
using ImGuiNET;
using RotationMaster.Data;
using RotationMaster.Utils;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace RotationMaster.Windows;

public class RotationBuilderContextMenu
{
    private const string PopupName = "rotation_builder_context_menu";
    private Rotation currentRotation;
    private IRotationNode currentNode;
    private int currentIndex;
    private int currentInnerIndex;
    private bool awaitingToStartSelection;
    private RotationMasterActionType typeToFilterSelectionBy;
    private Action<Task<Action>> onSelectionFinish;

    public void Render(Rotation rotation)
    {
        if (ImGui.BeginPopup(PopupName))
        {
            currentRotation = rotation;

            switch (currentNode)
            {
                case GCDActionNode actionNode:
                    RenderGCDOptions();
                    break;
                case OGCDActionsNode ogcdActionNode:
                    RenderOGCDOptions();
                    break;
                case PrePullActionNode prePullActionNode:
                    RenderPrePullOptions();
                    break;
                case PullIndicatorNode pullIndicatorNode:
                    RenderPullBarOptions();
                    break;
            }

            ImGui.EndPopup();
        }

        // This is a workaround to make the dialog pop regardless of where we clicked in the popup hierarchy 
        if (awaitingToStartSelection)
        {
            ActionSearchDialog.Instance.Open(typeToFilterSelectionBy).ContinueWith(onSelectionFinish);
            awaitingToStartSelection = false;
        }
        // ActionSearchDialog.Instance.Render();
    }

    public void Open(IRotationNode node, int index, int innerIndex = -1)
    {
        currentNode = node;
        currentIndex = index;
        currentInnerIndex = innerIndex;

        ImGui.OpenPopup(PopupName);
    }

    private void StartSelection(Action<Task<Action>> onFinish, RotationMasterActionType actionType = RotationMasterActionType.NA)
    {
        typeToFilterSelectionBy = actionType;
        awaitingToStartSelection = true;
        onSelectionFinish = onFinish;
    } 

    private void RenderGCDOptions()
    {
        RenderDeleteOption();
        RenderAddBeforeGCDActionOptions();
        RenderAddAfterGCDActionOptions();
    }

    private void RenderOGCDOptions()
    {
        RenderRemoveSelectionOption();
    }

    private void RenderPrePullOptions()
    {
        RenderDeleteOption();
        RenderAddBeforePrePullActionOptions();
        RenderAddAfterPrePullActionOptions();
    }

    private void RenderPullBarOptions()
    {
        RenderDeleteOption();
        RenderAddBeforePullBarOptions();
        RenderAddAfterPullBarOptions();
    }

    private void RenderDeleteOption()
    {
        var lastNodeIndex = currentRotation.Nodes.Length - 1;
        var isLastNode = currentIndex == lastNodeIndex;

        var isGCDNodeWithDependency = (!isLastNode && currentNode is GCDActionNode &&
                                       currentRotation.Nodes[currentIndex + 1] is OGCDActionsNode);
        var disabled = isGCDNodeWithDependency;

        if (disabled)
        {
            ImGui.BeginDisabled();
        }

        if (ImGui.MenuItem("Remove action"))
        {
            currentRotation.RemoveNode(currentIndex);
        }

        if (disabled)
        {
            ImGui.EndDisabled();

            if (ImGuiExt.IsMenuHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Can only remove last GCD");
                ImGui.EndTooltip();
            }
        }
    }

    private void RenderRemoveSelectionOption()
    {
        var ogcdNode = (OGCDActionsNode)currentNode;
        var currentActionId = ogcdNode.Ids[currentInnerIndex];

        var actionExists = FFAction.Exists(currentActionId);

        if (!actionExists)
        {
            ImGui.BeginDisabled();
        }

        if (ImGui.MenuItem("Remove selection"))
        {
            currentRotation.UpdateOgcdNode(currentIndex, currentInnerIndex, uint.MaxValue);
        }

        if (!actionExists)
        {
            ImGui.EndDisabled();
        }
    }

    private void RenderAddBeforePullBarOptions()
    {
        if (ImGui.BeginMenu("Add Before"))
        {
            if (ImGui.MenuItem("Pre pull action"))
            {
                StartSelection(task =>
                {
                    currentRotation.InsertNode(
                        new PrePullActionNode
                            { Id = task.Result.RowId }, currentIndex);
                });
            }

            if (ImGui.MenuItem("Action"))
            {
                StartSelection(task =>
                {
                    currentRotation.InsertNode(
                        new GCDActionNode { Id = task.Result.RowId },
                        currentIndex);
                });
            }

            ImGui.EndMenu();
        }
    }

    private void RenderAddAfterPullBarOptions()
    {
        if (ImGui.BeginMenu("Add After"))
        {
            if (ImGui.MenuItem("Action"))
            {
                StartSelection(task =>
                {
                    currentRotation.InsertNode(
                        new GCDActionNode { Id = task.Result.RowId },
                        currentIndex + 1);
                });
            }

            ImGui.EndMenu();
        }
    }

    private void RenderAddBeforePrePullActionOptions()
    {
        if (ImGui.BeginMenu("Add Before"))
        {
            if (ImGui.MenuItem("Pre pull action"))
            {
                StartSelection(task =>
                {
                    currentRotation.InsertNode(
                        new PrePullActionNode
                            { Id = task.Result.RowId }, currentIndex);
                });
            }

            if (ImGui.MenuItem("Action"))
            {
                currentRotation.InsertNode(new GCDActionNode(), currentIndex);
            }

            ImGui.EndMenu();
        }
    }

    private void RenderAddAfterPrePullActionOptions()
    {
        var index = currentIndex + 1;
        if (ImGui.BeginMenu("Add After"))
        {
            RenderAddPullIndicator(index);

            if (ImGui.MenuItem("Pre pull action"))
            {
                StartSelection(task =>
                {
                    currentRotation.InsertNode(
                        new PrePullActionNode
                            { Id = task.Result.RowId }, index);
                });
            }

            if (ImGui.MenuItem("Action"))
            {
                StartSelection(task =>
                {
                    currentRotation.InsertNode(
                        new GCDActionNode { Id = task.Result.RowId },
                        index);
                });
            }

            ImGui.EndMenu();
        }
    }

    private void RenderAddPullIndicator(int index)
    {
        var hasIndicator = currentRotation.HasPullIndicator;
        var hasPrePullActionsAfter =
            currentRotation.Nodes.ToList().FindLastIndex(node => node is PrePullActionNode) >= index;
        var disabled = hasIndicator || hasPrePullActionsAfter;

        if (disabled)
        {
            ImGui.BeginDisabled();
        }

        if (ImGui.MenuItem("Pull indicator"))
        {
            currentRotation.InsertNode(new PullIndicatorNode(), index);
        }

        if (disabled)
        {
            if (ImGuiExt.IsMenuHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(hasIndicator
                               ? "Pull indicator already exists"
                               : "Pull indicator can only be added after all pre pull actions");
                ImGui.EndTooltip();
            }

            ImGui.EndDisabled();
        }
    }

    private void RenderAddBeforeGCDActionOptions()
    {
        if (ImGui.BeginMenu("Add Before"))
        {
            RenderAddPullIndicator(currentIndex);

            if (ImGui.MenuItem("Action"))
            {
                StartSelection(task =>
                {
                    currentRotation.InsertNode(
                        new GCDActionNode { Id = task.Result.RowId },
                        currentIndex);
                });
            }

            if (currentRotation.HasPullIndicator)
            {
                ImGui.BeginDisabled();
            }

            if (ImGui.MenuItem("Pre Pull Action"))
            {
                StartSelection(task =>
                {
                    currentRotation.InsertNode(
                        new PrePullActionNode { Id = task.Result.RowId },
                        currentIndex);
                });
            }

            if (currentRotation.HasPullIndicator)
            {
                ImGui.EndDisabled();

                if (ImGuiExt.IsMenuHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("Can only be added after a pull indicator");
                    ImGui.EndTooltip();
                }
            }

            ImGui.EndMenu();
        }
    }

    private void RenderAddAfterGCDActionOptions()
    {
        var index = currentIndex + 1;
        if (ImGui.BeginMenu("Add After"))
        {
            RenderAddPullIndicator(index);

            if (ImGui.MenuItem("Action"))
            {
                StartSelection(task =>
                {
                    currentRotation.InsertNode(
                        new GCDActionNode() { Id = task.Result.RowId },
                        index);
                });
            }

            if (currentRotation.HasPullIndicator)
            {
                ImGui.BeginDisabled();
            }

            if (ImGui.MenuItem("Pre Pull Action"))
            {
                StartSelection(task =>
                {
                    currentRotation.InsertNode(
                        new PrePullActionNode { Id = task.Result.RowId },
                        index);
                });
            }

            if (currentRotation.HasPullIndicator)
            {
                ImGui.EndDisabled();

                if (ImGuiExt.IsMenuHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("Can only be added after a pull indicator");
                    ImGui.EndTooltip();
                }
            }

            ImGui.EndMenu();
        }
    }
}
