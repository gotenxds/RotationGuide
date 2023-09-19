using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;
using LuminaAction = Lumina.Excel.GeneratedSheets.Action;

namespace RotationGuide.Windows;

internal enum BuilderMode
{
    Menu,
    CreateChooseJob,
    Create,
}

public class RotationBuilderWindow : Window, IDisposable
{
    private const int transitionTime = 250;
    private Plugin Plugin;
    private BuilderMode Mode { get; set; }
    private Queue<BuilderMode> History { get; } = new();

    private Dictionary<BuilderMode, Renderer> ModeToRenderer { get; init; }

    private (BuilderMode from, BuilderMode to) UITransitionPair { get; set; }

    private readonly Stopwatch stopwatch = new();

    public RotationBuilderWindow(Plugin plugin) : base("Rotation Builder",
                                                       ImGuiWindowFlags.NoScrollbar |
                                                       ImGuiWindowFlags.NoScrollWithMouse)
    {
        Size = new Vector2(500, 500);
        // SizeCondition = ImGuiCond.FirstUseEver;

        Position = new Vector2(1000, 500);
        PositionCondition = ImGuiCond.Appearing;

        var menu = new MenuRenderer();
        var chooseJobRenderer = new ChooseJobRenderer();
        var rotationRenderer = new RotationPageRenderer();

        ModeToRenderer = new Dictionary<BuilderMode, Renderer>
        {
            { BuilderMode.Menu, menu },
            { BuilderMode.CreateChooseJob, chooseJobRenderer },
            { BuilderMode.Create, rotationRenderer }
        };

        menu.OnGoToCreateChooseJob += () => GoToMode(BuilderMode.CreateChooseJob);
        chooseJobRenderer.OnJobSelected += job =>
        {
            rotationRenderer.Job = job;
            GoToMode(BuilderMode.Create);
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
                    Mode = UITransitionPair.to;
                }
            }
            else if (ModeToRenderer.TryGetValue(Mode, out var renderer))
            {
                renderer.Render();
            }
        }
        catch (Exception e)
        {
            PluginLog.Error(e, "error");
        }
    }


    public void RenderMenu()
    {
        var padding = 50;
        var buttonSize = new Vector2(500, 100);
        var windowSize = ImGui.GetWindowSize();

        var createButtonPos = (windowSize / 2) - (buttonSize / 2);
        ImGui.SetCursorPos(createButtonPos);

        if (ImGui.Button("CREATE", buttonSize))
        {
            GoToMode(BuilderMode.CreateChooseJob);
        }
    }

    private void GoToMode(BuilderMode mode)
    {
        History.Enqueue(Mode);

        StartTransition(Mode, mode);
    }

    private void Back()
    {
        if (History.Count == 0)
        {
            return;
        }

        StartTransition(Mode, History.Dequeue());
    }

    public void RenderBackButton()
    {
        if (ImGui.ArrowButton("BACK", ImGuiDir.Left))
        {
            Back();
        }
    }

    private void StartTransition(BuilderMode prev, BuilderMode next)
    {
        UITransitionPair = (from: prev, to: next);
        stopwatch.Reset();
        stopwatch.Start();
    }
}
