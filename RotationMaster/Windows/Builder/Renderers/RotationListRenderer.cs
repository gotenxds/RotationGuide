using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using RotationMaster.Data;
using RotationMaster.Services;
using RotationMaster.Utils;

namespace RotationMaster.Windows;

internal enum RotationTableColumn
{
    Job = 0,
    Name = 1,
    Actions = 2,
}

public class RotationListRenderer : Renderer
{
    private const int JobColumnWidth = 45;
    private const int JobImageSize = 32;
    private const int ActionButtonSize = 40;
    private const int ActionColumnSize = ActionButtonSize * 3 + 20;
    private const string AreYouSurePopupId = "RotationList_AreYouSurePopup";

    private Rotation? rotationToDelete;

    private int currentlyEditedRotationNameIndex = -1;
    private WindowFooter footer;

    public RotationListRenderer()
    {
        footer = new WindowFooter("Tools", RenderFooterActions);
    }

    public static event Action<Rotation> OnEditClick;

    public override void Render(Transition transition = Transition.None, float time = 0)
    {
        var columns = Enum.GetValues<RotationTableColumn>();

        var contentRegionAvail = ImGui.GetContentRegionAvail() - footer.Size with { X = 0 };
        if (ImGui.BeginTable("rotations", columns.Length, ImGuiTableFlags.ScrollY, contentRegionAvail))
        {
            ImGui.TableSetupColumn("Job", ImGuiTableColumnFlags.WidthFixed, JobColumnWidth);
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("##Actions", ImGuiTableColumnFlags.WidthFixed, ActionColumnSize);

            ImGui.TableHeadersRow();

            var rotations = RotationDataService.GetAll().ToArray();
            for (var rotationIndex = 0; rotationIndex < rotations.Length; rotationIndex++)
            {
                var rotation = rotations[rotationIndex];

                RenderRow(rotation, rotationIndex);
            }

            ImGui.EndTable();
        }

        RenderAreYouSurePopup();
        footer.Render();
    }

    private void RenderAreYouSurePopup()
    {
        // This is a workaround for the fact you cant open a popup from within a table (or any other context)
        // https://github.com/ocornut/imgui/issues/331
        if (rotationToDelete != null)
        {
            ImGui.OpenPopup(AreYouSurePopupId);
        }

        var isOpen = true;
        if (ImGui.BeginPopupModal(AreYouSurePopupId, ref isOpen,
                                  ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize))
        {
            ImGuiExt.IndentV(10);
            ImGui.Indent(30);
            ImGui.Text("Delete rotation?");

            ImGuiExt.IndentV(30);
            if (ImGui.Button("CANCEL", new Vector2(100, 50))
                || (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGui.IsAnyItemHovered() &&
                    !ImGui.IsWindowAppearing()))
            {
                rotationToDelete = null;
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine(0, 80);
            
            if (ImGui.Button("YES", new Vector2(100, 50)))
            {
                RotationDataService.Delete(rotationToDelete!);
                rotationToDelete = null;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }

    private void RenderRow(Rotation rotation, int rotationIndex)
    {
        ImGui.TableNextRow();

        foreach (var column in Enum.GetValues<RotationTableColumn>())
        {
            ImGui.TableSetColumnIndex((int)column);

            switch (column)
            {
                case RotationTableColumn.Job:
                    RenderJobCell(rotation);
                    break;
                case RotationTableColumn.Name:
                    RenderNameCell(rotation, rotationIndex);
                    break;
                case RotationTableColumn.Actions:
                    RenderActions(rotation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void RenderActions(Rotation rotation)
    {
        ImGui.Indent(5);
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.PushID($"{rotation.Id}_edit");
        if (ImGui.Button(FontAwesomeIcon.Edit.ToIconString(), Vector2.One * ActionButtonSize))
        {
            OnEditClick?.Invoke(rotation);
        }

        ImGui.PopID();

        ImGui.SameLine();
        ImGui.PushID($"{rotation.Id}_delete");
        if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString(), Vector2.One * ActionButtonSize))
        {
            rotationToDelete = rotation;
        }

        ImGui.PopID();
        
        ImGui.SameLine();
        ImGui.PushID($"{rotation.Id}_export");
        if (ImGui.Button(FontAwesomeIcon.FileExport.ToIconString(), Vector2.One * ActionButtonSize))
        {
            ImGui.SetClipboardText(RotationDataService.Export(rotation));
            Plugin.ChatGui.Print("Rotation exported into clipboard");
        }

        ImGui.PopID();
        ImGui.PopFont();
        ImGui.Unindent(5);
    }

    private static void RenderJobCell(Rotation rotation)
    {
        var textureWrap = Job.GetJob(rotation.JobId).GetIcon();
        var jobColumnWidth = (JobColumnWidth - JobImageSize) / 2f;
        ImGui.Indent(jobColumnWidth);
        ImGui.Image(textureWrap.ImGuiHandle, new Vector2(JobImageSize, JobImageSize));
        ImGui.Unindent(jobColumnWidth);
    }

    private void RenderNameCell(Rotation rotation, int rotationIndex)
    {
        if (currentlyEditedRotationNameIndex != rotationIndex)
        {
            ImGui.Text(rotation.Name == "" ? "Unnamed rotation (click to edit)" : rotation.Name);

            if (ImGui.IsItemClicked())
            {
                currentlyEditedRotationNameIndex = rotationIndex;
            }
        }
        else
        {
            ImGui.SetKeyboardFocusHere();
            var nameRef = rotation.Name;
            if (ImGui.InputText($"##rotation{rotationIndex}name", ref nameRef, 30))
            {
                rotation.Name = nameRef;
            }

            if (ImGui.IsKeyPressed(ImGuiKey.Enter) ||
                (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGui.IsItemHovered()))
            {
                currentlyEditedRotationNameIndex = -1;
                RotationDataService.Save(rotation);
            }
        }
    }

    private void RenderFooterActions()
    {
        if (ImGui.Button("Create"))
        {
            RotationBuilderWindow.GoTo(BuilderScreen.CreateChooseJob);
        }
        ImGui.SameLine();
        if (ImGui.Button("Import"))
        {
            RotationDataService.Import(ImGui.GetClipboardText());
        }
    }
}
