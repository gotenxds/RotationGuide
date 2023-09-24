using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Interface;
using Dalamud.Logging;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using RotationGuide.Utils;

namespace RotationGuide.Windows;

public class ActionSearchDialog
{
    private const int WindowPadding = 10;
    private const int RowHeight = 80;
    private const int ActionsMargin = 50;
    private const int ActionMarginTop = 8;
    private const string ActionSearchDialogId = "ActionSearchDialogId";
    private static readonly Vector2 IconSize = new(64, 64);

    private string searchString = "";
    private bool focusOnSearch = false;

    private List<Action> actions;
    private ClassJob job;
    private TaskCompletionSource<Action> selectedActionTask;
    private bool isOpen = false;
    
    public static ActionSearchDialog Instance { get; private set; } = new();

    private ActionSearchDialog() { }

    public ClassJob Job
    {
        get => job;
        set
        {
            job = value;
            FilterActions();
        }
    }

    public Task<Action> Open()
    {
        ImGui.OpenPopup(ActionSearchDialogId);
        isOpen = true;
        focusOnSearch = true;

        selectedActionTask = new TaskCompletionSource<Action>();

        return selectedActionTask.Task;
    }

    public void Render()
    {
        var isOpen = true;
        
        ImGui.SetNextWindowPos(ImGui.GetWindowPos());
        ImGui.SetNextWindowSizeConstraints(new Vector2(ImGui.GetWindowSize().X, 200), new Vector2(ImGui.GetWindowSize().X, 500));
        
        if (ImGui.BeginPopupModal(ActionSearchDialogId, ref isOpen, ImGuiWindowFlags.NoTitleBar))
        {
            RenderSearch();

            RenderActions();

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGui.IsAnyItemHovered() && !ImGui.IsWindowAppearing())
            {
                ImGui.CloseCurrentPopup();
            }
            
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
            selectedActionTask.SetResult(action);
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
