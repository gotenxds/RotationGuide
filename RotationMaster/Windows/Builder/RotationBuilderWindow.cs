using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;
using RotationMaster.Data;
using RotationMaster.Services;
using LuminaAction = Lumina.Excel.GeneratedSheets.Action;

namespace RotationMaster.Windows;

public enum BuilderScreen
{
    Menu = 1,
    CreateChooseJob = 2,
    Create = 3,
}

public class RotationBuilderWindow : Window, IDisposable
{
    public static Router<BuilderScreen> Router = new(BuilderScreen.Menu);
    
    private Plugin Plugin;

    private Dictionary<BuilderScreen, Renderer> ModeToRenderer { get; init; }
    
    public RotationBuilderWindow(Plugin plugin) : base("Rotation Builder", ImGuiWindowFlags.HorizontalScrollbar)
    {
        Size = new Vector2(500, 320);
        SizeCondition = ImGuiCond.FirstUseEver;

        Position = new Vector2(1000, 500);
        PositionCondition = ImGuiCond.Appearing;
        
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(250, 270),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        
        var menu = new MenuRenderer();
        var chooseJobRenderer = new ChooseJobRenderer();
        var rotationPageRenderer = new RotationPageRenderer();

        ModeToRenderer = new Dictionary<BuilderScreen, Renderer>
        {
            { BuilderScreen.Menu, menu },
            { BuilderScreen.CreateChooseJob, chooseJobRenderer },
            { BuilderScreen.Create, rotationPageRenderer }
        };

        RotationListRenderer.OnEditClick += rotation =>
        {
            rotationPageRenderer.Rotation = rotation;
            Router.GoTo(BuilderScreen.Create);
        };
        
        menu.OnGoToCreateChooseJob += () => Router.GoTo(BuilderScreen.CreateChooseJob);
        chooseJobRenderer.OnJobSelected += job =>
        {
            var rotation = new Rotation(job.RowId);
            rotationPageRenderer.Rotation = rotation;
            RotationDataService.Save(rotation);
            
            Router.GoTo(BuilderScreen.Create);
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        try
        {
            if (Router.HasHistory)
            {
                RenderBackButton();
            }
            if (ModeToRenderer.TryGetValue(Router.CurrentScreen, out var renderer))
            {
                renderer.Render();
            }
        }
        catch (Exception e)
        {
            PluginLog.Error(e, "error");
        }
    }
    

    public void RenderBackButton()
    {
        if (ImGui.ArrowButton("BACK", ImGuiDir.Left))
        {
            Router.GoBack();
        }
    }
}
