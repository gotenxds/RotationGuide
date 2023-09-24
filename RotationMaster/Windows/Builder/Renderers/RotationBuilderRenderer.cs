using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Logging;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using RotationMaster.Data;
using RotationMaster.Services;
using RotationMaster.Utils;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace RotationMaster.Windows;

public class RotationPageRenderer : Renderer
{
    public Rotation Rotation
    {
        get => rotation;
        set
        {
            if (rotation != null)
            {
                rotation.OnRotationChanged -= OnRotationChange;
            }

            rotation = value;
            rotation.OnRotationChanged += OnRotationChange;
            RotationRenderer.Rotation = rotation;
            ActionSearchDialog.Instance.Job = Job;
        }
    }

    public ClassJob Job => Data.Job.GetJob(rotation.JobId);

    private RotationRenderer RotationRenderer { get; init; }
    private Rotation rotation;
    private bool isEditingName = false;
    private BuilderFooter BuilderFooter { get; init; }

    public RotationPageRenderer()
    {
        BuilderFooter = new BuilderFooter(() => !Rotation.HasPullIndicator);
        RotationRenderer = new RotationRenderer();

        BuilderFooter.OnAddActionClicked += () => OnAddActionClicked(ActionType.GCD);
        BuilderFooter.OnAddPrePullActionClicked += () => OnAddActionClicked(ActionType.PREPULL);
        BuilderFooter.OnAddPullBarClicked += () => rotation.AddPullIndicator();

        RotationRenderer.OnActionClick += async actionClickEventArgs =>
        {
            var action = await ActionSearchDialog.Instance.Open();

            OnActionSelected(action, actionClickEventArgs.Index, actionClickEventArgs.Type);
        };

        RotationRenderer.OnOGCDClick += async actionClickEventArgs =>
        {
            var action = await ActionSearchDialog.Instance.Open();

            OnOGCDSelected(action, actionClickEventArgs.Index, actionClickEventArgs.InnerIndex);
        };
    }

    private async void OnAddActionClicked(ActionType type)
    {
        var action = await ActionSearchDialog.Instance.Open();
        OnActionSelected(action, -1, type);
    }

    private void OnRotationChange(Rotation rotation)
    {
        RotationDataService.Save(rotation);
    }

    private void OnActionSelected(Action selectedAction, int actionIndexToReplace, ActionType actionType)
    {
        if (actionIndexToReplace != -1)
        {
            var action = (IActionNode)rotation.Nodes[actionIndexToReplace];
            action.Id = selectedAction.RowId;

            rotation.ReplaceActionNode(actionIndexToReplace, action);
        }
        else
        {
            IActionNode actionNode = actionType switch
            {
                ActionType.GCD => new GCDActionNode(),
                ActionType.PREPULL => new PrePullActionNode(),
                _ => throw new ArgumentException()
            };

            actionNode.Id = selectedAction.RowId;
            rotation.AddAction(actionNode);
        }
    }

    private void OnOGCDSelected(Action selectedAction, int index, int innerIndex)
    {
        rotation.UpdateOgcdNode(index, innerIndex, selectedAction.RowId);
    }

    public override void Render(Transition transition = Transition.None, float time = 0)
    {
        if (Job == null)
        {
            return;
        }

        ImGui.SetCursorPosY(100 + BaseCursorHeight);

        ImGui.BeginGroup();
        ImGui.Indent(50);

        RenderTitle();

        ImGuiExt.IndentV(100);

        RenderRotation();

        ImGui.EndGroup();

        BuilderFooter.Render();
    }

    private void RenderRotation()
    {
        if (rotation.Nodes.Length == 0)
        {
            var windowSize = ImGui.GetWindowSize();
            var windowPos = ImGui.GetWindowPos();
            var windowCenter = windowPos + (windowSize / 2) - new Vector2(265, 25);

            var imDrawListPtr = ImGui.GetWindowDrawList();
            imDrawListPtr.AddText(Fonts.Axis32.ImFont, 84, windowCenter, Colors.FadedText, "TIME TO CREATE");
            imDrawListPtr.AddText(Fonts.Axis32.ImFont, 32, windowCenter + new Vector2(50, 70), Colors.FadedText,
                                  "Use one of the tools below to start");
        }
        else
        {
            RotationRenderer.Render();
        }
    }

    private void RenderTitle()
    {
        var jobTextureWrap = Plugin.TextureProvider.GetIcon(Job.RowId + 100 + 62000u);
        
        ImGui.BeginGroup();
        
        ImGui.Image(jobTextureWrap!.ImGuiHandle, new Vector2(jobTextureWrap.Width, jobTextureWrap.Height));
        ImGui.SameLine();
        Fonts.WriteWithFont(Fonts.Jupiter23, $"{Job.Name} -");
        ImGui.SameLine();

        if (isEditingName)
        {
            ImGui.SetKeyboardFocusHere();
            ImGuiExt.IndentV(15);
            ImGui.SetNextItemWidth(300);
            var nameRef = rotation.Name;
            if (ImGui.InputTextWithHint("##RotationName", "Rotation name", ref nameRef, 30))
            {
                rotation.Name = nameRef;
            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGui.IsItemHovered())
            {
                isEditingName = false;
            }
        }
        else
        {
            Fonts.WriteWithFont(Fonts.Jupiter23, rotation.Name != "" ? rotation.Name : "Rotation name");

            if (ImGui.IsItemClicked())
            {
                isEditingName = true;
            }
        }

        ImGui.EndGroup();
    }
}
