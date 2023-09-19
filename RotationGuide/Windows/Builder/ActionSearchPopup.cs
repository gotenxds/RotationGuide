using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Logging;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace RotationGuide.Windows;

public class ActionSearchPopup
{
    private const int RowHeight = 80;
    private const int ActionsMargin = 50;
    private const int ActionMarginTop = 8;
    private const string ActionSearchDialogId = "ActionSearchDialogId";
    private static readonly Vector2 IconSize = new(64, 64);

    private string searchString = "";
    private bool focusOnSearch = false;

    private List<Action> actions;
    private ClassJob job;

    public event System.Action<Action> OnActionSelected;
    public ClassJob Job
    {
        get => job;
        set
        {
            job = value;
            FilterActions();
        }
    }

    public void Open()
    {
        ImGui.OpenPopup(ActionSearchDialogId);
        focusOnSearch = true;
    }

    public void Render()
    {
        var isOpen = true;

        if (ImGui.BeginPopupModal(ActionSearchDialogId, ref isOpen, ImGuiWindowFlags.NoTitleBar))
        {
            RenderSearch();

            RenderActions();

            ImGui.EndPopup();
        }
    }

    private void RenderSearch()
    {
        if (focusOnSearch)
        {
            focusOnSearch = false;
            ImGui.SetKeyboardFocusHere();
        }
        
        ImGui.SetNextItemWidth(ImGui.GetWindowWidth());
        if (ImGui.InputTextWithHint("", "Action name", ref searchString, 30))
        {
            FilterActions();
        }
    }

    private void FilterActions()
    {
        actions = Plugin.DataManager.GetExcelSheet<Action>()
              .Where(a => !a.IsPvP && a.ClassJob.Value?.Name == Job.Name && a.Name.RawString.ToLower().Contains(searchString.ToLower()))
              .ToList();
    }
    
    private void RenderActions()
    {
        var drawList = ImGui.GetWindowDrawList();
        
        for (var index = 0; index < actions.Count; index++)
        {
            var action = actions[index];

            RenderAction(drawList, action, index);
        }
    }

    private void RenderAction(ImDrawListPtr drawList, Action action, int index)
    {
        var currentRowYPosition = RowHeight * index + ActionsMargin;
        
        ImGui.SetCursorPosY(ActionMarginTop + currentRowYPosition);
        ImGui.Dummy(new Vector2(ImGui.GetWindowWidth(), RowHeight));

        if (ImGui.IsItemHovered())
        {
            drawList.AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(),
                                   ImGui.GetColorU32(ImGuiCol.ButtonHovered));
        }

        if (ImGui.IsItemClicked())
        {
            ImGui.CloseCurrentPopup();
            OnActionSelected.Invoke(action);
        }

        ImGui.SetCursorPosY((ActionMarginTop * 2) + currentRowYPosition);

        ImGui.BeginGroup();

        ImGui.Image(Plugin.TextureProvider.GetIcon(action.Icon).ImGuiHandle, IconSize);
        ImGui.SameLine();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ActionMarginTop + 2);
        ImGui.Text(action.Name);

        ImGui.EndGroup();
    }
}
