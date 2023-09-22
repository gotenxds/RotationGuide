using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;
using RotationGuide.Data;
using RotationGuide.Services;
using LuminaAction = Lumina.Excel.GeneratedSheets.Action;

namespace RotationGuide.Windows;

internal enum BuilderScreen
{
    Menu,
    CreateChooseJob,
    Create,
}

public class RotationBuilderWindow : Window, IDisposable
{
    private const int transitionTime = 1000;
    private Plugin Plugin;
    private BuilderScreen Screen { get; set; }
    private Queue<BuilderScreen> History { get; } = new();

    private Dictionary<BuilderScreen, Renderer> ModeToRenderer { get; init; }

    private (BuilderScreen from, BuilderScreen to) UITransitionPair { get; set; }

    private readonly Stopwatch stopwatch = new();

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
            GoToMode(BuilderScreen.Create);
        };
        
        menu.OnGoToCreateChooseJob += () => GoToMode(BuilderScreen.CreateChooseJob);
        chooseJobRenderer.OnJobSelected += job =>
        {
            var rotation = new Rotation(job.RowId);
            rotationPageRenderer.Rotation = rotation;
            RotationDataService.Save(rotation);
            
            GoToMode(BuilderScreen.Create);
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        try
        {
            if (History.Count != 0)
            {
                RenderBackButton();
            }

            if (stopwatch.IsRunning)
            {
                var time = (float)stopwatch.ElapsedMilliseconds / transitionTime;

                ModeToRenderer[UITransitionPair.from].Render(Transition.Out, time);
                ModeToRenderer[UITransitionPair.to].Render(Transition.In, time);

                if (time >= 1)
                {
                    stopwatch.Reset();
                    Screen = UITransitionPair.to;
                }
            }
            else if (ModeToRenderer.TryGetValue(Screen, out var renderer))
            {
                renderer.Render();
            }
        }
        catch (Exception e)
        {
            PluginLog.Error(e, "error");
        }
    }

    private void GoToMode(BuilderScreen screen)
    {
        History.Enqueue(Screen);

        // TODO: maybe one day make trasitions work 
        // StartTransition(Screen, screen);
        
        Screen = screen;
    }

    private void Back()
    {
        if (History.Count == 0)
        {
            return;
        }

        // StartTransition(Screen, History.Dequeue());
        Screen = History.Dequeue();
    }

    public void RenderBackButton()
    {
        if (ImGui.ArrowButton("BACK", ImGuiDir.Left))
        {
            Back();
        }
    }

    private void StartTransition(BuilderScreen prev, BuilderScreen next)
    {
        UITransitionPair = (from: prev, to: next);
        stopwatch.Reset();
        stopwatch.Start();
    }
}
